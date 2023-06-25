/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

/// <summary>
/// Merges two strands into one strand.
/// </summary>
public class DrawMerge : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool triggerReleased = true;
    private static GameObject s_GO = null;
    private static RaycastHit s_hit;

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
    }

    void Update()
    {
        if (!s_gridTogOn)
        {
            return;
        }

        if (!s_mergeTogOn)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        // Handles start and end nucleotide selection.
        bool triggerValue;
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && triggerValue
                && triggerReleased
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            if (s_hit.collider.name.Contains("nucleotide"))
            {
                s_GO = s_hit.collider.gameObject;
                DoMergeStrand(s_GO);
                
            }
        }


        // Resets triggers to avoid multiple selections.                                              
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && !triggerValue)
        {
            triggerReleased = true;
        }
    }

    /// <summary>
    /// Splits a strand into two substrands at selected nucleotide.
    /// </summary>
    /// <returns>Returns split off strand.</returns>
    public static int IsValid(GameObject go)
    {
        // Checks GameObject is part of strand.
        var startNtc = go.GetComponent<NucleotideComponent>();
        if (!startNtc.Selected)
        {
            return -1;
        }

        // Check GameObject is a strand head or a strand tail.
        int strandId = startNtc.StrandId;
        Strand strand = s_strandDict[strandId];
        if (strand.GetHead() != go && strand.GetTail() != go)
        {
            return -1;
        }

        // Check GameObject's neighbor is part of another strand.
        int helixId = startNtc.HelixId;
        Helix helix = s_gridList[0].GetHelix(helixId);
        int direction = startNtc.Direction;
        GameObject headNeighbor = helix.GetHeadNeighbor(go, direction);
        GameObject tailNeighbor = helix.GetTailNeighbor(go, direction);
        if (strand.GetHead() == go && headNeighbor.GetComponent<NucleotideComponent>().Selected)
        {
            return 0;
        }
        
        if (strand.GetTail() == go && tailNeighbor.GetComponent<NucleotideComponent>().Selected)
        {
            return 1;
        }
        return -1;

    }

    public void DoMergeStrand(GameObject go)
    {
        int valid = IsValid(go);
        if (valid == -1)
        {
            return;
        }
        var ntc = go.GetComponent<NucleotideComponent>();
        int helixId = ntc.HelixId;
        Helix helix = s_gridList[0].GetHelix(helixId);
        int direction = ntc.Direction;

        if (valid == 0)
        {
            GameObject neighbor = helix.GetHeadNeighbor(go, direction);
            Color color = neighbor.GetComponent<NucleotideComponent>().GetColor();
            ICommand command = new MergeCommand(go, color, false);
            CommandManager.AddCommand(command);
            command.Do();
        }
        else if (valid == 1)
        {
            GameObject neighbor = helix.GetTailNeighbor(go, direction);
            Color color = neighbor.GetComponent<NucleotideComponent>().GetColor();
            ICommand command = new MergeCommand(go, color, true);
            CommandManager.AddCommand(command);
            command.Do();
        }
    }

    public static void MergeStrand(GameObject go)
    {
        int valid = IsValid(go);

        var ntc = go.GetComponent<NucleotideComponent>();
        int helixId = ntc.HelixId;
        Helix helix = s_gridList[0].GetHelix(helixId);
        int direction = ntc.Direction;

        if (valid == 0)
        {
            GameObject neighbor = helix.GetHeadNeighbor(go, direction);
            GameObject backbone = helix.GetHeadBackbone(go, direction);
            MergeStrand(go, neighbor, backbone, true);
        }
        else if (valid == 1)
        {
            GameObject neighbor = helix.GetTailNeighbor(go, direction);
            GameObject backbone = helix.GetTailBackbone(go, direction);
            MergeStrand(go, neighbor, backbone, false);
        }
    }

    public static void MergeStrand(GameObject firstGO, GameObject secondGO, GameObject backbone, bool isHead)
    {
        var firstNtc = firstGO.GetComponent<NucleotideComponent>();
        var secondNtc = secondGO.GetComponent<NucleotideComponent>();
        Strand firstStrand = s_strandDict[firstNtc.StrandId];
        Strand secondStrand = s_strandDict[secondNtc.StrandId];

        /*
        if (secondGO == null)
        {
            firstStrand.SetComponents();
            return;
        }
        */

        if (isHead)
        {
            firstStrand.AddToHead(backbone);
            firstStrand.AddToHead(secondStrand.GetNucleotides());
            // must add backbone between 2 strands
        }
        else
        {
            firstStrand.AddToTail(backbone);
            firstStrand.AddToTail(secondStrand.GetNucleotides());
            // must add backbone between 2 strands
        }
        SelectStrand.DeleteStrand(secondGO);
        firstStrand.SetComponents();
    }
}
