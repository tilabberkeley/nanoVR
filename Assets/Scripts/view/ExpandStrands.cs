using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GlobalVariables;

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

        FileImport.ParseSC(json, false);
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
