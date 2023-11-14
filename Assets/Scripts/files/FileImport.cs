using System;
using System.Collections;
using UnityEngine;
using SimpleFileBrowser;
using static GlobalVariables;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Android;
using Newtonsoft.Json.Linq;
using OVRSimpleJSON;

public class FileImport : MonoBehaviour
{
    string selectedFilePath;
    // Start is called before the first frame update
    void Awake()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     private void OpenFilePicker()
    {
        // Check if we are running on Android
        if (Application.platform == RuntimePlatform.Android)
        {
            // Request READ_EXTERNAL_STORAGE permission
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }

            // Check if permission was granted
            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
                intent.Call<AndroidJavaObject>("setType", "*/*");
                intent.Call<AndroidJavaObject>("setAction", intent.GetStatic<string>("ACTION_GET_CONTENT"));

                // Start the file picker activity
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject chooser = intent.CallStatic<AndroidJavaObject>("createChooser", intent, "Select a file");
                currentActivity.Call("startActivityForResult", chooser, 0);
            }
        }
    }

    // This method handles the result when a file is picked
    public void OnActivityResult(string filePath)
    {
        selectedFilePath = filePath;
        StartCoroutine(LoadFileContent());
    }

    private IEnumerator LoadFileContent()
    {
        yield return new WaitForEndOfFrame();

        if (string.IsNullOrEmpty(selectedFilePath))
        {
            Debug.Log("No file selected.");
            yield break;
        }

        if (File.Exists(selectedFilePath))
        {
            try
            {
                string fileContent = File.ReadAllText(selectedFilePath);
                Debug.Log("File selected.");

            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e.Message);

            }
        }
        else
        {
            Debug.Log("File not found.");

        }
    }

    public void OpenFile()
    {
        var filePath = Path.Combine(Application.persistentDataPath + "/origami.sc");
        LoadFile(filePath);
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
           
            Parse(@fileContent);

        }
        else
        {
            Debug.Log("File not found.");
        }
    }

    private void Parse(string fileContents)
    {
        Debug.Log("Deserializing");
        JSONNode origami = JSON.Parse(fileContents);
        string gridType = origami["grid"].ToString();
        JSONArray helices = origami["helices"].AsArray;
        JSONArray strands = origami["strands"].AsArray;

        /*if (gridType.Equals("square"))
        {
            DrawGrid.CreateGrid(s_numGrids, "XY", transform.position);
        }*/

        Grid grid = DrawGrid.CreateGrid(s_numGrids, "XY", transform.position);

        for (int i = 0; i < helices.Count; i++)
        {
            JSONArray coord = helices[i]["grid_position"].AsArray;
            GridComponent gc = grid.Grid2D[coord[0].AsInt, coord[1].AsInt];
            grid.AddHelix(s_numHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), 64, "XY", gc);
            grid.CheckExpansion(gc);
        }

        for (int i = 0; i < strands.Count; i++)
        {
            JSONArray domains = strands[i]["domains"].AsArray;
            List<GameObject> xoverEndpoints = new List<GameObject>();
            for (int j = 0; j < domains.Count; j++)
            {
                int helixId = domains[j]["helix"].AsInt;
                bool forward = domains[j]["forward"].AsBool;
                int startId = domains[j]["start"].AsInt;
                int endId = domains[j]["end"].AsInt;
                Helix helix = s_helixDict[helixId];
                List<GameObject> nucleotides;
                if (forward)
                {
                    Debug.Log("Printing forward: " + startId + ", " + endId);
                    nucleotides = helix.GetHelixSub(startId, endId, 1);
                }
                else
                {
                    Debug.Log("Printing backward: " + startId + ", " + endId);
                    nucleotides = helix.GetHelixSub(startId, endId, 0);
                }
                Strand strand = new Strand(nucleotides, s_numStrands);
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

            for (int j = 1; j < xoverEndpoints.Count; j += 2)
            {
                DrawCrossover.CreateXover(xoverEndpoints[j - 1], xoverEndpoints[j]);
            }
        }
    }

    /*public void OpenFilePicker()
    {
        StartCoroutine(OpenFilePickerCoroutine());
    }*/

    private IEnumerator OpenFilePickerCoroutine()
    {
        // Request READ_EXTERNAL_STORAGE permission (for Android)
        yield return new WaitForSeconds(0.1f); // Wait for a moment (required for permission dialog)
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead));
        }

        // Check if permission was granted
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            // Use Android's Intent system to open the file picker
            AndroidJavaObject uri = GetFilePickerUri();
            AndroidJavaObject intent = CreateOpenFileIntent(uri);

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            currentActivity.Call("startActivity", intent);

            // Release resources
            uri.Dispose();
            intent.Dispose();
        }
    }

    private AndroidJavaObject GetFilePickerUri()
    {
        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        return uriClass.CallStatic<AndroidJavaObject>("parse", "content://com.android.externalstorage.documents/document/");
    }

    private AndroidJavaObject CreateOpenFileIntent(AndroidJavaObject uri)
    {
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", intentClass.GetStatic<AndroidJavaObject>("ACTION_OPEN_DOCUMENT"));

        intent.Call<AndroidJavaObject>("addCategory", intentClass.GetStatic<string>("CATEGORY_OPENABLE"));
        intent.Call<AndroidJavaObject>("setType", "text/*"); // Specify the MIME type (you can change this for different file types)

       /* intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_ALLOW_MULTIPLE"), false);
        intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_LOCAL_ONLY"), true);
        intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_MIME_TYPES"), new string[] { "text/plain" });

        intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TITLE"), "Select a file");
        intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_INITIAL_URI"), uri);*/

        return intent;
    }

  /*  public void OpenFileManager()
    {
        // Check if the device is running on Android
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                // Define the package name of the Oculus File Manager
                string package = "com.oculus.systemactivities";

                // Create an intent to open the File Manager
                AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
                AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
                intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_MAIN"));
                intentObject.Call<AndroidJavaObject>("addCategory", intentClass.GetStatic<string>("CATEGORY_DEFAULT"));
                intentObject.Call<AndroidJavaObject>("setPackage", package);

                // Get the current activity and start the File Manager intent
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                currentActivity.Call("startActivity", intentObject);

              
            }
            catch (Exception e)
            {
                Debug.LogError("Error opening Oculus File Manager: " + e);
            }
        }
        else
        {
            Debug.LogWarning("This feature is only available on Android devices.");
        }
    }

    public void OpenExplorer()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Text Files", ".txt", ".sc"));
        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Allow multiple selection: false
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Select Folder", Submit button text: "Select"
        *//*FileBrowser.ShowLoadDialog( ( paths ) => { Debug.Log( "Selected: " + paths[0] ); },
        						   () => { Debug.Log( "Canceled" ); },
        						   FileBrowser.PickMode.FilesAndFolders, false, null, null, "Select Folder", "Select" );*//*

        //FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

        Debug.Log("Opening file explorer!");
        StartCoroutine(ShowLoadDialogCoroutine());

    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        Debug.Log("Loading file explorer!");

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

        Debug.Log("Loaded file explorer!");

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

            // Does this work???
            Parse(BitConverter.ToString(bytes));
        }
    }*/

   
}
