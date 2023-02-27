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
    [SerializeField] public XRRayInteractor leftRayInteractor;
    [SerializeField] private Canvas _menu;
    
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
        
        bool secondaryValue;
        if (_device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryValue))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        _menu.enabled = !_menu.enabled;
    }
}
