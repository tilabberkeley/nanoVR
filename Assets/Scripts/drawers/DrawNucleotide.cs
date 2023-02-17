using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

public class DrawNucleotide : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] public XRRayInteractor rightRayInteractor;
    bool triggerReleased = true;
    GameObject startGO = null;
    GameObject endGO = null;
    public static RaycastHit s_hit;



    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
        _device = _devices[0];
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
        if (!GlobalVariables.s_gridTogOn)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        // SELECT OR DESELECT NUCLEOTIDE
        bool triggerValue;
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && triggerValue
                && triggerReleased
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            if (s_hit.collider.name.Contains("nucleotide"))
            {
                if (GlobalVariables.s_drawTogOn)
                    BuildStrand(s_hit.collider.gameObject);
                else if (GlobalVariables.s_eraseTogOn)
                    EraseStrand(s_hit.collider.gameObject);
            }
        }

        // HIGHLIGHT
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && !triggerValue
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            if (s_hit.collider.name.Contains("nucleotide"))
            {
                GameObject nt = s_hit.collider.gameObject;
                var ntc = nt.GetComponent<NucleotideComponent>();
                ntc.Highlight();
                startGO = nt;
            }
        }

        // UNHIGHLIGHT
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && !triggerValue
                && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            if (startGO != null)
            {
                startGO.GetComponent<NucleotideComponent>().Unhighlight();
                startGO = null;
            }
        }

        // Resets triggers do avoid multiple selections.                                              
        if ((_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && triggerValue
                && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
                || (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && !triggerValue))
        {
            triggerReleased = true;
           
        }

        if ((_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && !triggerValue))
        {
            
        }


    }

    public void BuildStrand(GameObject go)
    {
        
        var ntc = go.GetComponent<NucleotideComponent>();
        var helixC = go.GetComponent<HelixComponent>();
        var strandComp = go.GetComponent<StrandComponent>();
        if (startGO == null)
        {
            startGO = go;
        }
        if (!ntc.IsSelected())
        {

        }
    }

    // Erases nucleotide and resets variables
    public void EraseStrand(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        var helixC = go.GetComponent<HelixComponent>();
        var strandComp = go.GetComponent<StrandComponent>();

        if (ntc.IsSelected())
        {
            ntc.SetSelected(false);
            strandComp.SetNextBB(null);
            strandComp.SetNextGO(null);
            strandComp.SetPrevBB(null);
            strandComp.SetPrevGO(null);
            strandComp.SetCrossoverBB(null);
            strandComp.SetCrossoverGO(null);
            strandComp.SetStrandColor(new Color(0.5f, 0.5f, 0.5f, 0.5f));
            strandComp.SetStrandId(-1);
        }
    }

}
