/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Utils;
using Debug = UnityEngine.Debug;

/// <summary>
/// Handles all file importing into nanoVR.
/// </summary>
public class FileImport : MonoBehaviour
{
    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas loadingMenu;
    private Canvas fileBrowser;
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private Toggle boundExtensionTog;
    public static FileImport Instance;

    private const string PLANE = "XY";
    private const int MAX_STRAND_NUCLEOTIDES = 20000;
    private const int MAX_HELIX_NUCLEOTIDES = 30000;

    private const string DEFAULT_GRID_NAME = "default_group";
    private const int SPACING = 10;

    private static readonly List<Vector2> candidateOffsets = new List<Vector2>
    {
        new Vector2(1, 0),
        new Vector2(-1, 0),
        new Vector2(0, 1),
        new Vector2(0, -1),
        new Vector2(1, 1),
        new Vector2(-1, -1),
        new Vector2(-1, 1),
        new Vector2(1, -1),
        new Vector2(2, 0),
        new Vector2(0, 2),
        new Vector2(-2, 0),
        new Vector2(0, -2)
    };


    void Awake()
    {
        FileBrowser.HideDialog();
        FileBrowser.SingleClickMode = true;
        FileBrowser.Instance.enabled = false;
        loadingMenu.enabled = false;
        Instance = this;

#if UNITY_ANDROID
        typeof(SimpleFileBrowser.FileBrowserHelpers).GetField("m_shouldUseSAF", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, (bool?)false);
#endif

        // Code to get all file access on Oculus Quest 2 
        using var buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
        using var buildCodes = new AndroidJavaClass("android.os.Build$VERSION_CODES");
        //Check SDK version > 29
        if (buildVersion.GetStatic<int>("SDK_INT") > buildCodes.GetStatic<int>("Q"))
        {
            using var environment = new AndroidJavaClass("android.os.Environment");
            //сhecking if permission already exists
            if (!environment.CallStatic<bool>("isExternalStorageManager"))
            {
                using var settings = new AndroidJavaClass("android.provider.Settings");
                using var uri = new AndroidJavaClass("android.net.Uri");
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var parsedUri = uri.CallStatic<AndroidJavaObject>("parse", $"package:{Application.identifier}");
                using var intent = new AndroidJavaObject("android.content.Intent",
                    settings.GetStatic<string>("ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION"),
                    parsedUri);
                currentActivity.Call("startActivity", intent);
            }
        }
    }

    private void Start()
    {
        fileBrowser = FileBrowser.Instance.GetComponent<Canvas>();
        fileBrowser.gameObject.SetActive(false);
    }

    public void OpenFile()
    {
        if (fileBrowser == null) 
        {
            fileBrowser = FileBrowser.Instance.GetComponent<Canvas>();
        } 

        // enable file browser, disable menu
        fileBrowser.gameObject.SetActive(true);
        Menu.enabled = false;

        FileBrowser.Instance.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
        FileBrowser.ShowLoadDialog((paths) => { LoadFile(paths[0]); },
                              () => { }, //Debug.Log("Canceled"); },
                              FileBrowser.PickMode.Files, false, null, null, "Select File", "Select");
    }

    private void LoadFile(string selectedFilePath)
    {
        if (string.IsNullOrEmpty(selectedFilePath))
        {
            Debug.Log("No file selected.");
        }

        if (File.Exists(selectedFilePath))
        {
            StreamReader sr = File.OpenText(selectedFilePath);
            string fileContent = sr.ReadToEnd();
            string fileType = FileBrowser.GetExtensionFromFilename(selectedFilePath, false);
            try
            {
                if (fileType.Equals(".sc") || fileType.Equals(".sc.txt"))
                {
                    DoFileImport(fileContent);
                }
                else if (fileType.Equals(".oxview"))
                {
                    // Parse oxview JSON
                    loadingMenu.enabled = true;
                    OxViewImport(fileContent);
                    loadingMenu.enabled = false;
                }
                else if (fileType.Equals(".pdb"))
                {
                    loadingMenu.enabled = true;
                    PDBImport.ParseAndVisualizePDB(selectedFilePath);
                }
                else
                {
                    Debug.Log("Don't support file type: " + fileType);
                }
                loadingMenu.enabled = false;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                loadingMenu.enabled = false;
            }
        }
        else
        {
            Debug.Log("File not found.");
        }
    }

