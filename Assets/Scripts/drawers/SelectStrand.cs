/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

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
    private bool strandSelected = false;
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

        if (!s_drawTogOn && !s_eraseTogOn)
        {
            return;
        }

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

        // Resets start and end nucleotide.
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

    public void HighlightStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().GetStrandId();
        Strand strand = s_strandDict[strandId];
        strand.Highlight(Color.red);
    }

    public void UnhighlightStrand(GameObject go)
    {
        if (go == null) { return; }
        int strandId = go.GetComponent<NucleotideComponent>().GetStrandId();
        Strand strand = s_strandDict[strandId];
        strand.Highlight(Color.black);
    }

    public static void DoDeleteStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().GetStrandId();
        Strand strand = s_strandDict[strandId];
        ICommand command = new DeleteCommand(strand.GetStrandId(), strand.GetNucleotides(), strand.GetColor());
        CommandManager.AddCommand(command);
        command.Do();
    }

    public static void DeleteStrand(GameObject go)
    {
        int strandId = go.GetComponent<NucleotideComponent>().GetStrandId();
        Strand strand = s_strandDict[strandId];
        strand.RemoveStrand();
    }
}
