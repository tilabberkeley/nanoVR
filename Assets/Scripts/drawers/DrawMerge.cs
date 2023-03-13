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
    [SerializeField] public XRRayInteractor rightRayInteractor;
    bool triggerReleased = true;
    static GameObject s_GO = null;
    public static RaycastHit s_hit;

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
                if (IsValid())
                {
                    MergeStrand();
                }
            }
        }


        // Resets triggers to avoid multiple selections.                                              
        if ((_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && !triggerValue))
        {
            triggerReleased = true;
        }
    }

    /// <summary>
    /// Splits a strand into two substrands at selected nucleotide.
    /// </summary>
    /// <returns>Returns split off strand.</returns>
    public bool IsValid()
    {
        // Checks GameObject is part of strand.
        var startNtc = s_GO.GetComponent<NucleotideComponent>();
        if (!startNtc.IsSelected())
        {
            return false;
        }

        // Check GameObject is a strand head or a strand tail.
        int strandId = startNtc.GetStrandId();
        Strand strand = s_strandDict[strandId];
        if (strand.GetHead() != s_GO && strand.GetTail() != s_GO)
        {
            return false;
        }

        // Check GameObject's neighbor is part of another strand.
        int helixId = startNtc.GetHelixId();
        Helix helix = s_gridList[0].GetHelix(helixId);
        int direction = startNtc.GetDirection();
        GameObject headNeighbor = helix.GetHeadNeighbor(s_GO, direction);
        GameObject tailNeighbor = helix.GetTailNeighbor(s_GO, direction);
        if (!headNeighbor.GetComponent<NucleotideComponent>().IsSelected()
            && tailNeighbor.GetComponent<NucleotideComponent>().IsSelected())
        {
            return false;
        }
        return true;

    }

    public void MergeStrand()
    {
        var ntc = s_GO.GetComponent<NucleotideComponent>();
        int helixId = ntc.GetHelixId();
        int strandId = ntc.GetStrandId();
        Strand strand = s_strandDict[strandId];
        Helix helix = s_gridList[0].GetHelix(helixId);
        int direction = ntc.GetDirection();

        GameObject neighbor = null;
        if (strand.GetHead() == s_GO)
        {
            neighbor = helix.GetHeadNeighbor(s_GO, direction);
        }
        if (strand.GetTail() == s_GO)
        {
            neighbor = helix.GetTailNeighbor(s_GO, direction);
        }
        MergeStrand(s_GO, neighbor);
    }

    public void MergeStrand(GameObject firstGO, GameObject secondGO)
    {
        if (secondGO == null)
        {
            return;
        }

    }


    /// <summary>
    /// Creates a new strand with it's own id, color, and list of nucleotides.
    /// Adds new strand to the global strand dictionary.
    /// </summary>
    /// <param name="nucleotides">List of nucleotides to use in new strand.</param>
    public void CreateStrand(List<GameObject> nucleotides)
    {
        var startNtc = s_GO.GetComponent<NucleotideComponent>();
        int direction = startNtc.GetDirection();

        Strand strand;
        if (direction == 0)
        {
            strand = new Strand(nucleotides, s_numStrands, direction);
        }
        else
        {
            strand = new Strand(nucleotides, s_numStrands, direction);
        }
        strand.SetComponents();
        s_strandDict.Add(s_numStrands, strand);
        s_numStrands++;
    }
}
