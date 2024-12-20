/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
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
    //private static List<Strand> s_highlightedStrands = new List<Strand>();

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
       /* if (!s_selectTogOn)
        {
            return;
        }*/

        if (!_device.isValid)
        {
            GetDevice();
        }

        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);
        if (s_selectTogOn && triggerValue && triggerReleased && rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;

            // Check that hit Gameobject is part of strand
            DNAComponent dnaComp = s_hit.collider.GetComponent<DNAComponent>();

            if (dnaComp != null && dnaComp.Selected)
            {
                /*if (s_strand != null)
                {
                    UnhighlightStrand(s_strand, false);
                }*/ // Note: Chagned this DY 9/11
                AddStrand(dnaComp.StrandId);
                HighlightStrand(dnaComp.StrandId);
            }
        }

        _device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool axisClick);
        if (axisClick && axisReleased)
        {
            axisReleased = false;
            //UnhighlightStrand(s_strand, true);
            //DoDeleteStrand(s_strand);

            foreach (Strand strand in s_strands)
            {
                UnhighlightStrand(strand, true);
                DeleteStrand(strand.Head);
            }
            
        }

        // Resets selected strand.
        if (triggerValue && !rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            //UnhighlightStrand(s_strand, false);
            
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

    /*public static void HighlightStrand(GameObject go)
    {
        int strandId = -1;
        if (go.GetComponent<DNAComponent>())
        {
            strandId = go.GetComponent<DNAComponent>().StrandId;
        }
        if (go.GetComponent<XoverComponent>())
        {
            strandId = go.GetComponent<XoverComponent>().PrevGO.GetComponent<NucleotideComponent>().StrandId;
        }
        
        if (strandId == -1) { return; }
        HighlightStrand(strandId);
    }*/

    public static void AddStrand(int strandId)
    {
        s_strandDict.TryGetValue(strandId, out Strand strand);
        s_strands.Add(strand);
    }

    // TEST
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

    public static void DoDeleteStrand(Strand strand)
    {
        ICommand command = new DeleteCommand(strand.Id, strand.Nucleotides, strand.Color);
        CommandManager.AddCommand(command);
    }

    public static void DeleteStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;
        //Debug.Log("Strand Id of deleted strand: " + strandId);
        //Debug.Log("Nucleotide head being deleted: " + go);
        s_strandDict.TryGetValue(strandId, out Strand strand);
        //DeleteStrandFromHelix(go);
        ObjectListManager.DeleteStrandButton(strandId);
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

    /*
    public static void DeleteStrandFromHelix(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);
        List<int> helixIds = strand.GetHelixIds();
        foreach (int id in helixIds)
        {
            Debug.Log("Helix strand belongs to: " + id);
            s_helixDict.TryGetValue(id, out Helix helix);
            helix.DeleteStrandId(strandId);
        }
    }*/
}
