/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections;
using UnityEngine;
using static GlobalVariables;

public class ViewingPerspective : MonoBehaviour
{
    /// <summary>
    /// Singleton which provides a single static instance for this class. Allows other
    /// scripts to access instance methods in this class.
    /// </summary>
    public static ViewingPerspective Instance;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Toggles stencils (Grid circles and unused nucleotides) on and off.
    /// Called by StencilViewTog in View Panel of Menu.
    /// </summary>
    public void ChangeStencilsView()
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
    public void ViewNucleotide()
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

    /// <summary>
    /// Changes viewing mode to Strand View (abstracted Strands).
    /// Called by StrandViewTog in the View Panel of Menu.
    /// </summary>
    public void ViewStrand() 
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
    private IEnumerator ViewStrandHelper()
    {
        foreach (Strand strand in s_strandDict.Values)
        {
            strand.ShowHideCone(false);
            strand.ShowHideXovers(false);
            strand.DrawBezier();
            yield return null;
        }
    }

    public void EditMode()
    {

    }

    public void VisualMode()
    {

    }
}
