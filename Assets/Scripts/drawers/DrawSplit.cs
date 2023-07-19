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
/// Splits strand into two strands.
/// </summary>
public class DrawSplit : MonoBehaviour
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
        if (!s_gridTogOn || !s_splitTogOn || s_hideStencils)
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
                DoSplitStrand(s_GO);
                
            }
        }


        // Resets triggers to avoid multiple selections.                                              
        if ((_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && !triggerValue))
        {
            triggerReleased = true;
        }
    }

    public void DoSplitStrand(GameObject go)
    {
        if (!IsValid(go)) { return; }
        Color color = s_colors[s_numStrands % 6];
        ICommand command = new SplitCommand(go, s_numStrands, color);
        CommandManager.AddCommand(command);
        command.Do();
    }

    /// <summary>
    /// Splits a strand into two substrands at selected nucleotide.
    /// </summary>
    /// <returns>Returns split off strand.</returns>
    public static void SplitStrand(GameObject go, int id, Color color, bool splitAfter)
    {
        var startNtc = go.GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);


        int goIndex = strand.GetIndex(go);
        if (splitAfter)
        {
            List<GameObject> xovers = strand.GetXoversAfterIndex(goIndex);
            strand.RemoveXovers(xovers);
            CreateStrand(strand.SplitAfter(go), id, xovers, color);
        }
        else
        {
            List<GameObject> xovers = strand.GetXoversBeforeIndex(goIndex);
            strand.RemoveXovers(xovers);
            CreateStrand(strand.SplitBefore(go), id, xovers, color);
        }
    }

    public static bool IsValid(GameObject go)
    {
        var startNtc = go.GetComponent<NucleotideComponent>();
        if (!startNtc.Selected)
        {
            return false;
        }
        int strandId = startNtc.StrandId;
        Strand strand = s_strandDict[strandId];

        if (strand.GetHead() == go)
        {
            return false;
        }
       
        for (int i = 0; i < strand.GetXovers().Count; i++)
        {
            if (go == strand.GetXovers()[i].GetComponent<XoverComponent>().GetNextGO())
            {
                return false;
            }
        }

        return true;
    }


    /// <summary>
    /// Creates a new strand with it's own id, color, and list of nucleotides.
    /// Adds new strand to the global strand dictionary.
    /// </summary>
    /// <param name="nucleotides">List of nucleotides to use in new strand.</param>
    public static void CreateStrand(List<GameObject> nucleotides, int strandId, List<GameObject> xovers, Color color)
    {
        Strand strand = new Strand(nucleotides, xovers, strandId, color);
        strand.SetComponents();
        s_strandDict.Add(strandId, strand);

        DrawNucleotideDynamic.CreateButton(strandId);
        //DrawNucleotideDynamic.AddStrandToHelix(nucleotides[0]);

        s_numStrands += 1;
    }
}
