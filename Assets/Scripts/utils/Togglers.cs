/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;

public class Togglers : MonoBehaviour
{
    public void LineToggled() 
    {
        s_lineTogOn = true;
        s_curveTogOn = false;
        s_gridTogOn = false;
        s_honeycombTogOn = false;
    }

    public void CurveToggled() 
    {
        s_lineTogOn = false;
        s_curveTogOn = true;
        s_gridTogOn = false;
        s_honeycombTogOn = false;
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
    }

    public void HoneycombToggled() 
    {
        s_lineTogOn = false;
        s_curveTogOn = false;
        s_gridTogOn = false;
        s_honeycombTogOn = true;
    }

    public void DrawToggled()
    {
        s_drawTogOn = true;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        DrawNucleotide.ResetNucleotides();
    }

    public void EraseToggled()
    {
        s_drawTogOn = false;
        s_eraseTogOn = true;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        DrawNucleotide.ResetNucleotides();
    }

    public void SplitToggled()
    {
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = true;
        s_mergeTogOn = false;
        DrawNucleotide.ResetNucleotides();
    }

    public void MergeToggled()
    {
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = true;
        DrawNucleotide.ResetNucleotides();
    }
}
