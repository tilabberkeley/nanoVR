/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using SimpleFileBrowser;
using static GlobalVariables;
using static Utils;

public class FileImport : MonoBehaviour
{
    [SerializeField] private Canvas Menu;

    private const string PLANE = "XY";
    void Awake()
    {
        FileBrowser.HideDialog();
        FileBrowser.SingleClickMode = true;
        FileBrowser.Instance.enabled = false;

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

    public void OpenFile()
    {
        // enable file browser, disable menu
        FileBrowser.Instance.enabled = true;
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
                ParseSC(@fileContent);
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

    private void ParseSC(string fileContents)
    {
        JObject origami = JObject.Parse(fileContents);
        string gridType = CleanSlash(origami["grid"].ToString());
        JArray helices = origami["helices"].ToObject<JArray>();
        JArray strands = origami["strands"].ToObject<JArray>();
        DNAGrid grid = DrawGrid.CreateGrid(s_numGrids, PLANE, transform.position, gridType);
        int prevNumHelices = s_numHelices;

        for (int i = 0; i < helices.Count; i++)
        {
            JArray coord = helices[i]["grid_position"].ToObject<JArray>();
            int length = helices[i]["max_offset"].ToObject<int>();
            int xInd = grid.GridXToIndex(coord[0].ToObject<int>());
            int yInd = grid.GridYToIndex(coord[1].ToObject<int>() * -1);
            GridComponent gc = grid.Grid2D[xInd, yInd];
            grid.AddHelix(s_numHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), length, PLANE, gc);
            grid.CheckExpansion(gc);
        }

        for (int i = 0; i < strands.Count; i++)
        {
            JArray domains = strands[i]["domains"].ToObject<JArray>();
            string sequence = CleanSlash(strands[i]["sequence"].ToString());
            string hexColor = CleanSlash(strands[i]["color"].ToString());
            ColorUtility.TryParseHtmlString(hexColor, out Color color);
            int strandId = s_numStrands;
            List<GameObject> nucleotides = new List<GameObject>();
            List<GameObject> xoverEndpoints = new List<GameObject>();
            List<GameObject> sDeletions = new List<GameObject>();
            List<(GameObject, int)> sInsertions = new List<(GameObject, int)>();
            for (int j = 0; j < domains.Count; j++)
            {
                int helixId = domains[j]["helix"].ToObject<int>() + prevNumHelices;
                bool forward = domains[j]["forward"].ToObject<bool>();
                int startId = domains[j]["start"].ToObject<int>();
                int endId = domains[j]["end"].ToObject<int>() - 1; // End id is exclusive in .sc file
                JArray deletions = domains[j]["deletions"].ToObject<JArray>();
                JArray insertions = domains[j]["insertions"].ToObject<JArray>();
                Helix helix = s_helixDict[helixId];

                // Store deletions and insertions.
                for (int k = 0; k < deletions.Count; k++)
                {
                    GameObject nt = helix.GetNucleotide(deletions[k].ToObject<int>(), Convert.ToInt32(forward));
                    sDeletions.Add(nt);
                }
                for (int k = 0; k < insertions.Count; k++)
                {
                    GameObject nt = helix.GetNucleotide(insertions[k][0].ToObject<int>(), Convert.ToInt32(forward));
                    sInsertions.Add((nt, insertions[k][1].ToObject<int>()));
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

            CreateStrand(nucleotides, strandId, color);

            // Add deletions and insertions.
            for (int j = 0; j < sDeletions.Count; j++)
            {
                DrawDeletion.Deletion(sDeletions[j]);
            }
            for (int j = 0; j < sInsertions.Count; j++)
            {
                DrawInsertion.Insertion(sInsertions[j].Item1, sInsertions[j].Item2);
            }

            // Add xovers to strand object.
            xoverEndpoints.Reverse();
            for (int j = 1; j < xoverEndpoints.Count; j += 2)
            {
                DrawCrossover.CreateXoverHelper(xoverEndpoints[j - 1], xoverEndpoints[j]);
            }

            // Assign DNA sequence to strand.
            Strand fullStrand = s_strandDict[strandId];
            fullStrand.Sequence = sequence;
            //Debug.Log("# of nucleotides: " + fullStrand.Count);
            //Debug.Log("sequence length: " + sequence.Length);
            /*int seqCount = 0;
            for (int j = fullStrand.Nucleotides.Count - 1; j >= 0; j--)
            {
                var ntc = fullStrand.Nucleotides[j].GetComponent<NucleotideComponent>();
                if (ntc != null)
                {
                    if (ntc.IsDeletion)
                    {
                        ntc.Sequence = "X";
                    }
                    else
                    {
                        ntc.Sequence = sequence.Substring(seqCount, ntc.Insertion + 1);
                        seqCount += ntc.Insertion + 1;
                    }
                }
            }*/
        }
    }

    /// <summary>
    /// Strips away slashes from scadnano files.
    /// </summary>
    /// <param name="str">String to be cleaned.</param>
    /// <returns></returns>
    public static string CleanSlash(string str)
    {
        return str.Replace("\"", "");
    }
}
