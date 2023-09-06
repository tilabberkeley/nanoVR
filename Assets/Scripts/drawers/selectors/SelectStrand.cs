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
    private const float DOUBLE_CLICK_TIME = 1f;
    private float lastClickTime;
    private static bool strandSelected = false;
    public static Strand s_strand = null;
    private static RaycastHit s_hit;
    private static List<Strand> s_highlightedStrands = new List<Strand>();

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

        bool triggerValue; 
        /*if (_rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out rightTriggerValue)
            && rightTriggerValue
            && rightTriggerReleased
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            rightTriggerReleased = false;
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
        }*/
        bool axisClick;
        if ((_device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out axisClick)
            && axisClick)
            && axisReleased)
        {
            axisReleased = false;
            if (strandSelected)
            {
                UnhighlightStrand(s_strand);
                DoDeleteStrand(s_strand);
            }
        }

        // Resets nucleotide.
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue)
        {
            triggerReleased = false;
            UnhighlightStrand(s_strand);
            Reset();
        }

        // Resets axis click.
        if (!(_device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out axisClick)
            && axisClick))
        {
            axisReleased = true;
        }

        // Resets trigger.                                          
        if (!(_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue))
        {
            triggerReleased = true;
        }
    }

    /// <summary>
    /// Resets the start and end nucleotides.
    /// </summary>
    public void Reset()
    {
        s_strand = null;
        strandSelected = false;
    }

    public static void HighlightStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;
        if (strandId == -1) { return; }
        HighlightStrand(strandId);
    }

    // TEST
    public static void HighlightStrand(int strandId)
    {
        strandSelected = true;
        s_strandDict.TryGetValue(strandId, out Strand strand);
        s_strand = strand;
   
        Highlight.HighlightStrand(strand);
        
    }

    public static void UnhighlightStrand(Strand strand)
    {
        if (strand == null) { return; }
        Highlight.UnhighlightStrand(strand);
    }

    public static void DoDeleteStrand(Strand strand)
    {
        ICommand command = new DeleteCommand(strand.GetStrandId(), strand.GetNucleotides(), strand.GetXovers(), strand.GetColor());
        CommandManager.AddCommand(command);
        command.Do();
    }

    public static void DeleteStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;
        //Debug.Log("Strand Id of deleted strand: " + strandId);
        //Debug.Log("Nucleotide head being deleted: " + go);
        s_strandDict.TryGetValue(strandId, out Strand strand);
        //DeleteStrandFromHelix(go);
        ObjectListManager.DeleteButton(strandId);
        strand.DeleteStrand();
    }

    public static void RemoveStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);
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
