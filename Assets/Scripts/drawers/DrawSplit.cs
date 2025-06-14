/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using UnityEngine;
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

    public static void SplitStrand(NucleotideData nd, int strandId, Color color)
    {
        if (!IsValid(nd)) { return; }
        Strand strand = nd.GetStrand();

        bool splitAfter = Convert.ToBoolean(nd.Direction);
        CreateStrandWithoutXovers(strand.Split(nd, splitAfter), strandId, color);
    }

    private static bool IsValid(NucleotideData nd)
    {
        if (!nd.IsSelected())
        {
            return false;
        }

        if (nd.IsHead() || nd.IsTail())
        {
            return false;
        }

        if (nd.HasXover)
        {
            return false;
        }
        return true;
    }
}
