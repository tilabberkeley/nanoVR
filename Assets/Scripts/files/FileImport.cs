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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
    [SerializeField] private Toggle extensionTog;
    public static FileImport Instance;

    private const string PLANE = "XY";
    private const int MAX_STRAND_NUCLEOTIDES = 20000;
    private const int MAX_HELIX_NUCLEOTIDES = 30000;

    private const string DEFAULT_GRID_NAME = "default_group";
    private List<Vector3> atomPositions = new List<Vector3>();
    private Texture3D densityTexture;
    public int textureResolution = 128;
    public float atomRadius = 1.5f;
    public float densityScale = 1.0f;


    private static int[,] directNeighbors = {
            { -1, 0 }, // Left
            { 1, 0 },  // Right
            { 0, 1 }, // Up
            { 0, -1 }   // Down
        };

    private static int[,] diagonalNeighbors = {
            { -1, 1 }, // Upper Left
            { 1, 1 },  // Upper Right
            { -1, -1 },  // Lower Left
            { 1, -1 }    // Lower Right
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
        //rayInteractor = GameObject.Find("RightHand Controller").GetComponent<XRRayInteractor>();
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
            if (fileType.Equals(".sc") || fileType.Equals(".sc.txt"))
            {
                //StartCoroutine(ParseSC(@fileContent, false));
                DoFileImport(fileContent);
            }
            else if (fileType.Equals(".oxview"))
            {
                // Parse oxview JSON
                loadingMenu.enabled = true;
                OxViewImport(fileContent);
            }
            else if (fileType.Equals(".pdb"))
            {
                loadingMenu.enabled = true;
                PDBImport(selectedFilePath);
            }
            else
            {
                Debug.Log("Don't support file type: " + fileType);
            }
        }
        else
        {
            Debug.Log("File not found.");
        }
    }

    private void DoFileImport(string json)
    {
        /*ICommand command = new ImportCommand(json);
        CommandManager.AddCommand(command);*/
        ParseSC(json);
    }

    /// <summary>
    /// Parses through a scadnano file and draws out all Grids, Helices, and Strands.
    /// Also called when copy and pasting a Grid or converting the entire scene into visual mode (expanding out mutations).
    /// </summary>
    /// <param name="fileContents">Contents of scadnano file</param>
    /// <param name="isCopyPaste">Whether this is being called for copy/pasting a grid</param>
    /// <param name="visualMode">Whether this is being called for converting to visual mode</param>
    /// <returns>List of grids created</returns>
    public async Task<List<DNAGrid>> ParseSC(string fileContents, bool isCopyPaste = false, bool visualMode = false)
    {
        loadingMenu.enabled = true;
        List<DNAGrid> grids = new List<DNAGrid>();
        JObject origami = JObject.Parse(fileContents);
        JArray helices = JArray.Parse(origami["helices"].ToString());
        JArray strands = JArray.Parse(origami["strands"].ToString());
        bool isMultiGrid = false;

        /**
         * Parse grids
         */
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
                    if (!visualMode)
                    {
                        gridName = GetGridName(origName);
                        UpdateGridCopies(origName);
                    }
                    Debug.Log("gridname" + gridName);
                    JObject info = item.Value;
                    float x = 0;
                    float y = 0;
                    float z = 0;
                    if (info["position"] != null)
                    {
                        x = (float)info["position"]["x"] / SCALE;
                        y = (float)info["position"]["y"] / SCALE;
                        z = (float)info["position"]["z"] / SCALE;
                    }
                    
                    string gridType = CleanSlash(info["grid"].ToString());
                    Vector3 startPos;
                    if (isCopyPaste)
                    {
                        Debug.Log("Is Copypaste");
                        startPos = rayInteractor.transform.position;
                    }
                    else
                    {
                        startPos = new Vector3(x, y, z);
                    }
                    Debug.Log("startPos: " + startPos);
                    DNAGrid grid = DrawGrid.CreateGrid(gridName, PLANE, startPos, gridType);
                    Debug.Log("Created grid");
                    grids.Add(grid);

                    // Handle rotation
                    float pitch = 0f;
                    float roll = 0f;
                    float yaw = 0f;
                    if (info["pitch"] != null)
                    {
                        yaw = (float)info["pitch"];
                    }
                    if (info["roll"] != null)
                    {
                        roll = (float)info["roll"];
                    }
                    if (info["yaw"] != null)
                    {
                        pitch = (float)info["yaw"];
                    }
                    if (pitch > 0 || roll > 0 || yaw > 0)
                    {
                        grid.Rotate(pitch, roll, yaw);
                    }
                    Debug.Log("Fnish rotations");
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
            DNAGrid grid = DrawGrid.CreateGrid(s_numGrids.ToString(), PLANE, rayInteractor.transform.position, gridType);
            grids.Add(grid);
        }
        
        // Parse helices.
        int lastHelixId = s_numHelices;
        await ParseHelices(helices, isMultiGrid);

        // Parse strands.
        CoRunner.Instance.Run(ParseStrands(strands, lastHelixId));

        /* Unselect imported grids by default.
         * We choose to do this so that imp
        *//*
        foreach (DNAGrid grid in grids)
        {
            SelectGrid.ToggleGridCircles(grid.Id);
        }*/

        return grids;
    }

    /// <summary>
    /// Async method to parse and draw Helices from scadnano file
    /// </summary>
    private async Task ParseHelices(JArray helices, bool isMultiGrid)
    {
        int startHelixId = s_numHelices;
        //Debug.Log("Start parsing helices");

        for (int i = 0; i < helices.Count; i++)
        { 
            JArray coord = JArray.Parse(helices[i]["grid_position"].ToString());
            int length = (int) helices[i]["max_offset"];
            int helixId = s_numHelices;

            // Read helixId from file if available
            if (helices[i]["idx"] != null)
            {
                helixId = (int) helices[i]["idx"] + startHelixId;
            }

            string gridName;
            if (helices[i]["group"] != null)
            {
                string origName = CleanSlash(helices[i]["group"].ToString());
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
            //Debug.Log("Helix grid name: " + gridName);
            DNAGrid grid = s_gridDict[gridName];
            int xGrid = (int) coord[0];
            int yGrid = (int) coord[1] * -1;

            /**
             * Expands grid if necessary so that helix coordinates exist.
             */
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
                await helix.ExtendAsync(length, hideNucleotides: true);
                //Debug.Log("Finished extending helix");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
          
        }
    }

    /// <summary>
    /// Coroutine to parse and draw Strands from scadnano file.
    /// Also handles converting to Strand view if necessary. We do this 
    /// here since Coroutines are async and we must wait for all Strands
    /// to be parsed before going to next steps.
    /// </summary>
    private IEnumerator ParseStrands(JArray strands, int lastHelixId)
    {
        // Maps strand index in .sc file to strandId in nanoVR
        Dictionary<int, int> extensionStrands = new Dictionary<int, int>();
        bool isHelixBoundExt = extensionTog.isOn;

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
            List<GameObject> nucleotides = new List<GameObject>();
            List<GameObject> xoverEndpoints = new List<GameObject>();
            List<GameObject> sDeletions = new List<GameObject>();
            List<(GameObject, int)> sInsertions = new List<(GameObject, int)>();
            Dictionary<GameObject, int> loopouts = new Dictionary<GameObject, int>();

            for (int j = 0; j < domains.Count; j++)
            {
                if (domains[j]["helix"] != null)
                {
                    int helixId = (int) domains[j]["helix"] + lastHelixId;
                    bool forward = (bool) domains[j]["forward"];
                    int startId = (int) domains[j]["start"];
                    int endId = (int) domains[j]["end"] - 1; // End id is exclusive in .sc file
                    JArray deletions = new JArray();
                    JArray insertions = new JArray();
                    if (domains[j]["deletions"] != null) { deletions = JArray.Parse(domains[j]["deletions"].ToString()); }
                    if (domains[j]["insertions"] != null) { insertions = JArray.Parse(domains[j]["insertions"].ToString()); }
                    Helix helix = s_helixDict[helixId];

                    // Store deletions and insertions.
                    try
                    {
                        for (int k = 0; k < deletions.Count; k++)
                        {
                            GameObject nt = helix.GetNucleotide((int)deletions[k], Convert.ToInt32(forward));
                            sDeletions.Add(nt);
                        }
                        for (int k = 0; k < insertions.Count; k++)
                        {
                            GameObject nt = helix.GetNucleotide((int)insertions[k][0], Convert.ToInt32(forward));
                            sInsertions.Add((nt, (int)insertions[k][1]));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Exception when parsing deletions and insertions");
                        Debug.Log(e.Message);
                    }

                    // Store domains of strand.
                    try
                    {
                        List<GameObject> domain = helix.GetHelixSub(startId, endId, Convert.ToInt32(forward));
                        nucleotides.InsertRange(0, domain);

                        // Store xover endpoints.
                        if ((j == 0 && domains[0]["extension_num_bases"] == null)
                            // In case there are 5' extensions, second domain in domains list is first helix-bound domain
                            || (j == 1 && domains[0]["extension_num_bases"] != null))
                        {
                            xoverEndpoints.Insert(0, domain[0]);
                        }
                        else if ((j == domains.Count - 1 && domains[domains.Count - 1]["extension_num_bases"] == null)
                        // In case there are 3' extensions, second to last domain in domains list is last helix-bound domain
                            || (j == domains.Count - 2 && domains[domains.Count - 1]["extension_num_bases"] != null)) 
                        {
                            xoverEndpoints.Insert(0, domain.Last());
                        }
                        else
                        {
                            xoverEndpoints.Insert(0, domain.Last());
                            xoverEndpoints.Insert(0, domain[0]);
                        }

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
                    GameObject nextEndpoint = xoverEndpoints[0];
                    loopouts[nextEndpoint] = loopoutLength;
                }
                else
                {
                    // Save strands with extensions so that we can parse them after other strands
                    if (isHelixBoundExt)
                    {
                        extensionStrands.Add(i, strandId);
                    }
                    else
                    {
                        int extensionLength = (int)domains[j]["extension_num_bases"];
                        DrawOxViewExtension(extensionLength, j, xoverEndpoints, nucleotides);
                    }
                }
            }

            Strand strand = CreateStrand(nucleotides, strandId, color, sInsertions, sDeletions, sequence, isScaffold);
            if (isCircular)
            {
                strand.IsCircular = true;
                strand.ShowHideCone(false);
            }

            try
            {
                // Add xovers and loopouts to Strand object.
                for (int j = 1; j < xoverEndpoints.Count; j += 2)
                {
                    GameObject nextGO = xoverEndpoints[j];
                    
                    if (loopouts.ContainsKey(nextGO))
                    {
                        // TODO: Check this works
                        strand.Xovers.Add(DrawLoopout.CreateLoopoutHelper(xoverEndpoints[j - 1], nextGO, loopouts[nextGO]));
                    }
                    else
                    {
                        strand.Xovers.Add(DrawCrossover.CreateXoverHelper(xoverEndpoints[j - 1], nextGO, showXover: false));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception when adding xovers/loopouts to strand");
                Debug.Log(e.Message);
            }

            try
            {
                // Set sequence and check for mismatches with complement strands.
                strand.Sequence = sequence;
                Utils.CheckMismatch(strand);
            }
            catch (Exception e)
            {
                Debug.Log("Exception when setting strand sequence and checking mismatch");
                Debug.Log(e.Message);
            }

            yield return null;
        }

        foreach (var item in extensionStrands)
        {
            int strandIndex = item.Key;
            int strandId = item.Value;
            Strand strand = s_strandDict[strandId];

            JArray domains = JArray.Parse(strands[strandIndex]["domains"].ToString());
            string sequence = "";
            if (strands[strandIndex]["sequence"] != null)
            {
                sequence = CleanSlash(strands[strandIndex]["sequence"].ToString());
            }

            List<GameObject> nucleotides = new List<GameObject>();
            List<GameObject> xoverEndpoints = new List<GameObject>();
            

            for (int j = 0; j < domains.Count; j++)
            {
                if (domains[j]["extension_num_bases"] != null)
                {
                    // Look at next domain to see which helix it will lie on.
                    int extensionLength = (int)domains[j]["extension_num_bases"];

                    if (j == 0)
                    {     
                        int nextDomainHelixId = (int) domains[j + 1]["helix"] + lastHelixId;
                        Helix nextDomainHelix = s_helixDict[nextDomainHelixId];
                        GridComponent nextDomainGC = nextDomainHelix.GridComponent;
                        DNAGrid grid = s_gridDict[nextDomainHelix.GridId];

                        bool nextForward = (bool)domains[j + 1]["forward"];
                        int nextStartId = (int)domains[j + 1]["start"];
                        int nextEndId = (int)domains[j + 1]["end"] - 1; // End id is exclusive in .sc file

                        bool drawn = false;
                        for (int i = 0; i < directNeighbors.GetLength(0); i++)
                        {
                            bool success = DrawTailExtension(grid, nextDomainGC, strand, nextStartId, nextEndId, nextForward,
                                    extensionLength, directNeighbors[i, 0], directNeighbors[i, 1]);
                            if (success)
                            {
                                drawn = true;
                                break;
                            }
                        }

                        if (!drawn)
                        {
                            for (int i = 0; i < diagonalNeighbors.GetLength(0); i++)
                            {
                                bool success = DrawTailExtension(grid, nextDomainGC, strand, nextStartId, nextEndId, nextForward, 
                                    extensionLength, diagonalNeighbors[i, 0], diagonalNeighbors[i, 1]);
                                if (success)
                                {
                                    drawn = true;
                                    break;
                                }
                            }
                        }

                        if (!drawn)
                        {
                            Debug.Log($"Could not draw 5' extension for strand {strand.Id}");
                            continue;
                        }
                    }
                    else if (j == domains.Count - 1)
                    {
                        int nextDomainHelixId = (int)domains[j - 1]["helix"] + lastHelixId;
                        Helix nextDomainHelix = s_helixDict[nextDomainHelixId];
                        GridComponent nextDomainGC = nextDomainHelix.GridComponent;
                        DNAGrid grid = s_gridDict[nextDomainHelix.GridId];

                        bool nextForward = (bool)domains[j - 1]["forward"];
                        int nextStartId = (int)domains[j - 1]["start"];
                        int nextEndId = (int)domains[j - 1]["end"] - 1; // End id is exclusive in .sc file

                        bool drawn = false;
                        for (int i = 0; i < directNeighbors.GetLength(0); i++)
                        {
                            bool success = DrawHeadExtension(grid, nextDomainGC, strand, nextStartId, nextEndId, nextForward,
                                    extensionLength, directNeighbors[i, 0], directNeighbors[i, 1]);
                            if (success)
                            {
                                drawn = true;
                                break;
                            }
                        }

                        if (!drawn)
                        {
                            for (int i = 0; i < diagonalNeighbors.GetLength(0); i++)
                            {
                                bool success = DrawHeadExtension(grid, nextDomainGC, strand, nextStartId, nextEndId, nextForward,
                                    extensionLength, diagonalNeighbors[i, 0], diagonalNeighbors[i, 1]);
                                if (success)
                                {
                                    drawn = true;
                                    break;
                                }
                            }
                        }

                        if (!drawn)
                        {
                            Debug.Log($"Could not draw 3' extension for strand {strand.Id}");
                            continue;
                        }
                    }
                }
            }

            strand.Sequence = sequence;
            Utils.CheckMismatch(strand);

            yield return null;
        }

        // Abstracts to Helix or Strand Views if there are more than MAX_NUCLEOTIDES in scene.
        // This helps with performance.
        if (GlobalVariables.allGameObjects.Count > MAX_HELIX_NUCLEOTIDES || s_helixView)
        {
            //Togglers.Instance.CheckHelixToggle();
            CoRunner.Instance.Run(ViewingPerspective.ViewHelix());
        }
        else if (GlobalVariables.allGameObjects.Count > MAX_STRAND_NUCLEOTIDES || s_strandView)
        {
            //Togglers.Instance.CheckStrandToggle();
            CoRunner.Instance.Run(ViewingPerspective.ViewStrand());
        }
        else
        {
            CoRunner.Instance.Run(ViewingPerspective.ViewNucleotide());
        }

        loadingMenu.enabled = false;
        //Debug.Log(string.Format("Overall sc import took {0} ms to complete", st.ElapsedMilliseconds));
    }

    private bool DrawHeadExtension(DNAGrid grid, GridComponent domainGC, Strand strand, int startId, int endId, bool forward, int extensionLength, int dx, int dy)
    {
        int newX = domainGC.GridPoint.X + dx;
        int newY = domainGC.GridPoint.Y + dy;
        int xIndex = grid.GridXToIndex(newX);
        int yIndex = grid.GridYToIndex(newY);
        GridComponent gc = grid.Grid2D[xIndex, yIndex];

        // We have found a neighbor Grid circle with no helix.
        if (gc != null)
        {
            Helix helix;
            int length = Math.Max(startId + extensionLength, endId + 1);
            int num64 = length / 64 + 1;
            int actualLength = num64 * 64;

            if (gc.Helix == null)
            {
                helix = grid.AddHelix(s_numHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), actualLength, PLANE, gc);
                helix.Extend(actualLength, hideNucleotides: true);
                grid.CheckExpansion(gc);
            }
            else
            {
                helix = gc.Helix;
                if (length > helix.Length)
                {
                    helix.Extend(actualLength - helix.Length, hideNucleotides: true);
                }
            }

            List<GameObject> domain;
            if (forward)
            {
                domain = helix.GetHelixSub(startId - extensionLength + 1, startId, Convert.ToInt32(!forward));
            }
            else
            {
                domain = helix.GetHelixSub(startId, startId + extensionLength - 1, Convert.ToInt32(!forward));
            }
            if (Utils.IsValidNucleotides(domain))
            {
                SetExtensions(domain);
                GameObject oldHead = strand.Head;
                strand.AddToHead(domain);
                strand.SetComponents();
                strand.Xovers.Add(DrawCrossover.CreateXoverHelper(oldHead, domain.Last(), showXover: false));
                return true;
            }
        }

        return false;
    }

    private bool DrawTailExtension(DNAGrid grid, GridComponent domainGC, Strand strand, int startId, int endId, bool forward, int extensionLength, int dx, int dy)
    {
        int newX = domainGC.GridPoint.X + dx;
        int newY = domainGC.GridPoint.Y + dy;
        int xIndex = grid.GridXToIndex(newX);
        int yIndex = grid.GridYToIndex(newY);
        GridComponent gc = grid.Grid2D[xIndex, yIndex];

        // We have found a neighbor Grid circle with no helix.
        if (gc != null)
        {
            Helix helix;
            int length = Math.Max(startId + extensionLength, endId + 1);
            int num64 = length / 64 + 1;
            int actualLength = num64 * 64;

            if (gc.Helix == null)
            {
                helix = grid.AddHelix(s_numHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), actualLength, PLANE, gc);
                helix.Extend(actualLength, hideNucleotides: true);
                grid.CheckExpansion(gc);
            }
            else
            {
                helix = gc.Helix;
                if (length > helix.Length)
                {
                    helix.Extend(actualLength - helix.Length, hideNucleotides: true);
                }
            }

            List<GameObject> domain;
            if (forward)
            {
                domain = helix.GetHelixSub(startId, endId + extensionLength - 1, Convert.ToInt32(!forward));
            }
            else
            {
                domain = helix.GetHelixSub(startId - extensionLength + 1, endId, Convert.ToInt32(!forward));
            }
            if (Utils.IsValidNucleotides(domain))
            {
                GameObject oldTail = strand.Tail;
                SetExtensions(domain);
                strand.AddToTail(domain);
                strand.SetComponents();
                strand.Xovers.Add(DrawCrossover.CreateXoverHelper(domain[0], oldTail, showXover: false));
                return true;
            }
        }

        return false;
    }

    private void DrawOxViewExtension(int extensionLength, int domainIndex, List<GameObject> xoverEndpoints, List<GameObject> nucleotides)
    {
        // Handle extensions
        // 1. Get extension length n
        // 2. Generate n # of nucleotides/backbones
        // 3. Calculate the vector they will lie on
        // 4. Add these to the strand sequence
        // 5. Mark these nucleotides/backbones with isExtension = true flag, so domain is also marked as isExtension = true
        // 6. Update FileExport code to handle extensions (when isExtension = true)
        List<GameObject> domain = new List<GameObject>();
        NucleotideComponent currHead = xoverEndpoints[0].GetComponent<NucleotideComponent>();
        int currHeadDirection = currHead.Direction;
        GridComponent gc = s_helixDict[currHead.HelixId].GridComponent;
        Vector3 direction;
        if (currHeadDirection == 0)
        {
            // TODO: Get these turns correct
            direction = gc.transform.right;
        }
        else
        {
            direction = -gc.transform.right;
        }


        if (ObjectPoolManager.Instance.CanGetNucleotides(extensionLength) && ObjectPoolManager.Instance.CanGetBackbones(extensionLength - 1))
        {
            List<GameObject> nucls = ObjectPoolManager.Instance.GetNucleotides(extensionLength);
            List<GameObject> backs = ObjectPoolManager.Instance.GetBackbones(extensionLength - 1);

            for (int k = 0; k < nucls.Count; k++)
            {
                // TODO: Correctly set a1/a3 values
                DrawPoint.SetNucleotide(nucls[k], (k + 1) * Utils.RISE * 2 * direction + currHead.transform.position, Vector3.one, Vector3.one, -1, -1, -1, false, false, isExtension: true);

                if (k == 0)
                {
                    DrawPoint.SetBackbone(backs[k], -1, -1, -1, nucls[k].transform.position, currHead.transform.position);
                }
                if (k > 0)
                {
                    DrawPoint.SetBackbone(backs[k - 1], -1, -1, -1, nucls[k].transform.position, nucls[k - 1].transform.position);
                }
            }

            for (int k = 0; k < backs.Count; k++)
            {
                domain.Add(nucls[k]);
                domain.Add(backs[k]);
            }
            domain.Add(nucls.Last());
        }

        if (domainIndex == 0)
        {
            xoverEndpoints.Add(domain[0]);
        }

        // If extension is on 3' end (front of nanoVR strand), we reverse the domain to ensure correct ordering.
        else
        {
            domain.Reverse();
            xoverEndpoints.Insert(0, domain.Last());
        }
        nucleotides.InsertRange(0, domain);
    }

    private void SetExtensions(List<GameObject> domain)
    {
        foreach (GameObject go in domain)
        {
            DNAComponent dna = go.GetComponent<DNAComponent>();
            dna.IsExtension = true;
        }
    }

    private void OxViewImport(string fileContents)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        JObject origami = JObject.Parse(fileContents);
        List<double> box = JsonConvert.DeserializeObject<List<double>>(origami["box"].ToString());
        JArray systems = JArray.Parse(origami["systems"].ToString());
        for (int i = 0; i < systems.Count; i++)
        {
            List<OxViewStrand> strands = JsonConvert.DeserializeObject<List<OxViewStrand>>(systems[i]["strands"].ToString());
            oxView.BuildStrands(strands, box);
        }
        sw.Stop();
        loadingMenu.enabled = false;
        // Debug.Log(string.Format("OxView import took {0} ms to complete", sw.ElapsedMilliseconds));
    }

    private void PDBImport(string path)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        ReadPDBFile(path, vertices, triangles);

        // Create the mesh
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Attach the mesh to a GameObject
        GameObject proteinObject = new GameObject("Protein");
        MeshFilter meshFilter = proteinObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = proteinObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));

        GlobalVariables.proteins.Add(proteinObject);
    }

    private void ReadPDBFile(string filePath, List<Vector3> vertices, List<int> triangles)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("ATOM") || line.StartsWith("HETATM"))
                {
                    // Parse the atom coordinates from the PDB file
                    float x = float.Parse(line.Substring(30, 8));
                    float y = float.Parse(line.Substring(38, 8));
                    float z = float.Parse(line.Substring(46, 8));
                    vertices.Add(new Vector3(x, y, z));
                }
            }
        }

        // Generate the triangle indices
        for (int i = 0; i < vertices.Count - 2; i += 3)
        {
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
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
        /*int numCopies = s_gridCopies.ContainsKey(origName) ? s_gridCopies[origName] : 0;
        if (numCopies > 0)
        {
            s_gridCopies[origName] = numCopies + 1;
        }*/
        if (s_gridCopies.ContainsKey(origName))
        {
            s_gridCopies[origName] += 1;
        }
    }

    /* private static int CountDuplicates(string gridName)
     {
         int count = 0;
         foreach (string gridId in s_gridDict.Keys)
         {
             if (gridId.StartsWith(gridName + " ("))
             {
                 count += 1;
             }
         }
         return count;
     }*/
}
