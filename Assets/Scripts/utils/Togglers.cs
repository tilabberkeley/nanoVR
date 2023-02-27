using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        DrawNucleotide.ResetGameObjects();

    }

    public void EraseToggled()
    {
        s_drawTogOn = false;
        s_eraseTogOn = true;
        DrawNucleotide.ResetGameObjects();
    }
}
