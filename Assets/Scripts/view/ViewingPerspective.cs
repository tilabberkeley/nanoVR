/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections;
using static GlobalVariables;
using UnityEngine;
using System.Collections.Generic;

public class ViewingPerspective : MonoBehaviour
{
    //private static GameObject s_staticBatchTubesRoot = new GameObject();
    //private static GameObject s_staticBatchEndpointsRoot = new GameObject();

    /// <summary>
    /// Changes viewing mode to Nucleotide View (individual nucleotides).
    /// Called by NucleotideViewBtn in the View Panel of Menu.
    /// </summary>
    public void ViewNucleotide()
    {
        CoRunner.Instance.Run(ViewNucleotide(SelectGrid.Grids));
    }

    private IEnumerator ViewNucleotide(List<DNAGrid> grids)
    {
        //if (!s_nucleotideView) { return; }
        /*s_strandView = false;
        s_helixView = false;
        s_nucleotideView = true;*/

        foreach (DNAGrid grid in grids)
        {
            foreach (GridComponent gc in grid.GridComponents)
            {
                if (gc.Helix != null)
                {
                    gc.Helix.ToNucleotideView();
                    yield return null;
                }
            }
        }
     
        /*foreach (Helix helix in s_helixDict.Values)
        {
            helix.DestroyCylinder();
            //helix.ChangeRendering();
            yield return null;

        }*/
    }

    /// <summary>
    /// Changes viewing mode to Strand View (abstracted Strands).
    /// Called by StrandViewTog in the View Panel of Menu.
    /// </summary>
    /*public static IEnumerator ViewStrand()
    {
        s_strandView = true;
        s_helixView = false;
        s_nucleotideView = false;

        foreach (Helix helix in s_helixDict.Values)
        {
            helix.DestroyCylinders();
            helix.ChangeRendering();
        }

        List<GameObject> staticBatchingTubes = new List<GameObject>();
        List<GameObject> staticBatchingEndpoints = new List<GameObject>();

        foreach (Strand strand in s_strandDict.Values)
        {
            List<Bezier> strandBeziers = strand.ToStrandView();
            *//*foreach (Bezier bezier in strandBeziers)
            {
                GameObject tube = bezier.Tube;
                tube.isStatic = true;
                staticBatchingTubes.Add(tube);

                staticBatchingEndpoints.Add(bezier.Endpoint0);
                staticBatchingEndpoints.Add(bezier.Endpoint1);
            }*//*

            strand.ShowHideCone(false);
            // strand.ShowHideXovers(true);
            // strand.SetDomainActivity(false);
            yield return null;
        }

        //StaticBatchingUtility.Combine(staticBatchingTubes.ToArray(), s_staticBatchTubesRoot);
        //StaticBatchingUtility.Combine(staticBatchingEndpoints.ToArray(), s_staticBatchEndpointsRoot);
    }*/

    /// <summary>
    /// Changes viewing mode to Helix View (cylinders).
    /// Called by HelixViewBtn in the View Panel of Menu.
    /// </summary>
    public void ViewHelix()
    {
        CoRunner.Instance.Run(ViewHelix(SelectGrid.Grids));
    }

    private IEnumerator ViewHelix(List<DNAGrid> grids)
    {
        /*s_strandView = false;
        s_helixView = true;
        s_nucleotideView = false;*/

        /*foreach (Strand strand in s_strandDict.Values)
        {
            strand.ToHelixView();
            strand.ShowHideCone(false);
            yield return null;
        }*/

        foreach (DNAGrid grid in grids)
        {
            foreach (GridComponent gc in grid.GridComponents)
            {
                if (gc.Helix != null)
                {
                    gc.Helix.ToHelixView();
                    yield return null;
                }
            }
        }
    }
}