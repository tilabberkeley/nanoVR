using System;
using System.Collections;
using UnityEngine;
using SimpleFileBrowser;
using System.Collections.Generic;
using System.IO;
using OVRSimpleJSON;
using System.Reflection;
using static GlobalVariables;
using static Utils;

public class FileImport : MonoBehaviour
{
    // Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
    // Warning: FileBrowser can only show 1 dialog at a time

    private void Start()
    {
        FileBrowser.HideDialog();
#if !UNITY_EDITOR && UNITY_ANDROID
        Debug.Log("Test");
        typeof(FileBrowserHelpers).GetField("m_shouldUseSAF", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, (bool?)false);

#endif
        //FileBrowser.SetFilters(true, new FileBrowser.Filter("Scadnano", ".sc"), new FileBrowser.Filter("Cadnano", ".json"));
    }



    void Awake()
    {
#if UNITY_ANDROID
        Debug.Log("Test");
        typeof(FileBrowserHelpers).GetField("m_shouldUseSAF", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, (bool?)false);
#endif
        FileBrowser.SingleClickMode = true;
        //FileBrowser.SetFilters(true, new FileBrowser.Filter("Scadnano", ".sc"), new FileBrowser.Filter("Cadnano", ".json"));

        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        //FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Text Files", ".txt", ".pdf"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        //FileBrowser.SetDefaultFilter(".jpg");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        //FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        //FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        // Show a save file dialog 
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Allow multiple selection: false
        // Initial path: "C:\", Initial filename: "Screenshot.png"
        // Title: "Save As", Submit button text: "Save"
        // FileBrowser.ShowSaveDialog( null, null, FileBrowser.PickMode.Files, false, "C:\\", "Screenshot.png", "Save As", "Save" );

        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Allow multiple selection: false
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Select Folder", Submit button text: "Select"
        /*FileBrowser.ShowLoadDialog( ( paths ) => { Debug.Log( "Selected: " + paths[0] ); },
        					   () => { Debug.Log( "Canceled" ); },
        					   FileBrowser.PickMode.Files, false, null, null, "Select File", "Select" );
*/
        // Coroutine example
        //StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                Debug.Log(FileBrowser.Result[i]);

            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            // Or, copy the first file to persistentDataPath
            string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
        }
    }

    public void OpenFile()
    {
        //var filePath = Path.Combine(Application.persistentDataPath + "/origami.sc");\
        //LoadFile(filePath);
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
            Debug.Log("File selected.");
            ParseSC(@fileContent);
        }
        else
        {
            Debug.Log("File not found.");
        }
    }

    private void ParseSC(string fileContents)
    {
        Debug.Log("Deserializing");
        JSONNode origami = JSON.Parse(fileContents);
        string gridType = Clean(origami["grid"].ToString());
        JSONArray helices = origami["helices"].AsArray;
        JSONArray strands = origami["strands"].AsArray;


        // FIX THIS WITH DrawGrid.CreateGrid
        /*if (gridType.Equals("square"))
        {
            DrawGrid.CreateGrid(s_numGrids, "XY", transform.position);
        }*/

        //Grid grid = DrawGrid.CreateGrid(s_numGrids, "XY", transform.position);
        Grid grid = DrawGrid.CreateGrid(s_numGrids, "XY", transform.position, gridType);

        for (int i = 0; i < helices.Count; i++)
        {
            JSONArray coord = helices[i]["grid_position"].AsArray;
            int length = helices[i]["max_offset"].AsInt;

            int xInd = grid.GridXToIndex(coord[0].AsInt);
            int yInd = grid.GridYToIndex(coord[1].AsInt * -1);

            GridComponent gc = grid.Grid2D[xInd, yInd];
            grid.AddHelix(s_numHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), length, "XY", gc);
            grid.CheckExpansion(gc);
        }

        for (int i = 0; i < strands.Count; i++)
        {
            JSONArray domains = strands[i]["domains"].AsArray;
            string sequence = Clean(strands[i]["sequence"].ToString());
            string hexColor = Clean(strands[i]["color"].ToString());
            Color color;
            ColorUtility.TryParseHtmlString(hexColor, out color);
            int strandId = s_numStrands;

            List<GameObject> xoverEndpoints = new List<GameObject>();
            for (int j = 0; j < domains.Count; j++)
            {
                int helixId = domains[j]["helix"].AsInt;
                bool forward = domains[j]["forward"].AsBool;
                int startId = domains[j]["start"].AsInt;
                int endId = domains[j]["end"].AsInt - 1; // End id is exclusive in .sc file
                JSONArray deletions = domains[j]["deletions"].AsArray;
                JSONArray insertions = domains[j]["insertions"].AsArray;
                Helix helix = s_helixDict[helixId];
                List<GameObject> nucleotides;

                for (int k = 0; k < deletions.Count; k++)
                {
                    GameObject nt = helix.GetNucleotide(deletions[k], Convert.ToInt32(forward));
                    DrawDeletion.Deletion(nt);
                }

                for (int k = 0; k < insertions.Count; k++)
                {
                    GameObject nt = helix.GetNucleotide(insertions[k][0], Convert.ToInt32(forward));
                    DrawInsertion.Insertion(nt, insertions[k][1]);
                }
              
                Debug.Log("Printing: " + startId + ", " + endId);
                nucleotides = helix.GetHelixSub(startId, endId, Convert.ToInt32(forward));
                
                // Change to Utils method.
                Strand strand = new Strand(nucleotides, s_numStrands, color);
                strand.SetComponents();
                s_strandDict.Add(s_numStrands, strand);
                DrawNucleotideDynamic.CreateButton(s_numStrands);
                s_numStrands += 1;

                if (j == 0)
                {
                    xoverEndpoints.Add(strand.GetHead());
                }
                else if (j == domains.Count - 1)
                {
                    xoverEndpoints.Add(strand.GetTail());
                }
                else
                {
                    xoverEndpoints.Add(strand.GetTail());
                    xoverEndpoints.Add(strand.GetHead());
                }
            }

            // Add xovers to strand object.
            for (int j = 1; j < xoverEndpoints.Count; j += 2)
            {
                GameObject xover = DrawCrossover.CreateXover(xoverEndpoints[j - 1], xoverEndpoints[j]);
            }

            // Assign DNA sequence to strand.
            Strand fullStrand = s_strandDict[strandId];
            int seqCount = 0;
            for (int j = fullStrand.Nucleotides.Count - 1; j >= 0; j--)
            {
                var ntc = fullStrand.Nucleotides[j].GetComponent<NucleotideComponent>();
                if (ntc != null)
                {
                    if (ntc.IsDeletion)
                    {
                        ntc.Sequence = "X";
                    }
                    else if (ntc.IsInsertion)
                    {
                        ntc.Sequence = sequence.Substring(seqCount, ntc.Insertion + 1);
                        seqCount += ntc.Insertion + 1;
                    }
                    else
                    {
                        ntc.Sequence = sequence.Substring(seqCount, 1);
                        Debug.Log(ntc.Sequence);
                        seqCount += 1;
                    }
                }
            }
        }
    }  
}
