/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections;
using System;
using UnityEngine;
using static GlobalVariables;
using System.Collections.Generic;

public static class ViewingPerspective
{
    private static GameObject s_staticBatchTubesRoot = new GameObject();
    private static GameObject s_staticBatchEndpointsRoot = new GameObject();

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

        foreach (Strand strand in s_strandDict.Values)
        {
            strand.ToNucleotideView();
            strand.ShowHideCone(true);
            // strand.ShowHideXovers(true);
            // strand.SetDomainActivity(false);
        }

        foreach (Helix helix in s_helixDict.Values)
        {
            helix.ChangeRendering();
        }
    }

    /// <summary>
    /// Changes viewing mode to Strand View (abstracted Strands).
    /// Called by StrandViewTog in the View Panel of Menu.
    /// </summary>
    public static void ViewStrand()
    {
        if (!s_strandView) { return; }

        // CoRunner.Instance.Run(ViewStrandHelper());
        List<GameObject> staticBatchingTubes = new List<GameObject>();
        List<GameObject> staticBatchingEndpoints = new List<GameObject>();

        foreach (Strand strand in s_strandDict.Values)
        {
            List<Bezier> strandBeziers = strand.ToStrandView();
            foreach (Bezier bezier in strandBeziers)
            {
                staticBatchingTubes.Add(bezier.Tube);
                staticBatchingEndpoints.Add(bezier.Endpoint0);
                staticBatchingEndpoints.Add(bezier.Endpoint1);
            }

            strand.ShowHideCone(false);
            // strand.ShowHideXovers(true);
            // strand.SetDomainActivity(false);
        }

        foreach (Helix helix in s_helixDict.Values)
        {
            helix.ChangeRendering();
        }

        StaticBatchingUtility.Combine(staticBatchingTubes.ToArray(), s_staticBatchTubesRoot);
        StaticBatchingUtility.Combine(staticBatchingEndpoints.ToArray(), s_staticBatchEndpointsRoot);
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
            strand.ToStrandView();
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