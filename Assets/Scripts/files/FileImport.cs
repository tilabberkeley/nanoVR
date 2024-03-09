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
using Newtonsoft.Json.Linq;
using SimpleFileBrowser;
using static GlobalVariables;
using static Utils;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class FileImport : MonoBehaviour
{
    [SerializeField] private Canvas Menu;
    private Canvas fileBrowser;
    private static XRRayInteractor rayInteractor;

    private const string PLANE = "XY";
    void Awake()
    {
        FileBrowser.HideDialog();
        FileBrowser.SingleClickMode = true;
        FileBrowser.Instance.enabled = false;

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
        // enable file browser, disable menu
        fileBrowser.gameObject.SetActive(true);
        Menu.enabled = false;

        FileBrowser.Instance.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
        FileBrowser.ShowLoadDialog((paths) => { LoadFile(paths[0]); },
                              () => { Debug.Log("Canceled"); },
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
                StartCoroutine(ParseSC(@fileContent, false));
                //ParseSC(@fileContent, false);
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

    // Using LINQ JSON
    public static IEnumerator ParseSC(string fileContents, bool isCopyPaste, bool visualMode = false)
    {
        // TODO: Split these in different methods?
        JObject origami = JObject.Parse(fileContents);
        JArray helices = JArray.Parse(origami["helices"].ToString());
        JArray strands = JArray.Parse(origami["strands"].ToString());
        if (origami["groups"] != null)
        {
            JObject groupObject = JObject.Parse(origami["groups"].ToString());
            Dictionary<string, JObject> groups = groupObject.ToObject<Dictionary<string, JObject>>();
            foreach (var item in groups)
            {
                string gridName = CleanSlash(item.Key);
                JObject info = item.Value;
                float x = 0;
                float y = 0;
                float z = 0;
                if (info["position"] != null)
                {
                    x = (float)info["position"]["x"];
                    y = (float)info["position"]["y"];
                    z = (float)info["position"]["z"];
                }
                string gridType = CleanSlash(info["grid"].ToString());

                if (isCopyPaste) // Creates unique key for grid dictionary
                {
                    int numDuplicates = CountDuplicates(gridName);
                    gridName += " (" + (numDuplicates + 1) + ")";
                }
                DrawGrid.CreateGrid(gridName, PLANE, rayInteractor.transform.position + new Vector3(x, y, z), gridType);
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            string gridType = CleanSlash(origami["grid"].ToString());
            DrawGrid.CreateGrid(s_numGrids.ToString(), PLANE, rayInteractor.transform.position + new Vector3(0, 0, 0), gridType);
        }

        int prevNumHelices = s_numHelices;
        for (int i = 0; i < helices.Count; i++)
        {
            JArray coord = JArray.Parse(helices[i]["grid_position"].ToString());
            int length = (int)helices[i]["max_offset"];
            string gridName;
            if (helices[i]["group"]!= null)
            {
                gridName = CleanSlash(helices[i]["group"].ToString());
                if (isCopyPaste) // Finds the copied gridId key in grid dictionary
                {
                    int numDuplicates = CountDuplicates(gridName);
                    gridName += " (" + numDuplicates + ")";
                }
            }
            else
            {
                gridName = (s_numGrids - 1).ToString();
            }

            DNAGrid grid = s_gridDict[gridName];
            int xInd = grid.GridXToIndex((int)coord[0]);
            int yInd = grid.GridYToIndex((int)(coord[1]) * -1);
            GridComponent gc = grid.Grid2D[xInd, yInd];
            grid.AddHelix(s_numHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), length, PLANE, gc);
            grid.CheckExpansion(gc);
            yield return new WaitForSeconds(0.1f);
        }

        for (int i = 0; i < strands.Count; i++)
        {
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
            for (int j = 0; j < domains.Count; j++)
            {
                int helixId = (int) domains[j]["helix"] + prevNumHelices;
                bool forward = (bool) domains[j]["forward"];
                int startId = (int) domains[j]["start"];
                int endId = (int) domains[j]["end"] - 1; // End id is exclusive in .sc file
                JArray deletions = new JArray();
                JArray insertions = new JArray();
                if (domains[j]["deletions"] != null) { deletions = JArray.Parse(domains[j]["deletions"].ToString()); }
                if (domains[j]["insertions"] != null) { deletions = JArray.Parse(domains[j]["insertions"].ToString()); }
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
                    xoverEndpoints.Add(domain[0]);
                }
                else if (j == domains.Count - 1)
                {
                    xoverEndpoints.Add(domain.Last());
                }
                else
                {
                    xoverEndpoints.Add(domain.Last());
                    xoverEndpoints.Add(domain[0]);
                }
            }

            Strand strand = CreateStrand(nucleotides, strandId, color, sInsertions, sDeletions, sequence, isScaffold);

            /*// Add deletions and insertions.
            for (int j = 0; j < sDeletions.Count; j++)
            {
                DrawDeletion.Deletion(sDeletions[j]);
            }
            for (int j = 0; j < sInsertions.Count; j++)
            {
                DrawInsertion.Insertion(sInsertions[j].Item1, sInsertions[j].Item2);
            }*/

            // Add xovers to strand object.
            xoverEndpoints.Reverse();
            for (int j = 1; j < xoverEndpoints.Count; j += 2)
            {
                strand.Xovers.Add(DrawCrossover.CreateXoverHelper(xoverEndpoints[j - 1], xoverEndpoints[j]));
            }
            yield return new WaitForSeconds(0.1f);

        }
        yield return new WaitForSeconds(0.1f);
    }



    /// <summary>
    /// Strips away slashes from scadnano files.
    /// </summary>
    /// <param name="str">String to be cleaned.</param>
    /// <returns></returns>
    private static string CleanSlash(string str)
    {
        return str.Replace("\"", "");
    }

    private static int CountDuplicates(string gridName)
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
    }
}
