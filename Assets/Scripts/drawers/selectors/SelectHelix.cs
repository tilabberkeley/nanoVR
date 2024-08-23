/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

public class SelectHelix : MonoBehaviour
{
    [SerializeField] private XRNode _leftXRNode;
    [SerializeField] private XRNode _rightXRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _leftDevice;
    private InputDevice _rightDevice;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool rightTriggerReleased = true;
    private bool leftTriggerReleased = true;
    private bool axisReleased = true;
    private bool selectMultiple = false;
    private static RaycastHit s_hit;
    private static List<GridComponent> selectedHelices = new List<GridComponent>();

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_leftXRNode, _devices);
        if (_devices.Count > 0)
        {
            _leftDevice = _devices[0];
        }

        InputDevices.GetDevicesAtXRNode(_rightXRNode, _devices);
        if (_devices.Count > 0)
        {
            _rightDevice = _devices[0];
        }
    }

    private void OnEnable()
    {
        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }
    }

    private void Update()
    {
        if (s_hideStencils)
        {
            return;
        }

        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }

        _leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool axisClick);

        if (leftTriggerValue)
        {
            leftTriggerReleased = false;
            selectMultiple = true;
        }

        if (rightTriggerValue && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            rightTriggerReleased = false;
            ResetHelices();
        }

        if (rightTriggerReleased && rightTriggerValue
           && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            rightTriggerReleased = false;
            GridComponent gc = s_hit.collider.gameObject.GetComponent<GridComponent>();
            Debug.Log("Right trigger clicked");
            if (gc != null && gc.Selected)
            {
                Debug.Log("helix hit");
                if (!selectMultiple)
                {
                    ResetHelices();
                    Debug.Log("Helices reset");
                }
                HighlightHelix(gc.gameObject);
                selectedHelices.Add(gc);
                Debug.Log("helix highlighted");
            }
        }

        if (axisClick && axisReleased)
        {
            axisReleased = false;
            if (selectedHelices.Count > 0)
            {
                // DELETE HELIX
                DoDeleteHelix(selectedHelices[0].Helix.Id); //TODO: HANDLE MULTIPLE HELIX DELETION WITH LIST - DY 8/18/24
            }
        }

        if (!axisClick)
        {
            axisReleased = true;
        }

        // Resets triggers do avoid multiple selections.                                              
        if (!rightTriggerValue)
        {
            rightTriggerReleased = true;
        }

        if (!leftTriggerValue)
        {
            leftTriggerReleased = true;
            selectMultiple = false;
        }

    }

    /// <summary>
    /// Resets the start and end nucleotides.
    /// </summary>
    public static void ResetHelices()
    {
        UnhighlightHelices();
        selectedHelices.Clear();
    }

    public static void HighlightHelix(GameObject go)
    {
        GridComponent gc = go.GetComponent<GridComponent>();
        Highlight.HighlightHelix(gc.Helix);
    }

    public static void UnhighlightHelices()
    {
        foreach (GridComponent gc in selectedHelices)
        {
            UnhighlightHelix(gc.gameObject);

        }
    }

    public static void UnhighlightHelix(GameObject go)
    {
        if (go == null) { return; }
        var gc = go.GetComponent<GridComponent>();
        if (gc.Helix != null)
        {
            Highlight.UnhighlightHelix(gc.Helix);
        }
    }

    public static void DoDeleteHelix(int id)
    {
        ICommand command = new DeleteHelixCommand(id);
        CommandManager.AddCommand(command);
    }

    public static void DeleteHelix(int id)
    {
        s_helixDict.TryGetValue(id, out Helix helix);
        if (!helix.IsEmpty())
        {
            Debug.Log("Helix not empty. Cannot delete");
            return;
        }
        helix.DeleteHelix();
    }

    /// <summary>
    /// Creates SubGrid object which contains a collection of Helices.
    /// Called by CreateSubGrid button in scene.
    /// </summary>
    public void CreateSubGrid()
    {
        SubGrid subGrid = new SubGrid(s_numSubGrids, selectedHelices);
        s_subGridDict.Add(s_numSubGrids, subGrid);
        ObjectListManager.CreateSubGridButton(s_numSubGrids);
        s_numSubGrids += 1;
    }
}
