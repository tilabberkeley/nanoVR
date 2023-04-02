/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class UndoRedo : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    bool primaryReleased = true;
    bool secondaryReleased = true;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
        if (_devices.Count > 0)
        {
            _device = _devices[0];
        }
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
        if (!_device.isValid)
        {
            GetDevice();
        }

        bool primaryValue;
        if (_device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue)
            && primaryValue && primaryReleased)
        {
            primaryReleased = false;
            CommandManager.Undo();
        }

        bool secondaryValue;
        if (_device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryValue)
            && secondaryValue && secondaryReleased)
        {
            secondaryReleased = false;
            CommandManager.Redo();
        }

        // Reset primary button
        if (_device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue)
            && !primaryValue)
        {
            primaryReleased = true;
        }

        // Reset secondary button
        if (_device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryValue)
            && !secondaryValue)
        {
            secondaryReleased = true;
        }
    }
}
