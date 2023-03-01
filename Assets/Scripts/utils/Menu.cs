/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Toggles menu visibility.
/// </summary>
public class Menu : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private Canvas _menu;
    
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

    private void Start()
    {
        _menu = _menu.GetComponent<Canvas>();
    }

    void Update()
    {
        /*
        if (!GlobalVariables.s_gridTogOn)
        {
            return;
        } */

        if (!_device.isValid)
        {
            GetDevice();
        }
        
        bool primaryValue;
        if (_device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue)
            && primaryValue)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        _menu.enabled = !_menu.enabled;
    }
}
