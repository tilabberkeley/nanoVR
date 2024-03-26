/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;

public class ViewingPerspective : MonoBehaviour
{
    public void ChangeStencilsView()
    {
        foreach (DNAGrid grid in s_gridDict.Values)
        {
            grid.ChangeStencilView();
        }
      /*  foreach (Helix helix in s_helixDict.Values)
        {
            helix.ChangeStencilView();
        }*/
    }

    public static void ViewNucleotide()
    {
        if (!s_nucleotideView) { return; }
        foreach (Strand strand in s_strandDict.Values)
        {
            strand.DeleteBezier();
            strand.ShowHideCone(true);
            strand.ShowHideXovers(true);
        }
        foreach (Helix helix in s_helixDict.Values)
        {
            helix.ChangeRendering();
        }
    }

    public static void ViewStrand() 
    {
        if (!s_strandView) { return; }

        foreach (Helix helix in s_helixDict.Values)
        {
            helix.ChangeRendering();
        }

        foreach (Strand strand in s_strandDict.Values)
        {
            strand.ShowHideCone(false);
            strand.ShowHideXovers(false);
            strand.DrawBezier();
        }
    }

    public void EditMode()
    {

    }

    public void VisualMode()
    {

    }
    public static void ViewHelix() { }
}
