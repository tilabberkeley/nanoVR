/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

/// <summary>
/// Toggles menu visibility.
/// </summary>
public class Menu : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private GameObject _nanoVRUI;
    public Button[] tabButtons;
    public GameObject[] panels;
    bool primaryReleased = true;
    
    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
        if (_devices.Count > 0)
        {
            _device = _devices[0];
        }
    }

    private void Start()
    {
        // Initialize the menu by setting up the button click events
        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (i != 1)
            {
                panels[i].SetActive(false);
            }
            int buttonIndex = i; // Store the index in a separate variable to avoid closure issues
            tabButtons[i].onClick.AddListener(() => SelectTab(buttonIndex));
        }
    }

    public void SelectTab(int selectedButtonIndex)
    {
        // Disable all other buttons
        for (int i = 0; i < panels.Length; i++)
        {
            if (i != selectedButtonIndex)
            {
                panels[i].SetActive(false);
            }
        }

        panels[selectedButtonIndex].SetActive(true);

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
        if (_device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out primaryValue)
            && primaryValue && primaryReleased)
        {
            primaryReleased = false;
            ToggleMenu();
        }

        // Reset primary button
        if (_device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out primaryValue)
            && !primaryValue)
        {
            primaryReleased = true;
        }

    }

    public void ToggleMenu()
    {
        _nanoVRUI.SetActive(!_nanoVRUI.activeInHierarchy);
    }
}
