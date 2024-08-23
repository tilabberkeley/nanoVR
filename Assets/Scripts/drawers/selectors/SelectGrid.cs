/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

public class SelectGrid : MonoBehaviour
{
    private static DNAGrid s_grid = null;

    public static DNAGrid Grid { get { return s_grid; } }

    [SerializeField] private XRNode _XRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    private bool triggerReleased = true;
    //private bool axisReleased = true;

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_XRNode, _devices);
        if (_devices.Count > 0)
        {
            _device = _devices[0];
        }
    }

    private void OnEnable()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }
    }

    private void Update()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }

        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);

        // Resets selected grid.
        if (triggerValue && triggerReleased && !rayInteractor.TryGetCurrent3DRaycastHit(out _))
        {
            triggerReleased = false;
            //UnhighlightGrid(s_grid);
            //Reset();
        }

        // Resets trigger.                                          
        if (!triggerValue)
        {
            triggerReleased = true;
        }
    }

    /// <summary>
    /// Resets the grid.
    /// </summary>
    private static void Reset()
    {
        s_grid = null;
    }

    /*public static void HighlightGrid(string gridId)
    {
        UnhighlightGrid(s_grid);
        s_gridDict.TryGetValue(gridId, out DNAGrid grid);
        s_grid = grid;
        foreach (GridComponent gc in grid.Grid2D)
        {
            Highlight.HighlightGridCircle(gc);
            if (gc.Helix != null)
            {
                Highlight.HighlightHelix(gc.Helix);
            }
        }
    }

    public void UnhighlightGrid()
    {
        UnhighlightGrid(s_grid);
    }

    private static void UnhighlightGrid(DNAGrid grid)
    {
        if (grid == null) { return; }
        foreach (GridComponent gc in grid.Grid2D)
        {
            Highlight.UnhighlightGridCircle(gc);
            if (gc.Helix != null)
            {
                Highlight.UnhighlightHelix(gc.Helix);
            }
        }
    }*/


    /// <summary>
    /// Shows grid circles of DNAGrid
    /// </summary>
    /// <param name="gridId">id of DNAGrid</param>
    public static void ShowGridCircles(string gridId)
    {
        // First hide current selected grid's circles
        HideGridCircles(s_grid);

        s_gridDict.TryGetValue(gridId, out DNAGrid grid);
        s_grid = grid;
        grid.ToggleGridCircles(true);
    }

    /// <summary>
    /// Hides grid circles of DNAGrid
    /// </summary>
    public static void HideGridCircles(DNAGrid grid)
    {
        if (grid == null) { return; }
        grid.ToggleGridCircles(false);
    }
}
