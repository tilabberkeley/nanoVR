using OVRSimpleJSON;
using UnityEngine;
using SimpleFileBrowser;
using static GlobalVariables;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class FileExport : MonoBehaviour
{
    // URLs for tacoxdna website.
    private const string postURL = "http://tacoxdna.sissa.it/scadnano_oxDNA/submit";
    private const string tacoURL = "http://tacoxdna.sissa.it";

    [SerializeField] Dropdown exportTypeDropdown;

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

    public void Export()
    {
        string exportType = exportTypeDropdown.options[exportTypeDropdown.value].text;

        if (exportType.Equals("scadnano"))
        {
            WriteFile(GetSCJSON(), ".sc");
        }
        else
        {
            StartCoroutine(CreateOxdnaFile());
            StartCoroutine(CreateTopFile());
        }
    }

    private string GetSCJSON()
    {
        // Creating helices data.
        JArray helices = new JArray();
        for (int i = 0; i < s_helixDict.Count; i++)
        {
            Helix helix = s_helixDict[i];
            JObject jsonHelix = new JObject
            {
                ["grid_position"] = new JArray { helix._gridComponent.GridPoint.X, helix._gridComponent.GridPoint.Y * -1 }, // Negative Y-axis for .sc format 
                ["max_offset"] = helix.Length
            };
            helices.Add(jsonHelix);
        }

        // Creating strands data.
        JArray strands = new JArray();
        foreach (var item in s_strandDict)
        {
            Strand strand = item.Value;
            JObject domain = new JObject();
            JArray domains = new JArray();
            JArray insertions = new JArray();
            JArray deletions = new JArray();
            bool endGO = false;
            int startId = 0;

            // Creating domains data for each strand.
            for (int i = 0; i < strand.Nucleotides.Count; i++)
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

                if ((i == 0) || (ntc.HasXover() && !endGO))
                {
                    startId = ntc.Id;
                    endGO = true;
                }
                else if ((i == strand.Nucleotides.Count - 1)
                    || (ntc.HasXover() && endGO))
                {
                    Debug.Log("startId: " + startId);
                    Debug.Log("endId: " + ntc.Id);
                    endGO = false;
                    domain["helix"] = ntc.HelixId;
                    domain["forward"] = Convert.ToBoolean(ntc.Direction);
                    domain["start"] = Math.Min(startId, ntc.Id);
                    domain["end"] = Math.Max(startId, ntc.Id) + 1;
                    if (insertions.Count > 0)
                    {
                        domain["insertions"] = insertions;
                    }
                    if (deletions.Count > 0)
                    {
                        domain["deletions"] = deletions;
                    }
                    domains.Add(domain);
                    domain = new JObject();
                    insertions = new JArray();
                    deletions = new JArray();
                }
            }

            JObject jsonStrand = new JObject
            {
                ["color"] = ColorUtility.ToHtmlStringRGB(strand.GetColor()).ToLower(),
                ["sequence"] = strand.Sequence,
                ["domains"] = domains,
            };
            strands.Add(jsonStrand);
        }

        // Creating entire json file.
        JObject scadnano = new JObject
        {
            ["version"] = "0.19.1",
            ["grid"] = "square", // TODO: fix this to actual grid type
            ["helices"] = helices,
            ["strands"] = strands,
        };

        return scadnano.ToString();
    }

    private void CreateFile(string path, string content, string fileType)
    { 
        if (!path.Contains(fileType))
        {
            path += fileType;
        }
        File.WriteAllText(path, content);
    }

    private void WriteFile(string content, string fileType)
    {
        FileBrowser.Instance.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
        bool status = FileBrowser.ShowSaveDialog((paths) => { CreateFile(paths[0], content, fileType); },
            () => { Debug.Log("Canceled"); },
            FileBrowser.PickMode.Files, false, null, null, "Save", "Save");

        Debug.Log("Download status: " + status);
    }

    IEnumerator CreateTopFile()
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

        string requestResult = request.downloadHandler.text;
        string pattern = @"(jobs[^""&]*)";
        MatchCollection matches = Regex.Matches(requestResult, pattern);

        /* the topology and oxdna file are downloaded from the second and third job links of the GET respone. */

        // Download topology file
        string topFileURL = matches[1].Value;
        // Debug.Log(topFileURL);
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
            string dataInString = Encoding.UTF8.GetString(request.downloadHandler.data);

            while (!FileBrowser.IsOpen)
            {
                yield return new WaitForSeconds(0.5f);
            }

            WriteFile(dataInString, ".top");

            Debug.Log(".top downloaded");
        }
    }

    IEnumerator CreateOxdnaFile()
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

        string requestResult = request.downloadHandler.text;
        string pattern = @"(jobs[^""&]*)";
        MatchCollection matches = Regex.Matches(requestResult, pattern);

        /* the topology and oxdna file are downloaded from the second and third job links of the GET respone. */

        // Download oxdna file
        string oxdnaFileURL = matches[2].Value;
        //Debug.Log(oxdnaFileURL);
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
            string dataInString = Encoding.UTF8.GetString(request.downloadHandler.data);

            WriteFile(dataInString, ".oxdna");

            Debug.Log(".oxdna downloaded");
        }
    }
}
