/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Newtonsoft.Json.Linq;
using SimpleFileBrowser;
using System.Collections;
using static GlobalVariables;
using static Utils;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;

/// <summary>
/// Handles all file importing into nanoVR.
/// </summary>
public class FileImport : MonoBehaviour
{
    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas loadingMenu;
    private Canvas fileBrowser;
    private static XRRayInteractor rayInteractor;
    public static FileImport Instance;
    private int originalCullingMask;

    private const string PLANE = "XY";
    private const int MAX_NUCLEOTIDES = 20000;
    Stopwatch st = new Stopwatch();

    private const string DEFAULT_GRID_NAME = "default_group";

    void Awake()
    {
        FileBrowser.HideDialog();
        FileBrowser.SingleClickMode = true;
        FileBrowser.Instance.enabled = false;
        // loadingMenu.enabled = false;
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
        rayInteractor = GameObject.Find("RightHand Controller").GetComponent<XRRayInteractor>();
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
                // loadingMenu.enabled = true;
                DoFileImport(fileContent);
            }
            else if (fileType.Equals(".json"))
            {
                // Parse cadnano JSON
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
        originalCullingMask = Camera.main.cullingMask;
        Camera.main.cullingMask = LayerMask.GetMask("Loading Screen");
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
        st.Start();
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
                string origName = CleanSlash(item.Key);
                string gridName = origName;
                if (!visualMode)
                {
                    gridName = GetGridName(origName);
                    UpdateGridCopies(origName);
                }
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
            }
        }
        else
        {
            string gridType = CleanSlash(origami["grid"].ToString());
            DrawGrid.CreateGrid(s_numGrids.ToString(), PLANE, rayInteractor.transform.position, gridType);
        }
        
        int lastHelixId = s_numHelices;
        await ParseHelices(helices, isMultiGrid);
        //Task.Run(async () => await ParseHelices(helices)).Wait();
        Debug.Log("Done parsing helices");
        // Parse strands.
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log("Not active!" + gameObject.name);
        }
        CoRunner.Instance.Run(ParseStrands(strands, lastHelixId));
        return grids;
    }

    /// <summary>
    /// Async method to parse and draw Helices from scadnano file
    /// </summary>
    private async Task ParseHelices(JArray helices, bool isMultiGrid)
    {
        Stopwatch sw = new Stopwatch();
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < helices.Count; i++)
        {
            sw.Reset();
            sw.Start();
            JArray coord = JArray.Parse(helices[i]["grid_position"].ToString());
            int length = (int)helices[i]["max_offset"];
            string gridName;
            if (helices[i]["group"] != null)
            {
                string origName = CleanSlash(helices[i]["group"].ToString());
                gridName = GetGridName(origName);
            }
            else if (isMultiGrid)
            {
                string origName = DEFAULT_GRID_NAME;
                gridName = GetGridName(origName);
            }
            else
            {
                gridName = (s_numGrids - 1).ToString();
            }

            DNAGrid grid = s_gridDict[gridName];
            int xGrid = (int)coord[0];
            int yGrid = (int)coord[1] * -1;

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

            int xInd = grid.GridXToIndex(xGrid);
            int yInd = grid.GridYToIndex(yGrid);
            GridComponent gc = grid.Grid2D[xInd, yInd];
            Helix helix = grid.AddHelix(s_numHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), length, PLANE, gc);
            bool hideNucleotides = true;
            await helix.ExtendAsync(length, hideNucleotides);
            //Task task = helix.ExtendAsync(length, hideNucleotides);
            //tasks.Add(task);
            sw.Stop();
            //Debug.Log(string.Format("Parsing helix took {0} ms", sw.ElapsedMilliseconds));
            // Parellelize every 4 helices
            /*if (i % 8 == 0 || i == helices.Count - 1)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }*/
            //grid.CheckExpansion(gc);
        }
    }

    /// <summary>
    /// Coroutine to parse and draw Strands from scadnano file
    /// </summary>
    private IEnumerator ParseStrands(JArray strands, int lastHelixId)
    {
        Stopwatch sw = new Stopwatch();
        // Drawing strands
        for (int i = 0; i < strands.Count; i++)
        {
            sw.Restart();
            JArray domains = JArray.Parse(strands[i]["domains"].ToString());
            string sequence = CleanSlash(strands[i]["sequence"].ToString());
            string hexColor = CleanSlash(strands[i]["color"].ToString());
            ColorUtility.TryParseHtmlString(hexColor, out Color color);
            int strandId = s_numStrands;
            bool isScaffold = false;
            if (strands[i]["is_scaffold"] != null)
            {
                isScaffold = (bool) strands[i]["is_scaffold"];
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
                    for (int k = 0; k < deletions.Count; k++)
                    {
                        GameObject nt = helix.GetNucleotide((int) deletions[k], Convert.ToInt32(forward));
                        sDeletions.Add(nt);
                    }
                    for (int k = 0; k < insertions.Count; k++)
                    {
                        GameObject nt = helix.GetNucleotide((int) insertions[k][0], Convert.ToInt32(forward));
                        sInsertions.Add((nt, (int) insertions[k][1]));
                    }

                    // Store domains of strand.
                    List<GameObject> domain = helix.GetHelixSub(startId, endId, Convert.ToInt32(forward));
                    nucleotides.InsertRange(0, domain);

                    // Store xover endpoints.
                    if (j == 0)
                    {
                        xoverEndpoints.Insert(0, domain[0]);
                    }
                    else if (j == domains.Count - 1)
                    {
                        xoverEndpoints.Insert(0, domain.Last());
                    }
                    else
                    {
                        xoverEndpoints.Insert(0, domain.Last());
                        xoverEndpoints.Insert(0, domain[0]);
                    }
                }
                else // Parse loopout
                {
                    int loopoutLength = (int) domains[j]["loopout"];
                    GameObject nextEndpoint = xoverEndpoints[0];
                    loopouts[nextEndpoint] = loopoutLength;
                }
            }

            Strand strand = CreateStrand(nucleotides, strandId, color, sInsertions, sDeletions, sequence, isScaffold);

            // Add xovers to strand object.
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
                    strand.Xovers.Add(DrawCrossover.CreateXoverHelper(xoverEndpoints[j - 1], nextGO));
                }
            }

            strand.SetDomains();

            // Set sequence and check for mismatches with complement strands.
            strand.Sequence = sequence;
            CheckMismatch(strand);
            sw.Stop();
            //Debug.Log(string.Format("Parsing Strand took {0} ms", sw.ElapsedMilliseconds));
            yield return null;
        }

        // Abstracts to Strand View if there are more than MAX_NUCLEOTIDES in scene.
        // This helps with performance.
        if (GlobalVariables.allGameObjects.Count > MAX_NUCLEOTIDES)
        {
            Togglers.Instance.StrandViewToggled();
            // ViewingPerspective.ViewStrand();]]
        }
        else
        {
            Togglers.Instance.NucleotideViewToggled();
            // ViewingPerspective.Instance.ShowStencils();
        }

        loadingMenu.enabled = false;
        Camera.main.cullingMask = originalCullingMask;
        st.Stop();
        Debug.Log(string.Format("Overall import took {0} ms to complete", st.ElapsedMilliseconds));
    }

    /// <summary>
    /// Strips away slashes from scadnano files.
    /// </summary>
    private static string CleanSlash(string str)
    {
        return str.Replace("\"", "");
    }

    private static string GetGridName(string origName)
    {
        int numCopies = s_gridCopies.ContainsKey(origName) ? s_gridCopies[origName] : 0;
        if (numCopies == 0)
        {
            return origName;
        }
        else
        {
            return origName + " (" + numCopies + 1 + ")";
        }
    }

    private static void UpdateGridCopies(string origName)
    {
        int numCopies = s_gridCopies.ContainsKey(origName) ? s_gridCopies[origName] : 0;
        if (numCopies > 0)
        {
            s_gridCopies[origName] = numCopies + 1;
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
