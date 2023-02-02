using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawNucleotide : MonoBehaviour
{
  [SerializeField] private XRNode _xrNode;
  private List<InputDevice> _devices = new List<InputDevice>();
  private InputDevice _device;
  [SerializeField] public XRRayInteractor rightRayInteractor;
  bool triggerReleased = true;
  GameObject highlightedGO = null;
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

      if (s_hit.collider.name.Contains("nucleotide"))
      {
        triggerReleased = false;
        GameObject nt = s_hit.collider.gameObject;
        var ntc = nt.GetComponent<NucleotideComponent>();
        ntc.FlipSelected();
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
        if (nt != highlightedGO)
        {
          highlightedGO.GetComponent<NucleotideComponent>().Unhighlight();
        }
        var ntc = nt.GetComponent<NucleotideComponent>();
        ntc.Highlight();
        highlightedGO = nt;
      }

    }

    // UNHIGHLIGHT
    if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && !triggerValue
            && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
    {
      if (highlightedGO != null)
      {
        highlightedGO.GetComponent<NucleotideComponent>().Unhighlight();
        highlightedGO = null;
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

  }

}
