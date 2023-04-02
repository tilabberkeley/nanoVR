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
    private static Color[] s_colors = { Color.blue, Color.magenta, Color.green, Color.red, Color.cyan, Color.yellow };

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

        if (!s_splitTogOn)
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
        Color color = s_colors[s_numStrands % 6];
        ICommand command = new SplitCommand(go, color);
        CommandManager.AddCommand(command);
        command.Do();
    }

    /// <summary>
    /// Splits a strand into two substrands at selected nucleotide.
    /// </summary>
    /// <returns>Returns split off strand.</returns>
    public static void SplitStrand(GameObject go, Color color, bool splitAfter)
    {
        var startNtc = go.GetComponent<NucleotideComponent>();
        int strandId = startNtc.GetStrandId();
        Strand strand = s_strandDict[strandId];
        if (IsValid(go))
        {
            if (splitAfter)
            {
                CreateStrand(strand.SplitAfter(go), color);
            }
            else
            {
                CreateStrand(strand.SplitBefore(go), color);
            }
        }
    }

    public static bool IsValid(GameObject go)
    {
        var startNtc = go.GetComponent<NucleotideComponent>();
        if (!startNtc.IsSelected())
        {
            return false;
        }
        int strandId = startNtc.GetStrandId();
        Strand strand = s_strandDict[strandId];

        if (strand.GetHead() == go || strand.GetTail() == go)
        {
            return false;
        }
        if (strand.GetNextGO(go).GetComponent<XoverComponent>() != null)
        {
            return false;
        }

        return true;
    }


    /// <summary>
    /// Creates a new strand with it's own id, color, and list of nucleotides.
    /// Adds new strand to the global strand dictionary.
    /// </summary>
    /// <param name="nucleotides">List of nucleotides to use in new strand.</param>
    public static void CreateStrand(List<GameObject> nucleotides, Color color)
    {
        Strand strand = new Strand(nucleotides, s_numStrands, color);
        strand.SetComponents();
        s_strandDict.Add(s_numStrands, strand);
        s_numStrands++;
    }
}
