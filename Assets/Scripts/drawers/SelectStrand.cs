/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Highlight;

public class SelectStrand : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool triggerReleased = true;
    private bool axisReleased = true;
    private const float DOUBLE_CLICK_TIME = 1f;
    private float lastClickTime;
    private static bool strandSelected = false;
    private static GameObject s_startGO = null;
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
        if (!s_gridTogOn)
        {
            return;
        }
        /*
        if (!s_drawTogOn && !s_eraseTogOn)
        {
            return;
        }*/

        if (!_device.isValid)
        {
            GetDevice();
        }

        // SELECT CROSSOVER NUCLEOTIDE
        bool triggerValue;
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue
            && triggerReleased
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            if (s_hit.collider.name.Contains("nucleotide"))
            {
                if (s_startGO == null)
                {
                    s_startGO = s_hit.collider.gameObject;
                }
                else if (s_hit.collider.gameObject == s_startGO)
                {
                    float timeSinceLastClick = Time.time - lastClickTime;
                    if (timeSinceLastClick <= DOUBLE_CLICK_TIME)
                    {
                        // DOUBLE CLICK!
                        strandSelected = true;
                        HighlightStrand(s_startGO);
                    }
                    else
                    {
                        UnhighlightStrand(s_startGO);
                        ResetNucleotides();
                    }
                }
                else
                {
                    UnhighlightStrand(s_startGO);
                    ResetNucleotides();
                }

                lastClickTime = Time.time;
            }
            else
            {
                UnhighlightStrand(s_startGO);
                ResetNucleotides();
            }
        }

        bool axisClick;
        if ((_device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out axisClick)
            && axisClick)
            && axisReleased)
        {
            axisReleased = false;
            if (strandSelected)
            {
                DoDeleteStrand(s_startGO);
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

        // Resets nucleotide.
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue
            && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            UnhighlightStrand(s_startGO);
            ResetNucleotides();
        }
    }

    /// <summary>
    /// Resets the start and end nucleotides.
    /// </summary>
    public void ResetNucleotides()
    {
        s_startGO = null;
        strandSelected = false;
    }

    public static void HighlightStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;
        if (strandId == -1) { return; }
        Strand strand = s_strandDict[strandId];
        Highlight.HighlightStrand(strand);
    }

    // TEST
    public static void HighlightStrand(int strandId)
    {
        strandSelected = true;
        s_strandDict.TryGetValue(strandId, out Strand strand);
        Highlight.HighlightStrand(strand);
        s_startGO = strand.GetHead();
    }

    public void UnhighlightStrand(GameObject go)
    {
        if (go == null) { return; }
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;

        if (strandId == -1) { return; }
        Strand strand = s_strandDict[strandId];
        Highlight.UnhighlightStrand(strand);
    }

    public static void DoDeleteStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);
        ICommand command = new DeleteCommand(strandId, strand.GetNucleotides(), strand.GetXovers(), strand.GetColor());
        CommandManager.AddCommand(command);
        command.Do();
    }

    public static void DeleteStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;
        //Debug.Log("Strand Id of deleted strand: " + strandId);
        //Debug.Log("Nucleotide head being deleted: " + go);
        Strand strand = s_strandDict[strandId];
        //DeleteStrandFromHelix(go);
        ObjectListManager.DeleteButton(strandId);
        strand.DeleteStrand();
    }

    public static void RemoveStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;
        Strand strand = s_strandDict[strandId];
        //DeleteStrandFromHelix(go);
        ObjectListManager.DeleteButton(strandId);
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
