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
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool triggerReleased = true;
    private bool axisReleased = true;
    private static bool helixSelected = false;
    private static GameObject s_gridGO = null;
    private static RaycastHit s_hit;

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
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
        if (s_hideStencils)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool triggerValue;
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
           && triggerValue
           && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            UnhighlightHelix(s_gridGO);
            ResetNucleotides();
        }

        if (triggerReleased
           && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
           && triggerValue
           && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            if (s_hit.collider.gameObject.GetComponent<GridComponent>() && s_hit.collider.gameObject.GetComponent<GridComponent>().Selected)
            {
                UnhighlightHelix();
                ResetNucleotides();
                HighlightHelix(s_hit.collider.gameObject);
            }
        }

        bool axisClick;
        if ((_device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out axisClick)
            && axisClick)
            && axisReleased)
        {
            axisReleased = false;
            if (helixSelected)
            {
                // DELETE HELIX
                DoDeleteHelix(s_gridGO.GetComponent<GridComponent>().Helix.Id);
            }
        }

        if (!(_device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out axisClick)
            && axisClick))
        {
            axisReleased = true;
        }

        // Resets triggers do avoid multiple selections.                                              
        if (!(_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue))
        {
            triggerReleased = true;
        }
    }

    /// <summary>
    /// Resets the start and end nucleotides.
    /// </summary>
    public static void ResetNucleotides()
    {
        s_gridGO = null;
        helixSelected = false;
    }

    public static void HighlightHelix(GameObject go)
    {
        helixSelected = true;
        var gc = go.GetComponent<GridComponent>();
        s_gridGO = go;
        Highlight.HighlightHelix(gc.Helix);
    }

    public static void UnhighlightHelix()
    {
        UnhighlightHelix(s_gridGO);
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
}
