/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleFileBrowser;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using static GlobalVariables;

public class FileExport : MonoBehaviour
{
    // URLs for tacoxdna website.
    private const string postURL = "http://tacoxdna.sissa.it/scadnano_oxDNA/submit";
    private const string tacoURL = "http://tacoxdna.sissa.it";

    [SerializeField] private Dropdown exportTypeDropdown;
    [SerializeField] private Canvas Menu;
    private Canvas fileBrowser;

    void Awake()
    {
        FileBrowser.HideDialog();
        FileBrowser.SingleClickMode = true;

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

        // Added this because wasn't getting access to files.
        #if !UNITY_EDITOR && UNITY_ANDROID
        typeof(SimpleFileBrowser.FileBrowserHelpers).GetField("m_shouldUseSAF", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, (bool?)false);
        #endif
    }

    private void Start()
    {
        fileBrowser = FileBrowser.Instance.GetComponent<Canvas>();
        fileBrowser.gameObject.SetActive(false);
    }

    /// <summary>
    /// Initiates export of current DNA structure to the file browser.
    /// </summary>
    public void Export()
    {
        // enable file browser, disable menu
        fileBrowser.gameObject.SetActive(true);
        Menu.enabled = false;

        string exportType = exportTypeDropdown.options[exportTypeDropdown.value].text;

        if (exportType.Equals("scadnano"))
        {
            WriteSCFile(GetSCJSON());
        }
        else
        {
            StartCoroutine(CreateOxdnaFiles());
        }
    }

    private string GetSCJSON()
    {
        return GetSCJSON(new List<string>(s_gridDict.Keys), false);
    }

    /// <summary>
    /// Returns scadnano JSON format of given grids. If isCopyPaste boolean is set to true, the position of the copied grid is 
    /// set to (0, 0, 0). This accounts for the positioning when it is pasted back into the world.
    /// </summary>
    /// <param name="gridIds">List of grid ids associated with the DNAGrid objects that are being written to JSON</param>
    /// <param name="isCopyPaste">Whether or not we are copy pasting a DNAGrid object</param>
    /// <returns>Returns JSON string in scadnano format</returns>
    public static string GetSCJSON(List<string> gridIds, bool isCopyPaste)
    {
        JObject groups = new JObject();
        foreach (string gridId in gridIds)
        {
            DNAGrid grid = s_gridDict[gridId];
            
            JObject position = new JObject();
            if (isCopyPaste)
            {
                position["x"] = 0.0;
                position["y"] = 0.0;
                position["z"] = 0.0;
            }
            else
            {
                position["x"] = grid.Position.x; // TODO: Test if this gets relative positions correctly
                position["y"] = grid.Position.y;
                position["z"] = grid.Position.z;
            }

            JObject group = new JObject
            {
                ["position"] = position,
                ["pitch"] = grid.Grid2D[0, 0].transform.eulerAngles.x,
                ["roll"] = grid.Grid2D[0, 0].transform.eulerAngles.z,
                ["yaw"] = grid.Grid2D[0, 0].transform.eulerAngles.y,
                ["grid"] = grid.Type,
            };
            groups[gridId] = group;
        }


        // Creating helices data.
        JArray helices = new JArray();
        foreach (var item in s_helixDict)
        {
            int id = item.Key;
            Helix helix = item.Value;

            if (!gridIds.Contains(helix.GridId)) continue;

            JObject jsonHelix = new JObject
            {
                ["grid_position"] = new JArray { helix._gridComponent.GridPoint.X, helix._gridComponent.GridPoint.Y * -1 }, // Negative Y-axis for .sc format 
                ["group"] = helix.GridId,
                ["idx"] = id,
                ["max_offset"] = helix.Length
            };
            helices.Add(jsonHelix);
        }

        // Creating strands data.
        JArray strands = new JArray();
        foreach (var item in s_strandDict)
        {
            Strand strand = item.Value;
            if (!gridIds.Contains(strand.Head.GetComponent<NucleotideComponent>().GridId))
            {
                continue;
            }
            else if (strand.MoreThanOneGrid())
            {
                continue;
            }

            JArray domains = new JArray();
            List<int> insertions = new List<int>();
            List<int> deletions = new List<int>();
            bool isStartGO = false;
            int endId = 0;

            // Creating domains data for each strand.
            for (int i = strand.Nucleotides.Count - 1; i >= 0; i--)
            {
                var nt = strand.Nucleotides[i];
                var ntc = nt.GetComponent<NucleotideComponent>();
                if (ntc == null)
                {
                    continue;
                }

                if (ntc.IsDeletion)
                {
                    deletions.Add(ntc.Id);
                }

                if (ntc.IsInsertion)
                {
                    insertions.Add(ntc.Id);
                }

                if ((i == strand.Nucleotides.Count - 1) || (ntc.HasXover && !isStartGO))
                {
                    endId = ntc.Id;
                    isStartGO = true;
                }
                else if ((i == 0) || (ntc.HasXover && isStartGO))
                {
                    isStartGO = false;
                    JObject domain = new JObject
                    {
                        ["helix"] = ntc.HelixId,
                        ["forward"] = Convert.ToBoolean(ntc.Direction),
                        ["start"] = Math.Min(ntc.Id, endId),
                        ["end"] = Math.Max(ntc.Id, endId) + 1, // +1 accounts for .sc endId being exclusive
                    };
                    if (insertions.Count > 0)
                    {
                        insertions.Sort();
                        domain["insertions"] = JArray.FromObject(insertions);
                    }
                    if (deletions.Count > 0)
                    {
                        deletions.Sort();
                        domain["deletions"] = JArray.FromObject(deletions);
                    }
                    domains.Add(domain);
                    insertions.Clear();
                    deletions.Clear();
                }
            }

            JObject jsonStrand = new JObject
            {
                ["color"] = "#" + ColorUtility.ToHtmlStringRGB(strand.Color).ToLower(),
                ["sequence"] = strand.Sequence,
                ["is_scaffold"] = strand.IsScaffold,
                ["domains"] = domains,
            };
            strands.Add(jsonStrand);
        }

        // Creating entire json file.
        JObject scadnano = new JObject
        {
            ["version"] = "0.19.1",
            ["groups"] = groups,
            ["helices"] = helices,
            ["strands"] = strands,
        };

        return scadnano.ToString();
    }

