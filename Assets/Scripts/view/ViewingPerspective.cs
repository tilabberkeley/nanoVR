/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections;
using System;
using UnityEngine;
using static GlobalVariables;

public static class ViewingPerspective
{
    /// <summary>
    /// Toggles stencils (Grid circles and unused nucleotides) on and off.
    /// Called by StencilViewTog in View Panel of Menu.
    /// </summary>
    public static void ChangeStencilsView()
    {
        foreach (DNAGrid grid in s_gridDict.Values)
        {
            grid.ChangeStencilView();
        }
    }

    /// <summary>
    /// Changes viewing mode to Nucleotide View (individual nucleotides).
    /// Called by NucleotideViewTog in the View Panel of Menu.
    /// </summary>
    public static void ViewNucleotide()
    {
        if (!s_nucleotideView) { return; }

        foreach (Helix helix in s_helixDict.Values)
        {
            helix.ChangeRendering();
        }

        foreach (Strand strand in s_strandDict.Values)
        {
            strand.DeleteBezier();
            strand.ShowHideCone(true);
            // strand.ShowHideXovers(true);
            // strand.SetDomainActivity(false);
        }

    }

    /// <summary>
    /// Changes viewing mode to Strand View (abstracted Strands).
    /// Called by StrandViewTog in the View Panel of Menu.
    /// </summary>
    public static void ViewStrand()
    {
        if (!s_strandView) { return; }

        foreach (Helix helix in s_helixDict.Values)
        {
            helix.ChangeRendering();
        }

        CoRunner.Instance.Run(ViewStrandHelper());
    }

    /// <summary>
    /// Helper Coroutine for converting all Strands into abstracted Strand View.
    /// </summary>
    private static IEnumerator ViewStrandHelper()
    {
        if (!s_strandView) { yield return null; }
        foreach (Strand strand in s_strandDict.Values)
        {
            strand.ShowHideCone(false);
            // strand.ShowHideXovers(false);
            strand.DrawBezier();
            // strand.SetDomainActivity(true);
            yield return null;
        }
    }

    public static void EditMode()
    {
        throw new NotImplementedException();
    }

    public static void VisualMode()
    {
        throw new NotImplementedException();
    }
}