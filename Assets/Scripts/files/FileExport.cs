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
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using static GlobalVariables;
using static Utils;
using System.Reflection; // This is used by Android during build process

public class FileExport : MonoBehaviour
{
    // URLs for tacoxdna website.
    private const string postURL = "http://tacoxdna.sissa.it/scadnano_oxDNA/submit";
    private const string tacoURL = "http://tacoxdna.sissa.it";

    [SerializeField] private Dropdown exportTypeDropdown;
    [SerializeField] private Canvas Menu;
    [SerializeField] private Toggle includeScaffoldTog;
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
        /*fileBrowser.gameObject.SetActive(true);
        Menu.enabled = false;*/

        string exportType = exportTypeDropdown.options[exportTypeDropdown.value].text;

        if (exportType.Equals("scadnano"))
        {
            WriteSCFile();
        }
        else if (exportType.Equals("oxdna"))
        {
            WriteOxdnaFiles();
        }
        else
        {
            WriteCSVFile();
        }
    }

    private string GetSCJSON(bool isOxDNA = false)
    {
        return GetSCJSON(new List<string>(s_gridDict.Keys), false, isOxDNA);
    }

    /// <summary>
    /// Returns scadnano JSON format of given grids. If isCopyPaste boolean is set to true, the position of the copied grid is 
    /// set to (0, 0, 0). This accounts for the positioning when it is pasted back into the world.
    /// </summary>
    /// <param name="gridIds">List of grid ids associated with the DNAGrid objects that are being written to JSON</param>
    /// <param name="isCopyPaste">Whether or not we are copy pasting a DNAGrid object</param>
    /// <returns>Returns JSON string in scadnano format</returns>
    public static string GetSCJSON(List<string> gridIds, bool isCopyPaste = false, bool isOxDNA = false)
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
                position["x"] = grid.Position.x * SCALE * -1; // TODO: Check this is right
                position["y"] = grid.Position.y * SCALE;
                position["z"] = grid.Position.z * SCALE;
            }

            /* Converts Unity quaternion into pitch, yaw, roll for scadnano json. 
             * @source: https://discussions.unity.com/t/finding-pitch-roll-yaw-from-quaternions/65684/3
             */
            Quaternion q = grid.StartGridCircle.transform.rotation;
            float pitch = Utils.ToPitch(q);
            float yaw = Utils.ToYaw(q);
            float roll = Utils.ToRoll(q);

            Quaternion localQ = grid.StartGridCircle.transform.localRotation;
            float pitch_local = Utils.ToPitch(localQ);
            float yaw_local = Utils.ToYaw(localQ);
            float roll_local = Utils.ToRoll(localQ);
            Debug.Log($"pitch: {pitch}, yaw: {yaw}, roll: {roll}");

            Vector3 eulerAngles = localQ.eulerAngles;
            Debug.Log($"euler pitch: {eulerAngles.x}, euler yaw: {eulerAngles.y}, euler roll: {eulerAngles.z}");
            /*float pitch = Mathf.Rad2Deg * Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z);
            float yaw = Mathf.Rad2Deg * Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z);
            float roll = Mathf.Rad2Deg * Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w);*/
            /*Quaternion q = grid.StartGridCircle.transform.rotation;
            Vector3 zAxis = Vector3.forward;
            Vector3 xAxis = Vector3.right;
            Vector3 yAxis = Vector3.up;
            q.ToAngleAxis(out float pitch, out zAxis);
            q.ToAngleAxis(out float roll, out xAxis);
            q.ToAngleAxis(out float yaw, out zAxis);*/


            JObject group = new JObject
            {
                ["position"] = position,
                ["pitch"] = pitch_local,
                ["roll"] = roll_local,
                ["yaw"] = yaw_local,
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

            JArray gridPosition = new JArray { helix._gridComponent.GridPoint.X, helix._gridComponent.GridPoint.Y * -1 }; // Negative Y-axis for .sc format 
            if (isOxDNA)
            {
                gridPosition = new JArray { helix._gridComponent.GridPoint.X, helix._gridComponent.GridPoint.Y };
            }

            JObject jsonHelix = new JObject
            {
                ["grid_position"] = gridPosition,
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

            // Skip strands that span multiple grids when we are copy/pasting a single grid
            else if (isCopyPaste && strand.MoreThanOneGrid())
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
                    JObject domain;

                    if (!ntc.IsExtension)
                    {
                        domain = new JObject
                        {
                            ["helix"] = ntc.HelixId,
                            ["forward"] = Convert.ToBoolean(ntc.Direction),
                            ["start"] = Math.Min(ntc.Id, endId),
                            ["end"] = Math.Max(ntc.Id, endId) + 1, // + 1 accounts for .sc endId being exclusive
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
                    }
                    else
                    {
                        domain = new JObject
                        {
                            ["extension_num_bases"] = Math.Abs(ntc.Id - endId) + 1,
                        };
                    }
                    
                    domains.Add(domain);
                    insertions.Clear();
                    deletions.Clear();
                }

                // Adds loopout objects
                if (ntc.HasXover)
                {
                    LoopoutComponent loopComp = ntc.Xover.GetComponent<LoopoutComponent>();
                    if (loopComp != null && loopComp.NextGO == ntc.gameObject)
                    {
                        JObject loopout = new JObject
                        {
                            ["loopout"] = loopComp.SequenceLength,
                        };
                        domains.Add(loopout);
                    }
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
    private void WriteSCFile()
    {
        fileBrowser.enabled = true;
        fileBrowser.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
        bool result = FileBrowser.ShowSaveDialog((paths) => { CreateSCFile(paths[0], GetSCJSON()); },
            () => { Debug.Log("Canceled"); },
            FileBrowser.PickMode.Files, false, null, null, "Save", "Save");
    }

    /// <summary>
    /// Creates .csv file and writes to it given the file path and content.
    /// </summary>
    /// <param name="path">File path to write to.</param>
    /// <param name="content">Content of the .csv file.</param>
    private void CreateCSVFile(string path, string content)
    {
        if (!path.Contains(".csv"))
        {
            path += ".csv";
        }
        File.WriteAllText(path, content);
    }

    /// <summary>
    /// Writes .csv file to file browser.
    /// </summary>
    /// <param name="content">Contennt of the .csv file.</param>
    private void WriteCSVFile()
    {
        fileBrowser.enabled = true;
        fileBrowser.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
        bool result = FileBrowser.ShowSaveDialog((paths) => { CreateCSVFile(paths[0], GetCSV()); },
            () => { Debug.Log("Canceled"); },
            FileBrowser.PickMode.Files, false, null, null, "Save", "Save");
    }

    /// <summary>
    /// Returns CSV file of DNA seqeunces for all Strands.
    /// </summary>
    private string GetCSV()
    {
        bool includeScaffold = includeScaffoldTog.isOn;
        StringBuilder csv = new StringBuilder();

        foreach (Strand strand in s_strandDict.Values)
        {
            if (!includeScaffold && strand.IsScaffold)
            {
                continue;
            }
            NucleotideComponent startNtc = strand.Nucleotides.Last().GetComponent<NucleotideComponent>();
            NucleotideComponent endNtc = strand.Nucleotides[0].GetComponent<NucleotideComponent>();

            int startHelixId = startNtc.HelixId;
            int startNuclId = startNtc.Id;
            int endHelixId = endNtc.HelixId;
            int endNuclId = endNtc.Id;
            string sequence = strand.Sequence.Replace("X", ""); // Remove 'X' (deletion) from sequence
            string strandName = string.Format("ST{0}[{1}]{2}[{3}]", startHelixId, startNuclId, endHelixId, endNuclId);
            if (strand.IsScaffold)
            {
                strandName = strandName.Replace("ST", "SCAF");
            }
            string strandText = string.Format("{0}, {1}", strandName, sequence);

            csv.AppendLine(strandText);
        }
        return csv.ToString();
    }

    /// <summary>
    /// Creates .top and .oxdna files and writes to them given the file path and content.
    /// </summary>
    /// <param name="path">File path to write to.</param>
    /// <param name="topContent">Content of .top file.</param>
    /// <param name="oxdnaContent">Content of .oxdna file.</param>
    private void CreateOxdnaFiles(string path, string topContent, string oxdnaContent)
    {
        string topPath = path + ".top";
        string oxdnaPath = path + ".oxdna";

        File.WriteAllText(topPath, topContent);
        File.WriteAllText(oxdnaPath, oxdnaContent);
    }

    /// <summary>
    /// Writes the .top and .oxdna files to file browser.
    /// </summary>
    private void WriteOxdnaFiles()
    {
        bool result = FileBrowser.ShowSaveDialog((paths) => {
            GenerateOxDNAFiles(out string topFile, out string oxdnaFile, out OxDNAMapper oxDNAMapper);
            CreateOxdnaFiles(paths[0], topFile, oxdnaFile); 
        },
        () => { Debug.Log("Canceled"); },
        FileBrowser.PickMode.Files, false, null, null, "Save", "Save");

        Debug.Log("Download result: " + result);
    }

    /// <summary>
    /// Generates string contents of top and dat files for orgiami. Puts them in
    /// respective out variables. Additionally, the generation of the oxDNA files will 
    /// map line indexes to their nucleotide game objects with the given OxDNAMapper.
    /// This is used for simulation.
    /// </summary>
    public void GenerateOxDNAFiles(out string topFile, out string datFile, out OxDNAMapper oxDNAMapper)
    {
        StringBuilder topFileStringBuilder = new StringBuilder();
        StringBuilder datFileStringBuilder = new StringBuilder();
        oxDNAMapper = new OxDNAMapper();

        // Write dat file metadata
        datFileStringBuilder.Append("t = 0" + Environment.NewLine);
        datFileStringBuilder.Append("b = 92 92 92" + Environment.NewLine); // TODO: calculate correct box
        datFileStringBuilder.Append("E = 0 0 0" + Environment.NewLine);

        int numNucleotides = 0;
        int strandCounter = 1;
        int globalNucleotideIndex = -1;
        // Line number will increase as the nucleotides are parsed.
        int lineIndex = 0;

        // Iterate through all the strands
        foreach (Strand strand in s_strandDict.Values)
        {
            // Iterate through nucleotides of each strand
            foreach (GameObject nucleotide in strand.Nucleotides)
            {
                NucleotideComponent nucleotideComponent = nucleotide.GetComponent<NucleotideComponent>();
                if (nucleotideComponent == null || nucleotideComponent.IsDeletion)
                {
                    continue;
                }

                int prime5 = globalNucleotideIndex;
                int prime3 = globalNucleotideIndex + 2;

                // Beginning and end of strand edge cases
                if (nucleotide == strand.Head)
                {
                    prime5 = -1;
                }
                else if (nucleotide == strand.Tail)
                {
                    prime3 = -1;
                }

                topFileStringBuilder.Append($"{strandCounter} {nucleotideComponent.Sequence} {prime5} {prime3}" + Environment.NewLine);
                datFileStringBuilder.Append($"{nucleotideComponent.OxDNAPosition().x:F16} {nucleotideComponent.OxDNAPosition().y:F16} {nucleotideComponent.OxDNAPosition().z:F16} " +
                                            $"{nucleotideComponent.A1.x:F16} {nucleotideComponent.A1.y:F16} {nucleotideComponent.A1.z:F16} " +
                                            $"{nucleotideComponent.A3.x:F16} {nucleotideComponent.A3.y:F16} {nucleotideComponent.A3.z:F16} " +
                                            "0 0 0 0 0 0" + Environment.NewLine); // F16's add precision to the file writes.

                // Add mapping from line number to nucleotide
                oxDNAMapper.Add(lineIndex, nucleotide);

                numNucleotides++;
                globalNucleotideIndex++;
                lineIndex++;
            }

            strandCounter++;
        }

        // Add top file metadata
        topFileStringBuilder.Insert(0, $"{numNucleotides} {strandCounter - 1}" + Environment.NewLine);

        topFile = topFileStringBuilder.ToString();
        datFile = datFileStringBuilder.ToString();
    }

    /// <summary>
    /// Coroutine that downloads .top and .oxdna files from tacoxDNA using scandnano and writes them.
    /// </summary>
    /// <returns>Coroutine.</returns>
    IEnumerator CreateOxdnaFiles()
    {
        // get scadnano file structure
        byte[] data = Encoding.UTF8.GetBytes(GetSCJSON(true));

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

        // WriteOxdnaFiles(topContent, oxdnaContent);
    }
}
