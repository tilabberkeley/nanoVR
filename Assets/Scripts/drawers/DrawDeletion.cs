/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Highlight;

/// <summary>
/// Add deletion to selected nucleotide.
/// </summary>
public class DrawDeletion
{
    /*[SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool triggerReleased = true;
    private static GameObject s_GO = null;
    private static RaycastHit s_hit;*/

    /*void GetDevice()
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
    }

    void Update()
    {
        if (s_hideStencils || !s_delTogOn)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);
        if (triggerValue && triggerReleased
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            if (s_hit.collider.gameObject.GetComponent<NucleotideComponent>() != null)
            {
                s_GO = s_hit.collider.gameObject;
                DoDeletion(s_GO);
            }
        }

        // Resets triggers to avoid multiple selections.                                              
        if (!triggerValue)
        {
            triggerReleased = true;
        }
    }*/

    /// <summary>
    /// Command method to create deletion.
    /// </summary>
    /// <param name="go">Gameobject nucleotide of deletion.</param>
    public static void DoDeletion(GameObject go)
    {
        ICommand command = new DeletionCommand(go);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Actual method that creates deletion.
    /// </summary>
    /// <param name="go">Gameobject nucleotide of deletion.</param>
    public static void Deletion(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        if (ntc.IsInsertion)
        {
            Debug.Log("Cannot draw deletion over insertion.");
            return;
        }
        if (!ntc.Selected)
        {
            Debug.Log("Cannot draw insertion on unbound nucleotide.");
            return;
        }

        Strand strand = Utils.GetStrand(go);

        if (ntc.IsDeletion)
        {
            ntc.IsDeletion = false;
            UnhighlightDeletion(go);
        }
        else
        {
            ntc.IsDeletion = true;
            HighlightDeletion(go);
        }

        // Update strand DNA sequence
        if (strand != null)
        {
            string sequence = strand.Sequence;
            strand.Sequence = sequence;
            Utils.CheckMismatch(strand);
        }
    }
}
