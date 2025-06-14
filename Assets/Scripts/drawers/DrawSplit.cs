/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using UnityEngine;
using static GlobalVariables;
using static Utils;

/// <summary>
/// Splits strand into two strands.
/// </summary>
public class DrawSplit
{
    public static void DoSplitStrand(NucleotideData nd)
    {
        if (!IsValid(nd)) { return; }
        ICommand command = new SplitCommand(nd);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    /// <param name="id"></param>
    /// <param name="color"></param>
    /// <param name="splitAfter"></param>
    public static void SplitStrand(GameObject go, int id, Color color, bool splitAfter)
    {
        if (!IsValid(go)) { return; }
        var startNtc = go.GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);

        if (splitAfter)
        {
            if (strand.IsCircular)
            {
                strand.SplitCircularAfter(go);
            }
            else
            {
                CreateStrand(strand.SplitAfter(go), id, color);
            }
        }
        else
        {
            if (strand.IsCircular)
            {
                strand.SplitCircularBefore(go);
            }
            else
            {
                CreateStrand(strand.SplitBefore(go), id, color);
            }
        }
    }

    public static void SplitStrand(NucleotideData nd, int strandId, Color color)
    {
        if (!IsValid(nd)) { return; }
        Strand strand = nd.GetStrand();

        bool splitAfter = Convert.ToBoolean(nd.Direction);
        CreateStrandWithoutXovers(strand.Split(nd, splitAfter), strandId, color);
    }

    public static bool IsValid(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        if (!ntc.Selected)
        {
            return false;
        }
        int strandId = ntc.StrandId;
        Strand strand = s_strandDict[strandId];

        if (strand.Head == go || strand.Tail == go)
        {
            return false;
        }

        if (ntc.HasXover)
        {
            return false;
        }

       
        /*for (int i = 0; i < strand.GetXovers().Count; i++)
        {
            if (go == strand.GetXovers()[i].GetComponent<XoverComponent>().NextGO)
            {
                return false;
            }
        }*/

        return true;
    }

    private static bool IsValid(NucleotideData nd)
    {
        if (!nd.IsSelected())
        {
            return false;
        }
        Strand strand = nd.GetStrand();

        if (strand.GetHead() == nd || strand.GetTail() == nd)
        {
            return false;
        }

        if (nd.HasXover)
        {
            return false;
        }


        /*for (int i = 0; i < strand.GetXovers().Count; i++)
        {
            if (go == strand.GetXovers()[i].GetComponent<XoverComponent>().NextGO)
            {
                return false;
            }
        }*/

        return true;
    }
}
