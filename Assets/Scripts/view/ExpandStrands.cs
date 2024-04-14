/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using static GlobalVariables;
using static Utils;

public class ExpandStrands : MonoBehaviour
{
    [SerializeField] private Toggle toggle;

    public void ShowFullStrandToggle()
    {
        if (s_hideStencils) { return; } // Cannot handle no stencil conversion yet
        if (s_strandView) { return; }
        if (toggle.isOn)
        {
            s_visualMode = true;
            ConvertToVisualMode();
        }
        else
        {
            s_visualMode = false;
            ConvertToEditMode();
        }
    }

    private void ConvertToVisualMode()
    {
        string json = FileExport.GetSCJSON(new List<string>(s_gridDict.Keys), false); // TODO: Test this
        foreach (GameObject go in allGameObjects)
        {
            go.SetActive(false);
        }
        //StartCoroutine(ParseSC(json));
        FileImport.ParseSC(json, false, true); // TODO: Test this
    }

    public void ConvertToEditMode()
    {
        foreach (GameObject go in allVisGameObjects)
        {
            go.SetActive(false);
        }
        foreach (GameObject go in allGameObjects)
        {
            go.SetActive(true);
        }
        //StartCoroutine(DestroyVisGameObjects());
        DestroyVisGameObjects();
        ResetVisualGlobalVariables();
    }

    private void ResetVisualGlobalVariables()
    {
        s_visGridDict.Clear();
        s_visHelixDict.Clear();
        s_visStrandDict.Clear();
        s_numVisGrids = 0;
        s_numVisHelices = 0;
        s_numVisStrands = 0;
    }

    private void DestroyVisGameObjects()
    {
        foreach (GameObject go in allVisGameObjects)
        {
            Destroy(go);
        }
    }

    private void ParseSC(string fileContents)
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
                DrawGrid.CreateGrid(gridName, "XY", new Vector3(x, y, z), gridType);
            }
        }

        /*else
        {
            string gridType = CleanSlash(origami["grid"].ToString());
            DrawGrid.CreateGrid(s_numGrids.ToString(), "XY", rayInteractor.transform.position + new Vector3(0, 0, 0), gridType);
        }*/

        int prevNumHelices = s_numVisHelices;
        for (int i = 0; i < helices.Count; i++)
        {
            JArray coord = JArray.Parse(helices[i]["grid_position"].ToString());
            int length = (int) helices[i]["max_offset"];
            string gridName;
            if (helices[i]["group"] != null)
            {
                gridName = CleanSlash(helices[i]["group"].ToString());
                
            }
            else
            {
                gridName = (s_numVisGrids - 1).ToString();
            }

            DNAGrid grid = s_visGridDict[gridName];
            int xInd = grid.GridXToIndex((int)coord[0]);
            int yInd = grid.GridYToIndex((int)(coord[1]) * -1);
            GridComponent gc = grid.Grid2D[xInd, yInd];
            grid.AddHelix(s_numVisHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), length, "XY", gc);
            grid.CheckExpansion(gc);
        }

        for (int i = 0; i < strands.Count; i++)
        {
            JArray domains = JArray.Parse(strands[i]["domains"].ToString());
            string sequence = CleanSlash(strands[i]["sequence"].ToString());
            string hexColor = CleanSlash(strands[i]["color"].ToString());
            ColorUtility.TryParseHtmlString(hexColor, out Color color);
            int strandId = s_numVisStrands;
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
                Helix helix = s_visHelixDict[helixId];

                // Store deletions and insertions.
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

        }
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

    /// <summary>
    /// Gets Scadnano JSON of all grids and expands/contracts the insertions/deletions to get true visual view of structure.
    /// </summary>
    /// <returns>Returns scadnano JSON string</returns>
    private string GetSCJSON()
    {
        JObject groups = new JObject();
        foreach (var item in s_gridDict)
        {
            string gridId = item.Key;
            DNAGrid grid = item.Value;
            string name = gridId.ToString();
            JObject position = new JObject();
           
            position["x"] = grid.Position.x;
            position["y"] = grid.Position.y;
            position["z"] = grid.Position.z;

            JObject group = new JObject
            {
                ["position"] = position,
                ["grid"] = grid.Type,
            };
            groups[name] = group;
        }


        // Creating helices data.
        JArray helices = new JArray();
        foreach (var item in s_helixDict)
        {
            int id = item.Key;
            Helix helix = item.Value;
            JObject jsonHelix = new JObject
            {
                ["grid_position"] = new JArray { helix._gridComponent.GridPoint.X, helix._gridComponent.GridPoint.Y * -1 }, // Negative Y-axis for .sc format 
                ["group"] = helix.GridId.ToString(),
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
            if (strand.MoreThanOneGrid())
            {
                continue;
            }

            JArray domains = new JArray();
            bool isStartGO = false;
            NucleotideComponent firstNtc = null;

            // Creating domains data for each strand.
            for (int i = strand.Nucleotides.Count - 1; i >= 0; i--)
            {
                var nt = strand.Nucleotides[i];
                var ntc = nt.GetComponent<NucleotideComponent>();
                if (ntc == null)
                {
                    continue;
                }

                if ((i == strand.Nucleotides.Count - 1) || (ntc.HasXover && !isStartGO))
                {
                    firstNtc = ntc; //- numDeletions + numInsertions;
                    isStartGO = true;
                }
                else if ((i == 0) || (ntc.HasXover && isStartGO))
                {
                    isStartGO = false;
                    int startId = Math.Min(ntc.Id, firstNtc.Id);
                    int endId = Math.Max(ntc.Id, firstNtc.Id);
                    int ntcShift = ntc.NumModsToLeft();
                    int firstNtcShift = firstNtc.NumModsToLeft();

                    if (ntc.Id < firstNtc.Id)
                    {
                        startId += ntcShift;
                        endId += firstNtcShift;
                    } 
                    else
                    {
                        startId += firstNtcShift;
                        endId += ntcShift;
                    }

                    JObject domain = new JObject
                    {
                        ["helix"] = ntc.HelixId,
                        ["forward"] = Convert.ToBoolean(ntc.Direction),
                        ["start"] = startId,
                        ["end"] = endId + 1, // +1 accounts for .sc endId being exclusive
                    };
                    domains.Add(domain);
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
}
