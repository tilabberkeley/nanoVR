/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using Newtonsoft.Json.Linq;
using static GlobalVariables;

public class FileExport : MonoBehaviour
{
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

    public void Export()
    {
        // Creating helices data.
        JArray helices = new JArray();
        foreach (var item in s_helixDict)
        {
            int id = item.Key;
            Helix helix = item.Value;
            JObject jsonHelix = new JObject
            {
                ["grid_position"] = new JArray { helix._gridComponent.GridPoint.X, helix._gridComponent.GridPoint.Y * -1 }, // Negative Y-axis for .sc format 
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

                if ((i == strand.Nucleotides.Count - 1) || (ntc.HasXover() && !isStartGO))
                {
                    endId = ntc.Id;
                    isStartGO = true;
                }
                else if ((i == 0)
                    || (ntc.HasXover() && isStartGO))
                {
                    Debug.Log("startId: " + ntc.Id);
                    Debug.Log("endId: " + endId);
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
                ["color"] = "#" + ColorUtility.ToHtmlStringRGB(strand.GetColor()).ToLower(),
                ["sequence"] = strand.Sequence,
                ["domains"] = domains,
            };
            strands.Add(jsonStrand);
        }

        // Creating entire json file.
        JObject scadnano = new JObject
        {
            ["version"] = "0.19.1",
            ["grid"] = s_helixDict.Values.First()._gridComponent.Grid.Type,
            ["helices"] = helices,
            ["strands"] = strands,
        };

        string json = scadnano.ToString();

        FileBrowser.Instance.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
        FileBrowser.Instance.enabled = true;
        FileBrowser.ShowSaveDialog((paths) => { CreateFile(paths[0], json); }, 
            () => { Debug.Log("Canceled"); }, 
            FileBrowser.PickMode.Files, false, null, null, "Save", "Save");
    }

    private void CreateFile(string path, string content)
    {
        if (!path.Contains(".sc"))
        {
            path += ".sc";
        }
        File.WriteAllText(path, content);
    }
}
