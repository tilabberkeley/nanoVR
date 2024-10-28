/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class HelixColliderManager : MonoBehaviour
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
    private bool _rightTriggerReleased = true;
    private bool _rightGripReleased = true;
    private List<HelixComponent> helixComponents = new List<HelixComponent>();

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

        // Click to go deeper in viewing persepctives
        // Shift-click to go broader in viewing perspectives

        _leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool rightGripValue);

        bool leftRayInteractorHit = leftRayInteractor.TryGetCurrent3DRaycastHit(out s_hit);
        bool rightRayInteractorHit = rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit);

        /*if (leftTriggerValue && _leftTriggerReleased && leftRayInteractorHit)
        {
            _leftTriggerReleased = false;
            if (s_hit.collider.TryGetComponent<HelixComponent>(out var helixComp))
            {
                Debug.Log("Hit helix collider");
                helixComp.Helix.ToStrandView();
            }

            if (s_hit.collider.TryGetComponent<DomainComponent>(out var domain))
            {
                Debug.Log("Hit domain component in helix collider manager");
                domain.Helix.ToHelixView();
            }
        }*/

        if (!rightGripValue && rightTriggerValue && _rightTriggerReleased && rightRayInteractorHit)
        {
            _rightTriggerReleased = false;
            if (s_hit.collider.TryGetComponent<HelixComponent>(out var helixComp))
            {
                //Debug.Log("Hit helix collider");
                helixComp.Helix.ToStrandView();
            }
        }

        if (rightGripValue && rightTriggerValue && _rightTriggerReleased && rightRayInteractorHit)
        {
            _rightTriggerReleased = false;
            Debug.Log("Hit helix collider, trigger and grip");

            if (s_hit.collider.TryGetComponent<DomainComponent>(out var domain))
            {
                Debug.Log("Hit domain component in helix collider manager");
                domain.Helix.ToHelixView();
            }
        }

        if (!leftTriggerValue)
        {
            _leftTriggerReleased = true;
        }

        if (!rightTriggerValue)
        {
            _rightTriggerReleased = true;
        }

        if (!rightGripValue)
        {
            _rightGripReleased = true;
        }
    }
}
