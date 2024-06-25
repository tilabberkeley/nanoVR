/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using static GlobalVariables;
using UnityEngine.EventSystems;

public class DCInteractionManager : MonoBehaviour
{
    // Input controller variables
    [SerializeField] private XRNode _leftXRNode;
    [SerializeField] private XRNode _rightXRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _leftDevice;
    private InputDevice _rightDevice;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;


    // Helper variables
    private static RaycastHit s_hit;
    private bool leftReleased = true;
    private bool rightReleased = true;
    private List<DomainComponent> domains = new List<DomainComponent>();

    void GetDevice()
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

    void OnEnable()
    {
        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }

        if (!s_strandView || s_visualMode)
        {
            return;
        }

        _leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue);


        if (leftTriggerValue && leftReleased && leftRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            leftReleased = false;
            DomainComponent domainComponent = s_hit.collider.GetComponent<DomainComponent>();
            if (domainComponent != null)
            {
                domains.Add(domainComponent);
                domainComponent.ShowNucleotides();
            }
        }

        if (rightTriggerValue && rightReleased && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            rightReleased = false;
            DomainComponent domainComponent = s_hit.collider.GetComponent<DomainComponent>();
            if (domainComponent != null)
            {
                domains.Add(domainComponent);
                domainComponent.ShowNucleotides();
            }
        }

        // If both triggers pressed, reset all nucleotides back to Strand View
        if (leftTriggerValue && rightTriggerValue
                && leftReleased && rightReleased)
        {
            leftReleased = false;
            rightReleased = false;
            foreach (DomainComponent domain in domains)
            {
                domain.HideNucleotides();
            }
            domains.Clear();
        }

        // Resets triggers to avoid multiple selections.                                              
        if (!leftTriggerValue)
        {
            leftReleased = true;
        }

        if (!rightTriggerValue)
        {
            rightReleased = true;
        }
    }
}
