using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

public class ViewingPerspective : MonoBehaviour
{
    public void ChangeStencilsView()
    {
        foreach (Helix helix in s_helixDict.Values)
        {
            helix.ChangeStencilView();
        }
    }

    public void ViewNucleotides()
    {

    }

    public void ViewStrand() { }
    public void ViewHelix() { }
}
