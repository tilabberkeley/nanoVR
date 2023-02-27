using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawCrossover : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] public XRRayInteractor rightRayInteractor;
    bool gripReleased = true;
  
    GameObject firstNucleotide = null;

    GameObject secondNucleotide = null;
    public static RaycastHit s_hit;
    // public static GameObject s_crossover;



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

        // SELECT CROSSOVER NUCLEOTIDE
        bool gripValue;
        if (_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
            && gripValue
            && gripReleased
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            gripReleased = false;
            if (s_hit.collider.name.Contains("nucleotide")) {
                GameObject nt = s_hit.collider.gameObject;
                var ntc = nt.GetComponent<NucleotideComponent>();
                if (firstNucleotide == null && ntc.IsSelected())
                {
                    firstNucleotide = nt;
                }
                else if (firstNucleotide != null 
                    && secondNucleotide == null 
                    && ntc.IsSelected())
                {
                    if (ntc.GetHelixId() != firstNucleotide.GetComponent<NucleotideComponent>().GetHelixId())
                    {
                        secondNucleotide = nt;
                    }
                }
            }
            else
            {
                firstNucleotide = null;
            }
    
        }

        if (firstNucleotide != null) 
        {
            Vector3 direction = transform.rotation * Vector3.forward;
            Vector3 currPoint = transform.position + direction * 0.07f;
            DrawRealCylinder.Draw(firstNucleotide.transform.position, currPoint);
        }

        if (secondNucleotide != null)
        {
            
        }

        // Resets grips do avoid multiple selections.                                              
        if ((_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
                && !gripValue))
        {
            gripReleased = true;
        }

    }

}
