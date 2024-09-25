using System;
using UnityEngine;
using static GlobalVariables;

public class ViewChanger : MonoBehaviour
{
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

    public void EditMode()
    {
        throw new NotImplementedException();
    }

    public void VisualMode()
    {
        throw new NotImplementedException();
    }
}