    /// <summary>
    /// Creates .sc file and writes to it given the file path and content.
    /// </summary>
    /// <param name="path">File path to write to.</param>
    /// <param name="content">Content of the .sc file.</param>
    private void CreateSCFile(string path, string content)
    {
        if (!path.Contains(".sc"))
        {
            path += ".sc";
        }
        File.WriteAllText(path, content);
    }

    /// <summary>
    /// Writes .sc file to file browser.
    /// </summary>
    /// <param name="content">Contennt of the .sc file.</param>
    private void WriteSCFile(string content)
    {
        FileBrowser.Instance.GetComponent<Canvas>().enabled = true;
        FileBrowser.Instance.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
        bool result = FileBrowser.ShowSaveDialog((paths) => { CreateSCFile(paths[0], content); },
            () => { Debug.Log("Canceled"); },
            FileBrowser.PickMode.Files, false, null, null, "Save", "Save");

        Debug.Log("Download result: " + result);
    }

    /// <summary>
    /// Creates .top and .oxdna files and writes to them given the file path and content.
    /// </summary>
    /// <param name="path">File path to write to.</param>
    /// <param name="topContent">Content of .top file.</param>
    /// <param name="oxdnaContent">Content of .oxdna file.</param>
    private void CreateOxdnaFiles(string path, byte[] topContent, byte[] oxdnaContent)
    {
        string topPath = path + ".top";
        string oxdnaPath = path + ".oxdna";

        File.WriteAllBytes(topPath, topContent);
        File.WriteAllBytes(oxdnaPath, oxdnaContent);
    }

    /// <summary>
    /// Writes the .top and .oxdna files to file browser.
    /// </summary>
    /// <param name="topContent">Content of .top file.</param>
    /// <param name="oxdnaContent">Content of .oxdna file.</param>
    private void WriteOxdnaFiles(byte[] topContent, byte[] oxdnaContent)
    {
        bool result = FileBrowser.ShowSaveDialog((paths) => { CreateOxdnaFiles(paths[0], topContent, oxdnaContent); },
            () => { Debug.Log("Canceled"); },
            FileBrowser.PickMode.Files, false, null, null, "Save", "Save");

        Debug.Log("Download result: " + result);
    }

    /// <summary>
    /// Coroutine that downloads .top and .oxdna files from tacoxDNA using scandnano and writes them.
    /// </summary>
    /// <returns>Coroutine.</returns>
    IEnumerator CreateOxdnaFiles()
    {
        // get scadnano file structure
        byte[] data = Encoding.UTF8.GetBytes(GetSCJSON());

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("scadnano_file", data, "", null)
        };

        UnityWebRequest request = UnityWebRequest.Post(postURL, formData);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            yield break;
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
        }

        string requestResult = request.downloadHandler.text;
        string pattern = @"(jobs[^""&]*)";
        MatchCollection matches = Regex.Matches(requestResult, pattern);

        if (matches.Count != 3)
        {
            Debug.Log("Conversion to oxdna files failed");
            yield break;
        }

        /* the topology and oxdna file are downloaded from the second and third job links of the GET respone. */
        byte[] topContent;
        byte[] oxdnaContent;

        // Download topology file
        string topFileURL = matches[1].Value;
        string topFileDownloadURL = tacoURL + "/" + topFileURL;

        request = UnityWebRequest.Get(topFileDownloadURL);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            yield break;
        }
        else
        {
            topContent = request.downloadHandler.data;

            Debug.Log(".top file downloaded");
        }

        // Download oxdna file
        string oxdnaFileURL = matches[2].Value;
        string oxdnaFileDownloadURL = tacoURL + "/" + oxdnaFileURL;

        request = UnityWebRequest.Get(oxdnaFileDownloadURL);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            yield break;
        }
        else
        {
            oxdnaContent = request.downloadHandler.data;

            Debug.Log(".oxdna file downloaded");
        }

        WriteOxdnaFiles(topContent, oxdnaContent);
    }
}
