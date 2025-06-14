/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

public class SelectStrand : MonoBehaviour
{
    [SerializeField] private XRNode _XRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    private bool triggerReleased = true;
    private bool axisReleased = true;
    private static RaycastHit s_hit;
    private static List<Strand> s_strands = new List<Strand>();
    public static List<Strand> Strands { get { return s_strands; } }

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
        if (s_selectTogOn && triggerValue && triggerReleased && rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;

            // Check that hit Gameobject is part of strand
            NucleotideColliderComponent comp = s_hit.collider.GetComponent<NucleotideColliderComponent>();

            if (comp != null && comp.Data.IsSelected())
            {
                /*if (s_strand != null)
                {
                    UnhighlightStrand(s_strand, false);
                }*/ // Note: Chagned this DY 9/11
                AddStrand(comp.Data.StrandId);
                HighlightStrand(comp.Data.StrandId);
            }
        }

        _device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool axisClick);
        if (axisClick && axisReleased)
        {
            axisReleased = false;

            foreach (Strand strand in s_strands)
            {
                UnhighlightStrand(strand, true);
            }
            DoDeleteStrands(s_strands);
            s_strands.Clear();
        }

        // Resets selected strand.
        if (triggerValue && !rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            Reset();
        }

        // Resets axis click.
        if (!axisClick)
        {
            axisReleased = true;
        }

        // Resets trigger.                                          
        if (!triggerValue)
        {
            triggerReleased = true;
        }
    }

    /// <summary>
    /// Resets the strand and strandSelected values.
    /// </summary>
    public static void Reset()
    {
        foreach (Strand strand in s_strands)
        {
            UnhighlightStrand(strand, false);
        }
        s_strands.Clear();
    }

    public static void AddStrand(int strandId)
    {
        s_strandDict.TryGetValue(strandId, out Strand strand);
        s_strands.Add(strand);
    }

    public static void HighlightStrand(int strandId)
    {
        s_strandDict.TryGetValue(strandId, out Strand strand);
        Highlight.HighlightStrand(strand);
    }

    public static void UnhighlightStrand(Strand strand, bool isDelete)
    {
        if (strand == null) { return; }
        Highlight.UnhighlightStrand(strand, isDelete);
    }

    public static void DoDeleteStrands(List<Strand> strands)
    {
        ICommand command = new DeleteCommand(strands);
        CommandManager.AddCommand(command);
    }

    public static void DeleteStrand(Strand strand)
    {
        RemoveStrand(strand.Id);
        strand.DeleteStrand();
    }

    public static void RemoveStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);
        //DeleteStrandFromHelix(go);
        ObjectListManager.DeleteStrandButton(strandId);
        strand.RemoveStrand();
    }

    public static void RemoveStrand(int strandId)
    {
        ObjectListManager.DeleteStrandButton(strandId);
        s_strandDict.Remove(strandId);
    }
}
