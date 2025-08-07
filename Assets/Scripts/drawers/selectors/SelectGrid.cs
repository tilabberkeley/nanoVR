/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    //private bool triggerReleased = true;
    private bool axisReleased = true;
    private static string BLUE = "#00F4FF";

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

        _device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool axisValue);

        // Resets selected grid.
        if (axisValue && axisReleased)
        {
            axisReleased = false;
            DeleteGrids();
        }

        // Resets axis click.                                          
        if (!axisValue)
        {
            axisReleased = true;
        }
    }

    private void DeleteGrids()
    {
        foreach (DNAGrid grid in s_grids)
        {
            grid.DeleteGrid();
        }
    }

    /// <summary>
    /// Shows grid circles of DNAGrid
    /// </summary>
    /// <param name="gridId">id of DNAGrid</param>
    public static void ToggleGridSelection(string gridId, Button button)
    {
        // First hide current selected grid's circles
        //HideGridCircles(s_grid);
        ColorUtility.TryParseHtmlString(BLUE, out Color blue);
        s_gridDict.TryGetValue(gridId, out DNAGrid grid);
        if (!s_grids.Contains(grid))
        {
            s_grids.Add(grid);
            grid.ToggleGridCircles(true);
            SetButtonColor(button, blue);
        }
        else
        {
            s_grids.Remove(grid);
            grid.ToggleGridCircles(false);
            SetButtonColor(button, Color.gray);
        }
    }

    private static void SetButtonColor(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color;
        colors.selectedColor = color;
        button.colors = colors;
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
