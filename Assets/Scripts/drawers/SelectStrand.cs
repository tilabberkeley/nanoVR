/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Highlight;

public class SelectStrand : MonoBehaviour
{
    [SerializeField] private XRNode _leftXRNode;
    [SerializeField] private XRNode _rightXRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _leftDevice;
    private InputDevice _rightDevice;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    private bool rightTriggerReleased = true;
    private bool leftTriggerReleased = true;
    private bool axisReleased = true;
    private const float DOUBLE_CLICK_TIME = 1f;
    private float lastClickTime;
    private static bool strandSelected = false;
    private static GameObject s_startGO = null;
    private static RaycastHit s_hit;
    private static List<Strand> s_highlightedStrands = new List<Strand>();

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_leftXRNode, _devices);
        if (_devices.Count > 0)
        {
            _leftDevice = _devices[0];
        }

        InputDevices.GetDevicesAtXRNode(_rightXRNode, _devices);
        if (_devices.Count > 0)
        {
            _rightDevice = _devices[0];
        }
    }

    private void OnEnable()
    {
        if (!_leftDevice.isValid || !_rightDevice.isValid)
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

        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }

        bool rightTriggerValue; 
        bool leftTriggerValue;

        if (_rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out rightTriggerValue)
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
        }

        bool axisClick;
        if ((_rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out axisClick)
            && axisClick)
            && axisReleased)
        {
            axisReleased = false;
            if (strandSelected)
            {
                UnhighlightStrand(s_startGO);
                DoDeleteStrand(s_startGO);
            }
        }


        if (!(_rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out axisClick)
            && axisClick))
        {
            axisReleased = true;
        }

        // Resets triggers do avoid multiple selections.                                              
        if (!(_rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out rightTriggerValue)
            && rightTriggerValue))
        {
            rightTriggerReleased = true;
        }

        if (!(_leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out leftTriggerValue)
            && leftTriggerValue))
        {
            leftTriggerReleased = true;
        }

        // Resets nucleotide.
        if (_rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out rightTriggerValue)
            && rightTriggerValue
            && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            rightTriggerReleased = false;
            UnhighlightStrand(s_startGO);
            ResetNucleotides();
        }

        if (_leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out leftTriggerValue)
            && leftTriggerValue
            && !leftRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            leftTriggerReleased = false;
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
        HighlightStrand(strandId);
    }

    // TEST
    public static void HighlightStrand(int strandId)
    {
        strandSelected = true;
        s_strandDict.TryGetValue(strandId, out Strand strand);
        s_startGO = strand.GetHead();
   
        Highlight.HighlightStrand(strand);
        
    }

    public static void UnhighlightStrand(GameObject go)
    {
       
        if (go == null) { return; }
        int strandId = go.GetComponent<NucleotideComponent>().StrandId;

        if (strandId == -1) { return; }
        s_strandDict.TryGetValue(strandId, out Strand strand);
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
