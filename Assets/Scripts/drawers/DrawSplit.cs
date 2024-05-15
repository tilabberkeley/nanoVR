/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Utils;

/// <summary>
/// Splits strand into two strands.
/// </summary>
public class DrawSplit
{
    /*[SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool triggerReleased = true;
    private static GameObject s_GO = null;
    private static RaycastHit s_hit;*/

/*    void GetDevice()
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
        if (!s_splitTogOn || s_hideStencils)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        // Handles start and end nucleotide selection.
        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);
        if (triggerValue && triggerReleased
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            if (s_hit.collider.GetComponent<NucleotideComponent>() != null)
            {
                s_GO = s_hit.collider.gameObject;
                DoSplitStrand(s_GO);
            }
        }

        // Resets triggers to avoid multiple selections.                                              
        if (!triggerValue)
        {
            triggerReleased = true;
        }
    }*/

    public static void DoSplitStrand(GameObject go)
    {
        if (!IsValid(go)) { return; }
        Color color = Colors[s_numStrands % Colors.Length];
        ICommand command = new SplitCommand(go, s_numStrands, color);
        CommandManager.AddCommand(command);
        //command.Do();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    /// <param name="id"></param>
    /// <param name="color"></param>
    /// <param name="splitAfter"></param>
    public static void SplitStrand(GameObject go, int id, Color color, bool splitAfter)
    {
        if (!IsValid(go)) { return; }
        var startNtc = go.GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);

        if (splitAfter)
        {
            if (strand.IsCircular)
            {
                strand.SplitCircularAfter(go);
            }
            else
            {
                CreateStrand(strand.SplitAfter(go), id, color);
            }
        }
        else
        {
            if (strand.IsCircular)
            {
                strand.SplitCircularBefore(go);
            }
            else
            {
                CreateStrand(strand.SplitBefore(go), id, color);
            }
        }
    }

    public static bool IsValid(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        if (!ntc.Selected)
        {
            return false;
        }
        int strandId = ntc.StrandId;
        Strand strand = s_strandDict[strandId];

        if (strand.Head == go || strand.Tail == go)
        {
            return false;
        }

        if (ntc.HasXover)
        {
            return false;
        }

       
        /*for (int i = 0; i < strand.GetXovers().Count; i++)
        {
            if (go == strand.GetXovers()[i].GetComponent<XoverComponent>().NextGO)
            {
                return false;
            }
        }*/

        return true;
    }
}
