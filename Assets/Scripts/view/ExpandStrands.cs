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
    private static GameObject[] scene;
    [SerializeField] private Toggle toggle;

    public void ShowFullStrandToggle()
    {
        if (toggle.isOn)
        {
            Expand();
            s_visualMode = true;
        }
        else
        {
            Contract();
            s_visualMode = false;
        }
    }

    private void Expand()
    {
        string json = GetSCJSON(); // FIX: This is a copy so need to figure out a way to not get key collisions in dictionaries!
                                   // Maybe have temp dictionaries in GlobalVariables to help with visual mode?
        scene = GameObject.FindGameObjectsWithTag("Nucleotide");
        foreach (var go in scene)
        {
            go.SetActive(false);
        }
        StartCoroutine(ParseSC(json));
        //FileImport.ParseSC(json, false);
    }

    public void Contract()
    {
        var rootObjects = GameObject.FindGameObjectsWithTag("Nucleotide");
        foreach (var go in rootObjects)
        {
            GameObject.Destroy(go);
        }
        foreach (var go in scene)
        {
            go.SetActive(true);
        }
    }

    private IEnumerator ParseSC(string fileContents)
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

                DrawGrid.CreateGrid(gridName, "XY", new Vector3(x, y, z), gridType, true);
                yield return new WaitForSeconds(0.1f);
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
            int length = (int)helices[i]["max_offset"];
            string gridName;
            if (helices[i]["group"] != null)
            {
                gridName = CleanSlash(helices[i]["group"].ToString());
                
            }
            else
            {
                gridName = (s_numGrids - 1).ToString();
            }

            DNAGrid grid = s_visGridDict[gridName];
            int xInd = grid.GridXToIndex((int)coord[0]);
            int yInd = grid.GridYToIndex((int)(coord[1]) * -1);
            GridComponent gc = grid.Grid2D[xInd, yInd];
            grid.AddHelix(s_numHelices, new Vector3(gc.GridPoint.X, gc.GridPoint.Y, 0), length, "XY", gc, true);
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
                isScaffold = (bool)strands[i]["is_scaffold"];
            }
            List<GameObject> nucleotides = new List<GameObject>();
            List<GameObject> xoverEndpoints = new List<GameObject>();
            List<GameObject> sDeletions = new List<GameObject>();
            List<(GameObject, int)> sInsertions = new List<(GameObject, int)>();
            for (int j = 0; j < domains.Count; j++)
            {
                int helixId = (int)domains[j]["helix"] + prevNumHelices;
                bool forward = (bool)domains[j]["forward"];
                int startId = (int)domains[j]["start"];
                int endId = (int)domains[j]["end"] - 1; // End id is exclusive in .sc file
                JArray deletions = new JArray();
                JArray insertions = new JArray();
                if (domains[j]["deletions"] != null) { deletions = JArray.Parse(domains[j]["deletions"].ToString()); }
                if (domains[j]["insertions"] != null) { deletions = JArray.Parse(domains[j]["insertions"].ToString()); }
                Helix helix = s_helixDict[helixId];

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

    private string GetSCJSON()
    {
        JObject groups = new JObject();
        foreach (var item in s_gridDict)
        {
            string gridId = item.Key;
            DNAGrid grid = item.Value;
            string name = gridId.ToString();
            JObject position = new JObject();
           
            position["x"] = grid.StartPos.x;
            position["y"] = grid.StartPos.y;
            position["z"] = grid.StartPos.z;

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
            int numDeletions = 0;
            int numInsertions = 0;
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
                    numDeletions += 1;
                }

                if (ntc.IsInsertion)
                {
                    numInsertions += ntc.Insertion;
                }

                if ((i == strand.Nucleotides.Count - 1) || (ntc.HasXover && !isStartGO))
                {
                    endId = ntc.Id - numDeletions + numInsertions;
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
