/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;

public class Togglers : MonoBehaviour
{
    /*public void LineToggled() 
    {
        s_lineTogOn = true;
        s_curveTogOn = false;
        s_gridTogOn = false;
        s_honeycombTogOn = false;
        s_cameraTogOn = false;
    }

    public void CurveToggled() 
    {
        s_lineTogOn = false;
        s_curveTogOn = true;
        s_gridTogOn = false;
        s_honeycombTogOn = false;
        s_cameraTogOn = false;
    }

    public void LoopToggled() 
    {
        s_loopTogOn = !s_loopTogOn;
    }

    public void GridToggled() 
    {
        s_lineTogOn = false;
        s_curveTogOn = false;
        s_gridTogOn = true;
        s_honeycombTogOn = false;
        s_cameraTogOn = false;
    }

    public void HoneycombToggled() 
    {
        s_lineTogOn = false;
        s_curveTogOn = false;
        s_gridTogOn = false;
        s_honeycombTogOn = true;
        s_cameraTogOn = false;
    }

    public void CameraToggled()
    {
        s_lineTogOn = false;
        s_curveTogOn = false;
        s_gridTogOn = false;
        s_honeycombTogOn = false;
        s_cameraTogOn = true;
    }*/

    public void SelectToggled()
    {
        s_selectTogOn = true;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void DrawToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = true;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void EraseToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = true;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void SplitToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = true;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void MergeToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = true;
        s_insTogOn = false;
        s_delTogOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void InsertionToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = true;
        s_delTogOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void DeletionToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = true;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void StencilsToggled()
    {
        s_hideStencils = !s_hideStencils;
    }

    public void NucleotideViewToggled()
    {
        s_strandView = false;
        s_helixView = false;
        s_nucleotideView = true;
    }

    public void StrandViewToggled()
    {
        s_strandView = true;
        s_helixView = false;
        s_nucleotideView = false;
    }

    public void HelixViewToggled()
    {
        s_strandView = false;
        s_helixView = true;
        s_nucleotideView = false;
    }
}
