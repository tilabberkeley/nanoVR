using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Togglers : MonoBehaviour
{
    public void LineToggled() 
    {
        GlobalVariables.s_lineTogOn = true;
        GlobalVariables.s_curveTogOn = false;
        GlobalVariables.s_gridTogOn = false;
        GlobalVariables.s_honeycombTogOn = false;

    }

    public void CurveToggled() 
    {
        GlobalVariables.s_lineTogOn = false;
        GlobalVariables.s_curveTogOn = true;
        GlobalVariables.s_gridTogOn = false;
        GlobalVariables.s_honeycombTogOn = false;

    }

    public void LoopToggled() 
    {
        GlobalVariables.s_loopTogOn = !GlobalVariables.s_loopTogOn;
    }

    public void GridToggled() 
    {
        GlobalVariables.s_lineTogOn = false;
        GlobalVariables.s_curveTogOn = false;
        GlobalVariables.s_gridTogOn = true;
        GlobalVariables.s_honeycombTogOn = false;
    }

    public void HoneycombToggled() 
    {
        GlobalVariables.s_lineTogOn = false;
        GlobalVariables.s_curveTogOn = false;
        GlobalVariables.s_gridTogOn = false;
        GlobalVariables.s_honeycombTogOn = true;
    }

    public void DrawToggled()
    {
        GlobalVariables.s_drawTogOn = true;
        GlobalVariables.s_eraseTogOn = false;
        DrawNucleotide.ResetGameObject();

    }

    public void EraseToggled()
    {
        GlobalVariables.s_drawTogOn = false;
        GlobalVariables.s_eraseTogOn = true;
        DrawNucleotide.ResetGameObject();
    }
}
