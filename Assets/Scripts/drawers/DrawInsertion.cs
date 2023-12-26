/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using static GlobalVariables;

/// <summary>
/// Add insertion to selected nucleotide.
/// </summary>
public class DrawInsertion : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    [SerializeField] private Canvas _editPanel;
    [SerializeField] private TMP_InputField _inputField;
    private bool triggerReleased = true;
    private bool gripReleased = true;
    private static GameObject s_GO = null;
    private static RaycastHit s_hit;
    private const int DEFAULT_LENGTH = 1;

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

        _editPanel.enabled = false;
    }

    void Update()
    {
        if (!s_gridTogOn || s_hideStencils || !s_insTogOn)
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
                && triggerReleased
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            if (s_hit.collider.gameObject.GetComponent<NucleotideComponent>() != null)
            {
                s_GO = s_hit.collider.gameObject;
                DoInsertion(s_GO, DEFAULT_LENGTH);
            }
        }

        // Checks grab button to show edit insertion length panel.
        bool gripValue;
        if (_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
                && gripValue
                && gripReleased
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            gripReleased = false;
            if (s_hit.collider.gameObject.GetComponent<NucleotideComponent>() != null 
                && s_hit.collider.gameObject.GetComponent<NucleotideComponent>().IsInsertion)
            {
                s_GO = s_hit.collider.gameObject;
                ShowEditPanel();
            }
        }

        // Resets triggers to avoid multiple selections.                                              
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && !triggerValue)
        {
            triggerReleased = true;
        }

        // Resets grips to avoid multiple selections.                                              
        if (_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
                && !gripValue)
        {
            gripReleased = true;
        }
    }

    /// <summary>
    /// Command method to create insertion.
    /// </summary>
    /// <param name="go">Gameobject nucleotide of insertion.</param>
    /// <param name="length">Length of insertion.</param>
    public void DoInsertion(GameObject go, int length)
    {
        ICommand command = new InsertionCommand(go, length);
        CommandManager.AddCommand(command);
        command.Do();
    }

    /// <summary>
    /// Actual method that creates insertion.
    /// </summary>
    /// <param name="go">Gameobject nucleotide of insertion.</param>
    /// <param name="length">Length of insertion.</param>
    public static void Insertion(GameObject go, int length)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        if (ntc.IsDeletion)
        {
            Debug.Log("Cannot draw insertion over deletion.");
            return;
        }

        if (ntc.IsInsertion)
        {
            ntc.Insertion = 0;
            return;
        }

        ntc.Insertion = length;
    }

    /// <summary>
    /// Edits insertion length to something other than default of 1.
    /// </summary>
    public void Edit()
    {
        var ntc = s_GO.GetComponent<NucleotideComponent>();
        if (ntc.IsInsertion)
        {
            ntc.Insertion = Int32.Parse(_inputField.text);
        }
        HideEditPanel();
    }

    public void ShowEditPanel()
    {
        _editPanel.enabled = true;
    }

    public void HideEditPanel()
    {
        _editPanel.enabled = false;
    }
}
