/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

/// <summary>
/// Handles all the operations for creating and interacting with grid objects.
/// </summary>
public class DrawGrid : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    bool triggerReleased = true;
    private static RaycastHit s_hit;
    [SerializeField] private Dropdown directionDropdown;
    [SerializeField] private Dropdown gridTypeDropdown;
    private string plane;
    private const int LENGTH = 64;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
        if (_devices.Count > 0)
        {
            _device = _devices[0];
        }
    }

    void OnEnable()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }
        /*var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);

        if (leftHandDevices.Count == 1)
        {
            UnityEngine.XR.InputDevice device = leftHandDevices[0];
        }*/
    }

    void Update()
    {
        if (!s_gridTogOn || s_hideStencils)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool triggerValue;

        // Adds a new grid.
        /*if (triggerReleased
            && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue
            && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            plane = dropdown.options[dropdown.value].text;
            Vector3 direction = transform.rotation * Vector3.forward;
            Vector3 currPoint = transform.position + direction * 0.07f;
            CreateGrid(s_numGrids, plane, currPoint);
        }*/

        // Adds a helix to a grid.
        if (triggerReleased
            && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;

            // clicking grid sphere
            if (s_hit.collider.GetComponent<GridComponent>())
            {
                GridComponent gc = s_hit.collider.GetComponent<GridComponent>();
                Vector3 startPos = s_hit.collider.bounds.center;
                int id = s_numHelices;
                CreateHelix(id, startPos, LENGTH, plane, gc);
            }
        }
        
        if (!(_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue))
        {
            triggerReleased = true;
        }
    }

    // ADD A STATIC HELPER METHOD TO THIS

    /// <summary>
    /// Creates a grid when new grid button is clicked in game. 
    /// </summary>
    public void CreateGrid()
    {
        plane = directionDropdown.options[directionDropdown.value].text;
        Vector3 direction = transform.rotation * Vector3.forward;
        Vector3 currPoint = transform.position + direction * 0.2f;
        Grid grid;

        if (gridTypeDropdown.options[gridTypeDropdown.value].text.Equals("Square"))
        {
            grid = new SquareGrid(s_numGrids, plane, currPoint);
        }
        else if (gridTypeDropdown.options[gridTypeDropdown.value].text.Equals("Honeycomb"))
        {
            grid = new HoneycombGrid(s_numGrids, plane, currPoint);
        }
        else
        {
            grid = new HexGrid(s_numGrids, plane, currPoint);
        }
        s_gridDict.Add(s_numGrids, grid);
        s_numGrids += 1;
    }

    public static Grid CreateGrid(int gridId, string orientation, Vector3 position, string gridType)
    {
        return new SquareGrid(gridId, orientation, position);
    }


    // FIGURE OUT THIS WITH FileImport.ParseSC()
    /*public static Grid CreateGrid(int id, string orientation, Vector3 position)
    {
        Grid grid = new Grid(id, orientation, position);
        s_gridDict.Add(id, grid);
        Grid grid;

        if (gridTypeDropdown.options[gridTypeDropdown.value].text.Equals("Square"))
        {
            grid = new SquareGrid(s_numGrids, plane, currPoint);
        } 
        else if (gridTypeDropdown.options[gridTypeDropdown.value].text.Equals("Honeycomb"))
        {
            grid = new HoneycombGrid(s_numGrids, plane, currPoint);
        }
        else
        {
            grid = new HexGrid(s_numGrids, plane, currPoint);
        }
        s_gridDict.Add(s_numGrids, grid);
        s_numGrids += 1;
        return grid;
    }*/

    public void CreateHelix(int id, Vector3 startPos, int length, string orientation, GridComponent gc)
    {
        if (!gc.Selected)
        {
            gc.Grid.DoAddHelix(id, startPos, length, orientation, gc);
            gc.Grid.CheckExpansion(gc);
        }
    }
}
