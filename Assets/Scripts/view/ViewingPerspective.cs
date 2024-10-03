/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections;
using static GlobalVariables;
using System.Collections.Generic;
using UnityEngine;

public static class ViewingPerspective
{
    private static GameObject s_staticBatchTubesRoot = new GameObject();
    private static GameObject s_staticBatchEndpointsRoot = new GameObject();

    /// <summary>
    /// Changes viewing mode to Nucleotide View (individual nucleotides).
    /// Called by NucleotideViewTog in the View Panel of Menu.
    /// </summary>
    public static IEnumerator ViewNucleotide()
    {
        //if (!s_nucleotideView) { return; }

        foreach (Strand strand in s_strandDict.Values)
        {
            strand.ToNucleotideView();
            strand.ShowHideCone(true);
            // strand.ShowHideXovers(true);
            // strand.SetDomainActivity(false);
            yield return null;

        }

        foreach (Helix helix in s_helixDict.Values)
        {
            helix.DestroyCylinder();
            helix.ChangeRendering();
            yield return null;

        }
    }

    /// <summary>
    /// Changes viewing mode to Strand View (abstracted Strands).
    /// Called by StrandViewTog in the View Panel of Menu.
    /// </summary>
    public static IEnumerator ViewStrand()
    {
        //if (!s_strandView) { return; }

        // CoRunner.Instance.Run(ViewStrandHelper());
        s_strandView = true;
        s_helixView = false;
        s_nucleotideView = false;

        List<GameObject> staticBatchingTubes = new List<GameObject>();
        List<GameObject> staticBatchingEndpoints = new List<GameObject>();

        foreach (Strand strand in s_strandDict.Values)
        {
            List<Bezier> strandBeziers = strand.ToStrandView();
            foreach (Bezier bezier in strandBeziers)
            {
                GameObject tube = bezier.Tube;
                tube.isStatic = true;
                staticBatchingTubes.Add(tube);

                staticBatchingEndpoints.Add(bezier.Endpoint0);
                staticBatchingEndpoints.Add(bezier.Endpoint1);
            }

            strand.ShowHideCone(false);
            // strand.ShowHideXovers(true);
            // strand.SetDomainActivity(false);
            yield return null;
        }

        foreach (Helix helix in s_helixDict.Values)
        {
            helix.DestroyCylinder();
            helix.ChangeRendering();
        }

        StaticBatchingUtility.Combine(staticBatchingTubes.ToArray(), s_staticBatchTubesRoot);
        StaticBatchingUtility.Combine(staticBatchingEndpoints.ToArray(), s_staticBatchEndpointsRoot);
    }

    public static IEnumerator ViewHelix()
    {
        foreach (Strand strand in s_strandDict.Values)
        {
            strand.ToHelixView();
            strand.ShowHideCone(false);
            yield return null;
        }

        foreach (Helix helix in s_helixDict.Values)
        {
            helix.CreateCylinder();
            helix.ChangeRendering();
            yield return null;
        }
    }
}