/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

public class SelectGrid : MonoBehaviour
{
    private static List<DNAGrid> s_grids = new List<DNAGrid>();

    public static List<DNAGrid> Grids { get { return s_grids; } }

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
        }

        // Resets trigger.                                          
        if (!triggerValue)
        {
            triggerReleased = true;
        }
    }

    /// <summary>
    /// Shows grid circles of DNAGrid
    /// </summary>
    /// <param name="gridId">id of DNAGrid</param>
    public static void ToggleGridCircles(string gridId)
    {
        // First hide current selected grid's circles
        //HideGridCircles(s_grid);

        s_gridDict.TryGetValue(gridId, out DNAGrid grid);
        if (!s_grids.Contains(grid))
        {
            s_grids.Add(grid);
            grid.ToggleGridCircles(true);
        }
        else
        {
            s_grids.Remove(grid);
            grid.ToggleGridCircles(false);
        }
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