    private void DoFileImport(string json)
    {
        ICommand command = new ImportCommand(json);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Parses through a scadnano file and draws out all Grids, Helices, and Strands.
    /// Also called when copy and pasting a Grid or converting the entire scene into visual mode (expanding out mutations).
    /// </summary>
    /// <param name="fileContents">Contents of scadnano file</param>
    /// <param name="isCopyPaste">Whether this is being called for copy/pasting a grid</param>
    /// <param name="visualMode">Whether this is being called for converting to visual mode</param>
    /// <returns>List of grids created</returns>
    public List<DNAGrid> ParseSC(string fileContents, bool isCopyPaste = false)
    {
        loadingMenu.enabled = true;
        List<DNAGrid> grids = new List<DNAGrid>();
        JObject origami = JObject.Parse(fileContents);
        JArray helices = JArray.Parse(origami["helices"].ToString());
        JArray strands = JArray.Parse(origami["strands"].ToString());
        bool isMultiGrid = false;
        Vector3 gridPosition = rayInteractor.transform.position;
        Dictionary<DNAGrid, Vector3> gridRotations = new Dictionary<DNAGrid, Vector3>();

        // Parse grids
        if (origami["groups"] != null)
        {
            JObject groupObject = JObject.Parse(origami["groups"].ToString());
            Dictionary<string, JObject> groups = groupObject.ToObject<Dictionary<string, JObject>>();
            isMultiGrid = groups.Count > 1; 

            foreach (var item in groups)
            {
                try
                {
                    string origName = CleanSlash(item.Key);
                    string gridName = origName;
                    gridName = GetGridName(origName);
                    UpdateGridCopies(origName);
                    
                    JObject info = item.Value;
                    float x = 0;
                    float y = 0;
                    float z = 0;
                    if (info["position"] != null)
                    {
                        x = (float)info["position"]["x"] / SCALE_FROM_NANOVR_TO_NM;
                        y = (float)info["position"]["y"] / SCALE_FROM_NANOVR_TO_NM;
                        z = (float)info["position"]["z"] / SCALE_FROM_NANOVR_TO_NM;
                    }
                    
                    string gridType = CleanSlash(info["grid"].ToString());
                    Vector3 startPos;
                    startPos = new Vector3(x, y, z);

                    if (isCopyPaste)
                    {
                        startPos = rayInteractor.transform.position;
                    }
                    else
                    {
                        startPos = new Vector3(x, y, z);
                    }
                    DNAGrid grid = DrawGrid.CreateGrid(gridName, PLANE, startPos, gridType);
                    grids.Add(grid);

                    // Handle rotation
                    float pitch = 0f;
                    float roll = 0f;
                    float yaw = 0f;
                    if (info["pitch"] != null)
                    {
                        pitch = (float)info["pitch"];
                    }
                    if (info["roll"] != null)
                    {
                        roll = (float)info["roll"];
                    }
                    if (info["yaw"] != null)
                    {
                        yaw = (float)info["yaw"];
                    }
                    if (pitch != 0f || roll != 0f || yaw != 0f)
                    {
                        //Quaternion q = ScadnanoRPYToUnity(roll, pitch, yaw);
                        gridRotations.Add(grid, new Vector3(pitch, -yaw, roll));
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }
        else
        {
            string gridType = CleanSlash(origami["grid"].ToString());
            DNAGrid grid = DrawGrid.CreateGrid(s_numGrids.ToString(), PLANE, gridPosition, gridType);
            grids.Add(grid);
        }
        
        // Parse helices.
        int lastHelixId = s_numHelices;
        Dictionary<int, int> oldHelixToNewHelixMap = new Dictionary<int, int>();
        ParseHelices(helices, isMultiGrid, gridPosition, oldHelixToNewHelixMap);

        // Rotate grid circles and helices
        RotateGrids(gridRotations);

        // Parse strands.
        CoRunner.Instance.Run(ParseStrands(strands, lastHelixId, new List<Strand>(), oldHelixToNewHelixMap));
        return grids;
    }

    private void RotateGrids(Dictionary<DNAGrid, Vector3> gridRotations)
    {
        if (gridRotations.Count == 0)
        {
            return;
        }

        foreach (var item in gridRotations)
        {
            DNAGrid grid = item.Key;
            Vector3 q = item.Value;
            TransformHandle.ShowTransform(grid, grid.GetTransform().position);
            TransformHandle.AttachChildren(new List<DNAGrid> { grid });
            TransformHandle.GizmosTransform.Rotate(q, Space.World); 
            TransformHandle.DetachChildren(new List<DNAGrid> { grid });
        }
    }

    private void ParseHelices(JArray helices, bool isMultiGrid, Vector3 gridPosition, Dictionary<int, int> oldHelixToNewHelixMap)
    {
        int startHelixId = s_numHelices;
        for (int i = 0; i < helices.Count; i++)
        {
            if (helices[i]["grid_position"] != null)
            {
                ParseHelix(helices[i], isMultiGrid, startHelixId, oldHelixToNewHelixMap);
            }
            else
            {
                ParseNoneHelix(helices[i], isMultiGrid, gridPosition, oldHelixToNewHelixMap);
            }
        }
    }

    /// <summary>
    /// Parse and draw Helices from scadnano file
    /// </summary>
    private void ParseHelix(JToken fileHelix, bool isMultiGrid, int startHelixId, Dictionary<int, int> oldHelixToNewHelixMap)
    {
        JArray coord = JArray.Parse(fileHelix["grid_position"].ToString());
        int length = (int)fileHelix["max_offset"];
        int helixId = s_numHelices;

        // Map helixId from file if available
        if (fileHelix["idx"] != null)
        {
            int oldHelixId = (int)fileHelix["idx"];
            oldHelixToNewHelixMap.Add(oldHelixId, helixId);
        }

        string gridName;
        if (fileHelix["group"] != null)
        {
            string origName = CleanSlash(fileHelix["group"].ToString());
            gridName = GetGridName(origName, true);
        }
        else if (isMultiGrid)
        {
            string origName = DEFAULT_GRID_NAME;
            gridName = GetGridName(origName, true);
        }
        else
        {
            gridName = (s_numGrids - 1).ToString();
        }

        DNAGrid grid = s_gridDict[gridName];
        int xGrid = (int) coord[0];
        int yGrid = (int) coord[1] * -1;

        // Expands grid if necessary so that helix coordinates exist.
        GridPoint minBound = grid.MinimumBound;
        GridPoint maxBound = grid.MaximumBound;
        while (xGrid <= minBound.X)
        {
            grid.ExpandWest();
        }
        while (xGrid >= maxBound.X)
        {
            grid.ExpandEast();
        }
        while (yGrid <= minBound.Y)
        {
            grid.ExpandSouth();
        }
        while (yGrid >= maxBound.Y)
        {
            grid.ExpandNorth();
        }

        try
        {
            int xInd = grid.GridXToIndex(xGrid);
            int yInd = grid.GridYToIndex(yGrid);
            GridComponent gc = grid.Grid2D[xInd, yInd];
            Helix helix = grid.AddHelix(helixId, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), length, PLANE, gc);
            helix.Extend(length);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void ParseNoneHelix(JToken fileHelix, bool isMultiGrid, Vector3 gridPosition, Dictionary<int, int> oldHelixToNewHelixMap)
    {
        int length = (int)fileHelix["max_offset"];

        float x = (float)fileHelix["position"]["x"] / SCALE_FROM_NANOVR_TO_NM;
        float y = (float)fileHelix["position"]["y"] * -1 / SCALE_FROM_NANOVR_TO_NM;
        float z = (float)fileHelix["position"]["z"] / SCALE_FROM_NANOVR_TO_NM;

        int helixId = s_numHelices;

        string gridName;
        if (fileHelix["group"] != null)
        {
            string origName = CleanSlash(fileHelix["group"].ToString());
            gridName = GetGridName(origName, true);
        }
        else if (isMultiGrid)
        {
            string origName = DEFAULT_GRID_NAME;
            gridName = GetGridName(origName, true);
        }
        else
        {
            gridName = (s_numGrids - 1).ToString();
        }

        NoneGrid grid = (NoneGrid)s_gridDict[gridName];
        GridPoint gp = new GridPoint(x, y, z);
        Vector3 finalPosition = gridPosition + new Vector3(x, y, z);

        GameObject go = grid.CreateNoneGridCircle(gp, finalPosition);
        GridComponent gc = go.GetComponent<GridComponent>();

        Helix helix = grid.AddHelix(helixId, finalPosition, length, PLANE, gc);
        helix.Extend(length);
    }

    /// <summary>
    /// Coroutine to parse and draw Strands from scadnano file.
    /// Also handles converting to Strand view if necessary. We do this 
    /// here since Coroutines are async and we must wait for all Strands
    /// to be parsed before going to next steps.
    /// </summary>
    public IEnumerator ParseStrands(JArray strands, int lastHelixId, List<Strand> newStrands, Dictionary<int, int> oldHelixToNewHelixMap)
    {
        // Maps strand index in .sc file to strandId in nanoVR
        Dictionary<int, int> extensionStrands = new Dictionary<int, int>();

        // Drawing strands
        for (int i = 0; i < strands.Count; i++)
        {
            JArray domains = JArray.Parse(strands[i]["domains"].ToString());
            string sequence = "";
            if (strands[i]["sequence"] != null)
            {
                sequence = CleanSlash(strands[i]["sequence"].ToString());
            }
            string hexColor = CleanSlash(strands[i]["color"].ToString());
            ColorUtility.TryParseHtmlString(hexColor, out Color color);
            int strandId = s_numStrands;
            bool isScaffold = false;
            if (strands[i]["is_scaffold"] != null)
            {
                isScaffold = (bool) strands[i]["is_scaffold"];
            }
            bool isCircular = false;
            if (strands[i]["circular"] != null)
            {
                isCircular = (bool) strands[i]["circular"];
            }

            Dictionary<int, int> loopouts = new Dictionary<int, int>(); // (domainIdx of previous domain, loopoutLength)
            List<Domain> strandDomains = new List<Domain>();

            for (int j = 0; j < domains.Count; j++)
            {
                if (domains[j]["helix"] != null)
                {
                    int helixId = (int)domains[j]["helix"];// + lastHelixId;
                    if (oldHelixToNewHelixMap.ContainsKey(helixId))
                    {
                        helixId = oldHelixToNewHelixMap[helixId];
                    }
                    else
                    {
                        helixId += lastHelixId;
                    }

                    bool forward = (bool) domains[j]["forward"];
                    int startId = (int) domains[j]["start"];
                    int endId = (int) domains[j]["end"] - 1; // End id is exclusive in .sc file
                    JArray deletions = new JArray();
                    JArray insertions = new JArray();
                    if (domains[j]["deletions"] != null) { deletions = JArray.Parse(domains[j]["deletions"].ToString()); }
                    if (domains[j]["insertions"] != null) { insertions = JArray.Parse(domains[j]["insertions"].ToString()); }
                    Helix helix = s_helixDict[helixId];

                    // Store domains of strand.
                    try
                    {
                        Dictionary<int, int> domainInsertions = insertions.Children<JArray>().ToDictionary(inner => (int)inner[0], inner => (int)inner[1]);
                        List<int> domainDeletions = deletions.Select(j => j.Value<int>()).ToList();
                        Domain domain = new Domain(helixId, Convert.ToInt32(forward), startId, endId, domainInsertions, domainDeletions);
                        strandDomains.Add(domain);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Exception when getting domain nucleotides / storing xover endpoints");
                        Debug.Log(e.Message);
                    }
                }
                else if (domains[j]["loopout"] != null)
                {
                    int loopoutLength = (int) domains[j]["loopout"];
                    int domainIdx = strandDomains.Count - 1;
                    loopouts[domainIdx] = loopoutLength;
                }
                else if (domains[j]["extension_num_bases"] != null)
                {
                    // Save strands with extensions so that we can parse them after other strands
                    //int extensionLength = (int)domains[j]["extension_num_bases"];
                    //extensionStrands.Add(i, strandId);
                    Helix helix;
                    int helixId;
                    if (j == 0)
                    {
                        helixId = (int)domains[j + 1]["helix"] + lastHelixId;
                        helix = s_helixDict[helixId];
                    }
                    else
                    {
                        helixId = (int)domains[j - 1]["helix"] + lastHelixId;
                        helix = s_helixDict[helixId];
                    }

                    DNAGrid grid = s_gridDict[helix.GridId];

                    if (boundExtensionTog.isOn)
                    {
                        extensionStrands.Add(i, strandId);
                    }
                    else
                    {
                        int extensionLength = (int)domains[j]["extension_num_bases"];
                        int direction;
                        Vector3 lastPos;
                        Vector3 secondToLastPos;
                        if (j == 0)
                        {
                            helixId = (int)domains[j + 1]["helix"] + lastHelixId;
                            bool forward = (bool)domains[j + 1]["forward"];
                            direction = Convert.ToInt32(forward);
                            int startId = (int)domains[j + 1]["start"];
                            lastPos = helix.GetNucleotideData(startId, direction).GetPosition();
                            secondToLastPos = helix.GetNucleotideData(startId + 1, direction).GetPosition();
                        }
                        else
                        {
                            helixId = (int)domains[j - 1]["helix"] + lastHelixId;
                            bool forward = (bool)domains[j - 1]["forward"];
                            direction = Convert.ToInt32(forward);
                            int endId = (int)domains[j - 1]["end"];
                            lastPos = helix.GetNucleotideData(endId, direction).GetPosition();
                            secondToLastPos = helix.GetNucleotideData(endId - 1, direction).GetPosition();
                        }
                        Extension ext = new Extension(helixId, direction, extensionLength, lastPos, secondToLastPos);
                        helix.Extensions.Add(ext);
                        strandDomains.Add(ext);
                    }
                }
                else
                {
                    Debug.Log($"Cannot parse SC file: {domains[j]}");
                }
            }

            //Strand strand = CreateStrand(nucleotides, strandId, color, sInsertions, sDeletions, sequence, isScaffold);
            Strand strand = CreateStrand(strandDomains, strandId, color, isScaffold, loopouts);
            if (isCircular)
                strand.IsCircular = true;

            if (!extensionStrands.ContainsKey(i))
            {
                strand.SetSequenceRevamp(sequence);
            }
            newStrands.Add(strand);

            yield return null;
        }

        foreach (var item in extensionStrands)
        {
            int strandIndex = item.Key;
            int strandId = item.Value;
            Strand strand = s_strandDict[strandId];

            JArray domains = JArray.Parse(strands[strandIndex]["domains"].ToString());
            string sequence = strands[strandIndex]["sequence"]?.ToString() ?? "";

            for (int j = 0; j < domains.Count; j++)
            {
                if (domains[j]["extension_num_bases"] == null) continue;

                int extensionLength = (int)domains[j]["extension_num_bases"];

                bool isHead = j == 0;
                bool isTail = j == domains.Count - 1;
                if (!isHead && !isTail) continue;

                int neighborIndex = isHead ? j + 1 : j - 1;
                JObject neighborDomain = (JObject)domains[neighborIndex];

                int helixId = (int)neighborDomain["helix"] + lastHelixId;
                Helix baseHelix = s_helixDict[helixId];
                GridComponent baseGC = baseHelix.GridComponent;

                bool baseForward = (bool)neighborDomain["forward"];
                int startId = (int)neighborDomain["start"];
                int endId = (int)neighborDomain["end"] - 1;

                bool drawn = TryDrawExtension(
                    baseGC.Grid,
                    baseGC,
                    strand,
                    startId,
                    endId,
                    !baseForward,
                    extensionLength,
                    isHead
                );

                if (!drawn)
                {
                    Debug.LogWarning($"Could not draw {(isHead ? "5'" : "3'")} extension for strand {strand.Id}");
                }
            }

            strand.SetSequenceRevamp(CleanSlash(sequence));
            Utils.CheckMismatch(strand); // Check for mismatches after setting sequence
            yield return null;
        }
        loadingMenu.enabled = false;
    }

    private bool TryDrawExtension(
        DNAGrid grid,
        GridComponent baseGC,
        Strand strand,
        int nextStartId,
        int nextEndId,
        bool forward,
        int extensionLength,
        bool isHead)
    {
        foreach (var offset in candidateOffsets)
        {
            float dx = offset.x;
            float dy = offset.y;

            bool success = isHead
                ? DrawHeadExtension(grid, baseGC, strand, nextStartId, nextEndId, forward, extensionLength, dx, dy)
                : DrawTailExtension(grid, baseGC, strand, nextStartId, nextEndId, forward, extensionLength, dx, dy);

            if (success)
                return true;
        }

        return false;
    }

    private bool DrawHeadExtension(DNAGrid grid, GridComponent domainGC, Strand strand,
    int nextStartId, int nextEndId, bool forward, int extensionLength, float dx, float dy)
    {
        if (grid != null && grid.Type != "none")
        {
            return TryGridBasedHeadExtension(grid, domainGC, strand, nextStartId, nextEndId, forward, extensionLength, (int) dx, (int) dy);
        }
        else
        {
            return TryNoGridHeadExtension((NoneGrid)grid, domainGC, strand, nextStartId, nextEndId, forward, extensionLength, dx, dy);
        }
    }

    private bool TryGridBasedHeadExtension(DNAGrid grid, GridComponent domainGC, Strand strand,
    int nextStartId, int nextEndId, bool forward, int extensionLength, int dx, int dy)
    {
        int newX = domainGC.GridPoint.X + dx;
        int newY = domainGC.GridPoint.Y + dy;
        int xIndex = grid.GridXToIndex(newX);
        int yIndex = grid.GridYToIndex(newY);
        if (xIndex == -1 || yIndex == -1) return false;

        GridComponent gc = grid.Grid2D[xIndex, yIndex];
        if (gc == null) return false;

        Helix helix = gc.Helix;
        int length = Math.Max(nextStartId + extensionLength, nextEndId + 1);
        int num64 = length / 64 + 1;
        int actualLength = num64 * 64;

        if (helix == null)
        {
            helix = grid.AddHelix(s_numHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), actualLength, PLANE, gc);
            helix.Extend(actualLength);
            grid.CheckExpansion(gc);
        }
        else if (actualLength > helix.Length)
        {
            helix.Extend(actualLength - helix.Length);
        }

        int startId = forward ? nextEndId - extensionLength + 1 : nextStartId;
        int endId = forward ? nextEndId : nextStartId + extensionLength - 1;

        if (!Utils.IsValidDomain(helix, startId, endId, Convert.ToInt32(forward)))
            return false;

        Domain domain = new Domain(helix.Id, Convert.ToInt32(forward), startId, endId,
            new Dictionary<int, int>(), new List<int>())
        { IsExtension = true };

        strand.AddToHead(domain);
        strand.SetDomainsRevamp();
        DrawCrossover.CreateXoverHelper(domain, strand.GetDomain(1), strand.Id, strand.Color, strand.Color);
        return true;
    }

    private bool TryNoGridHeadExtension(NoneGrid grid, GridComponent domainGC, Strand strand,
                        int nextStartId, int nextEndId, bool forward, int extensionLength, float dx, float dy)
    {
        Vector3 basePos = domainGC.transform.position;
        Vector3 offset = new Vector3(dx, dy, 0) * SPACING / SCALE_FROM_NANOVR_TO_NM; ;
        Vector3 candidatePos = basePos + offset;

        if (grid.GridComponents.Any(gc => Vector3.Distance(gc.transform.position, candidatePos) < SPACING))
            return false;

        int length = Math.Max(nextStartId + extensionLength, nextEndId + 1);
        int num64 = length / 64 + 1;
        int actualLength = num64 * 64;

        int startId = forward ? nextEndId - extensionLength + 1 : nextStartId;
        int endId = forward ? nextEndId : nextStartId + extensionLength - 1;

        foreach (GridComponent gc in grid.GridComponents)
        {
            if (actualLength > gc.Helix.Length)
            {
                gc.Helix.Extend(actualLength - gc.Helix.Length);
            }

            if (Utils.IsValidDomain(gc.Helix, startId, endId, Convert.ToInt32(forward)))
            {
                Domain domain = new Domain(gc.Helix.Id, Convert.ToInt32(forward), startId, endId, new Dictionary<int, int>(), new List<int>())
                { IsExtension = true };

                strand.AddToHead(domain);
                strand.SetDomainsRevamp();
                DrawCrossover.CreateXoverHelper(domain, strand.GetDomain(1), strand.Id, strand.Color, strand.Color);
                return true;
            }
        }

        GridPoint gp = new GridPoint(candidatePos.x, candidatePos.y, candidatePos.z);

        GameObject go = grid.CreateNoneGridCircle(gp, candidatePos);
        GridComponent newGC = go.GetComponent<GridComponent>();

        Helix newHelix = grid.AddHelix(s_numHelices, candidatePos, actualLength, PLANE, newGC);
        newHelix.Extend(actualLength);

        if (Utils.IsValidDomain(newHelix, startId, endId, Convert.ToInt32(forward)))
        {
            Domain domain = new Domain(newHelix.Id, Convert.ToInt32(forward), startId, endId,
            new Dictionary<int, int>(), new List<int>())
            { IsExtension = true };

            strand.AddToHead(domain);
            strand.SetDomainsRevamp();
            DrawCrossover.CreateXoverHelper(domain, strand.GetDomain(1), strand.Id, strand.Color, strand.Color);
            return true;
        }

        return false;
    }

    private bool TryGridBasedTailExtension(DNAGrid grid, GridComponent domainGC, Strand strand,
    int nextStartId, int nextEndId, bool forward, int extensionLength, int dx, int dy)
    {
        int newX = domainGC.GridPoint.X + dx;
        int newY = domainGC.GridPoint.Y + dy;
        int xIndex = grid.GridXToIndex(newX);
        int yIndex = grid.GridYToIndex(newY);
        if (xIndex == -1 || yIndex == -1) return false;

        GridComponent gc = grid.Grid2D[xIndex, yIndex];
        if (gc == null) return false;

        Helix helix = gc.Helix;
        int length = Math.Max(nextStartId + extensionLength, nextEndId + 1);
        int num64 = length / 64 + 1;
        int actualLength = num64 * 64;

        if (helix == null)
        {
            helix = grid.AddHelix(s_numHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), actualLength, PLANE, gc);
            helix.Extend(actualLength);
            grid.CheckExpansion(gc);
        }
        else if (actualLength > helix.Length)
        {
            helix.Extend(actualLength - helix.Length);
        }

        int startId = forward ? nextStartId : nextEndId - extensionLength + 1;
        int endId = forward ? nextStartId + extensionLength - 1 : nextEndId;

        if (!Utils.IsValidDomain(helix, startId, endId, Convert.ToInt32(forward)))
            return false;

        Domain domain = new Domain(helix.Id, Convert.ToInt32(forward), startId, endId,
            new Dictionary<int, int>(), new List<int>())
        { IsExtension = true };

        strand.AddToTail(domain);
        strand.SetDomainsRevamp();
        DrawCrossover.CreateXoverHelper(strand.GetDomain(strand.Domains.Count - 2), domain, strand.Id, strand.Color, strand.Color);
        return true;
    }


    private bool TryNoGridTailExtension(NoneGrid grid, GridComponent domainGC, Strand strand,
    int nextStartId, int nextEndId, bool forward, int extensionLength, float dx, float dy)
    {
        Vector3 basePos = domainGC.transform.position;
        Vector3 offset = new Vector3(dx, dy, 0) * SPACING / SCALE_FROM_NANOVR_TO_NM; ;
        Vector3 candidatePos = basePos + offset;

        /*if (grid.GridComponents.Any(gc => Vector3.Distance(gc.transform.position, candidatePos) < SPACING))
            return false;*/

        int length = Math.Max(nextStartId + extensionLength, nextEndId + 1);
        int num64 = length / 64 + 1;
        int actualLength = num64 * 64;

        int startId = forward ? nextStartId : nextEndId - extensionLength + 1;
        int endId = forward ? nextStartId + extensionLength - 1 : nextEndId;

        // Try to find existing helix first
        foreach (GridComponent gc in grid.GridComponents)
        {
            if (actualLength > gc.Helix.Length)
            {
                gc.Helix.Extend(actualLength - gc.Helix.Length);
            }
            if (Utils.IsValidDomain(gc.Helix, startId, endId, Convert.ToInt32(forward)))
            {
                Domain domain = new Domain(gc.Helix.Id, Convert.ToInt32(forward), startId, endId, new Dictionary<int, int>(), new List<int>())
                { IsExtension = true };

                strand.AddToTail(domain);
                strand.SetDomainsRevamp();
                DrawCrossover.CreateXoverHelper(strand.GetDomain(strand.Domains.Count - 2), domain, strand.Id, strand.Color, strand.Color);
                return true;
            }
        }

        // If we couldn't find existing helix, dynamically create a new one.
        GridPoint gp = new GridPoint(candidatePos.x, candidatePos.y, candidatePos.z);

        GameObject go = grid.CreateNoneGridCircle(gp, candidatePos);
        GridComponent newGC = go.GetComponent<GridComponent>();

        Helix newHelix = grid.AddHelix(s_numHelices, candidatePos, actualLength, PLANE, newGC);
        newHelix.Extend(actualLength);

        if (Utils.IsValidDomain(newHelix, startId, endId, Convert.ToInt32(forward)))
        {
            Domain domain = new Domain(newHelix.Id, Convert.ToInt32(forward), startId, endId, new Dictionary<int, int>(), new List<int>())
            { IsExtension = true };

            strand.AddToTail(domain);
            strand.SetDomainsRevamp();
            DrawCrossover.CreateXoverHelper(strand.GetDomain(strand.Domains.Count - 2), domain, strand.Id, strand.Color, strand.Color);
            return true;
        }

        return false;
    }

    private bool DrawTailExtension(DNAGrid grid, GridComponent domainGC, Strand strand,
    int nextStartId, int nextEndId, bool forward, int extensionLength, float dx, float dy)
    {
        if (grid != null && grid.Type != "none")
        {
            return TryGridBasedTailExtension(grid, domainGC, strand, nextStartId, nextEndId, forward, extensionLength, (int)dx, (int)dy);
        }
        else
        {
            return TryNoGridTailExtension((NoneGrid)grid, domainGC, strand, nextStartId, nextEndId, forward, extensionLength, dx, dy);
        }
    }

    public static void OxViewImport(string fileContents)
    {
        JObject origami = JObject.Parse(fileContents);
        List<double> box = JsonConvert.DeserializeObject<List<double>>(origami["box"].ToString());
        JArray systems = JArray.Parse(origami["systems"].ToString());
        for (int i = 0; i < systems.Count; i++)
        {
            List<OxViewStrand> strands = JsonConvert.DeserializeObject<List<OxViewStrand>>(systems[i]["strands"].ToString());
            int oxViewId = s_numOxViews;
            OxView oxView = new OxView(oxViewId);
            oxView.BuildStrands(strands, box);
            s_oxViewDict.Add(oxViewId, oxView);
            s_numOxViews++;
        }
    }

    /// <summary>
    /// Strips away slashes from scadnano files.
    /// </summary>
    private static string CleanSlash(string str)
    {
        return str.Replace("\"", "");
    }

    private static string GetGridName(string origName, bool parsingHelices = false)
    {
        if (!parsingHelices)
        {
            int numCopies = s_gridCopies.ContainsKey(origName) ? s_gridCopies[origName] : -1;
            if (numCopies == -1)
            {
                return origName;
            }
            else
            {
                return origName + " (" + numCopies + ")";
            }
        }
        else
        {
            int numCopies = s_gridCopies[origName];
            if (numCopies == 0)
            {
                return origName;
            }
            else
            {
                return origName + " (" + (numCopies - 1) + ")";
            }
        }
    }

    private static void UpdateGridCopies(string origName)
    {
        if (s_gridCopies.ContainsKey(origName))
        {
            s_gridCopies[origName] += 1;
        }
    }
}
