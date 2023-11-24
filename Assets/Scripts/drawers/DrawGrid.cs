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
    [SerializeField] private Dropdown dropdown;
    private string plane;

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
                if (!gc.Selected)
                {
                    Vector3 startPos = s_hit.collider.bounds.center;
                    Vector3 endPos = startPos - new Vector3(0, 0, 64 * 0.034f);
                    int id = s_numHelices;
                    gc.Grid.DoAddHelix(id, startPos, endPos, plane, gc);
                    gc.Grid.CheckExpansion(gc);
                }
                else
                {
                    SelectHelix.UnhighlightHelix();
                    SelectHelix.ResetNucleotides();
                    SelectHelix.HighlightHelix(s_hit.collider.gameObject);
                }
            }
        }
        
        if (!(_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue))
        {
            triggerReleased = true;
        }
    }

    public void CreateGrid()
    {
        plane = dropdown.options[dropdown.value].text;
        Vector3 direction = transform.rotation * Vector3.forward;
        Vector3 currPoint = transform.position + direction * 0.2f;
        Grid grid = new Grid(s_numGrids, plane, currPoint);
        s_gridDict.Add(s_numGrids, grid);
        s_numGrids += 1;
    }
}
