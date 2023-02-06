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

      if (s_hit.collider.name.Contains("nucleotide"))
      {
        // NOT WORKING CHECK XR DIRECT INTERACTABLE
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject nt = s_hit.collider.gameObject;
        var ntc = nt.GetComponent<NucleotideComponent>();
        if (firstNucleotide == null)
        {
          firstNucleotide = nt;
        }
        else
        {
          secondNucleotide = nt;
        }
      }

      if (firstNucleotide != null) 
      {
        Vector3 direction = transform.rotation * Vector3.forward;
        Vector3 currPoint = transform.position + direction * 0.07f ;
      }

      if (secondNucleotide != null)
      {
        DrawRealCylinder.Clear();
        GameObject temp = DrawRealCylinder.DrawReal(firstNucleotide.transform.position, secondNucleotide.transform.position);
        var firstNtc = firstNucleotide.GetComponent<NucleotideComponent>();
        var secondNtc = secondNucleotide.GetComponent<NucleotideComponent>();
        firstNtc.SetCrossoverGO(secondNucleotide);
        firstNtc.SetCrossoverBB(temp);
        secondNtc.SetCrossoverGO(firstNucleotide);
        secondNtc.SetCrossoverBB(temp);
      } 

    }


    // Resets grips do avoid multiple selections.                                              
    if ((_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
            && !gripValue))
    {
      gripReleased = true;
    }

  }

}
