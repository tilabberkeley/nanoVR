/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using static GlobalVariables;

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
    private bool _leftTriggerReleased = true;
    private bool _leftGripReleased = true;
    private bool _rightTriggerReleased = true;
    private bool _rightGripReleased = true;
    // private List<DomainComponent> _domains = new List<DomainComponent>();
    private DomainComponent _savedDomainComponent = null;

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
        _leftDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool leftGripValue);

        _rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool rightGripValue);

        HandleLeftHandInteraction(leftTriggerValue, leftGripValue, rightGripValue);
        HandleRightHandInteraction(rightTriggerValue, rightGripValue, leftGripValue);

        if (!leftGripValue && !rightGripValue)
        {
            // Reactivate saved domain if grip released.
            ReactivateSavedDomain();
        }
    }

    private void HandleLeftHandInteraction(bool leftTriggerValue, bool leftGripValue, bool rightGripValue)
    {
        bool leftRayInteractorHit = leftRayInteractor.TryGetCurrent3DRaycastHit(out s_hit);

        // Check for hiding a domain if grip is pressed
        if (leftGripValue && _leftGripReleased && leftRayInteractorHit)
        {
            _leftGripReleased = false;
            DomainComponent domainComponent = s_hit.collider.GetComponent<DomainComponent>();
            if (domainComponent != null)
            {
                _savedDomainComponent = domainComponent;
                domainComponent.gameObject.SetActive(false);
            }
        }
        // Handle holding down grip when not looking at a domain
        else if (leftGripValue && _leftGripReleased)
        {
            _leftGripReleased = false;
        }

        // Handle interacting with nucleotides when holding down grip
        if (leftGripValue && leftTriggerValue && _leftTriggerReleased && leftRayInteractorHit)
        {
            _leftTriggerReleased = false;
            NucleotideComponent nucleotideComponent = s_hit.collider.GetComponent<NucleotideComponent>();
            // Nucleotide complement must be apart of the domain to turn it 
            if (nucleotideComponent != null)
            {
                nucleotideComponent.Domain.StrandViewWithoutComplement();
            }
        }
        // Handle showing nucleotides of domain on click - shouldn't be able to show nucleotides when holding grip.
        else if (leftTriggerValue && _leftTriggerReleased && leftRayInteractorHit)
        {
            _leftTriggerReleased = false;
            DomainComponent domainComponent = s_hit.collider.GetComponent<DomainComponent>();
            if (domainComponent != null)
            {
                domainComponent.NucleotideView();
            }
        }

        // Resets triggers and grips to avoid multiple selections.                                              
        if (!leftTriggerValue)
        {
            _leftTriggerReleased = true;
        }

        if (!leftGripValue)
        {
            _leftGripReleased = true;
        }
    }

    private void HandleRightHandInteraction(bool rightTriggerValue, bool rightGripValue, bool leftGripValue)
    {
        bool rightRayInteractorHit = rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit);

        // Check for hiding a domain if grip is pressed
        if (rightGripValue && _rightGripReleased && rightRayInteractorHit)
        {
            _rightGripReleased = false;
            DomainComponent domainComponent = s_hit.collider.GetComponent<DomainComponent>();
            if (domainComponent != null)
            {
                _savedDomainComponent = domainComponent;
                domainComponent.gameObject.SetActive(false);
            }
        }
        // Handle holding down grip when not looking at a domain
        else if (rightGripValue && _rightGripReleased)
        {
            _rightGripReleased = false;
        }

        // Handle interacting with nucleotides when holding down grip
        if (rightGripValue && rightTriggerValue && _rightTriggerReleased && rightRayInteractorHit)
        {
            _rightTriggerReleased = false;
            NucleotideComponent nucleotideComponent = s_hit.collider.GetComponent<NucleotideComponent>();
            // Nucleotide complement must be apart of the domain to turn it 
            if (nucleotideComponent != null)
            {
                nucleotideComponent.Domain.StrandViewWithoutComplement();
            }
        }
        // Handle showing nucleotides of domain on click - shouldn't be able to show nucleotides when holding grip.
        else if (rightTriggerValue && _rightTriggerReleased && rightRayInteractorHit)
        {
            Debug.Log("Show nucls of dc in dc manager");
            _rightTriggerReleased = false;
            DomainComponent domainComponent = s_hit.collider.GetComponent<DomainComponent>();
            if (domainComponent != null)
            {
                Debug.Log("Showing nucls now");
                domainComponent.NucleotideView();
            }
        }

        // Resets triggers and grips to avoid multiple selections.
        if (!rightTriggerValue)
        {
            _rightTriggerReleased = true;
        }

        if (!rightGripValue)
        {
            _rightGripReleased = true;
            // Reactivate saved domain if grip released.
            ReactivateSavedDomain();
        }
    }

    private void ReactivateSavedDomain()
    {
        if (_savedDomainComponent != null)
        {
            _savedDomainComponent.gameObject.SetActive(true);
            _savedDomainComponent = null;
        }
    }
}
