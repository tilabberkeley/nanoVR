/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Toggles menu visibility.
/// </summary>
public class Menu : MonoBehaviour
{
    [SerializeField] private XRNode _leftXRNode;
    [SerializeField] private XRNode _rightXRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _leftDevice;
    private InputDevice _rightDevice;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    [SerializeField] private Canvas _menu;
    public Button[] tabButtons;
    public GameObject[] panels;
    bool primaryReleased = true;
    bool leftTriggerReleased = true;
    bool rightTriggerReleased = true;

    private void GetDevice()
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

    private void OnEnable()
    {
        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }
    }

    private void Start()
    {
        _menu = _menu.GetComponent<Canvas>();

        // Initialize the menu by setting up the button click events
        for (int i = 0; i < tabButtons.Length; i++)
        {
            /*if (i != 1)
            {
                panels[i].SetActive(false);
            }*/
            panels[i].SetActive(false);
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

    void Update()
    {
        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }

        _leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool primaryValue);
        _leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue);
        bool rightRayInteractorHit = rightRayInteractor.TryGetCurrentUIRaycastResult(out _);
        bool leftRayInteractorHit = rightRayInteractor.TryGetCurrentUIRaycastResult(out _);


        if (primaryValue && primaryReleased)
        {
            primaryReleased = false;
            ToggleMenu();
        }

        if (leftTriggerValue && leftTriggerReleased && !leftRayInteractorHit)
        {
            leftTriggerReleased = false;
            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].SetActive(false);
            }
        }

        if (rightTriggerValue && rightTriggerReleased && !rightRayInteractorHit)
        {
            rightTriggerReleased = false;
            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].SetActive(false);
            }
        }

        // Reset primary button
        if (!primaryValue)
        {
            primaryReleased = true;
        }

        if (!leftTriggerValue)
        {
            leftTriggerReleased = true;
        }

        if (!rightTriggerValue)
        {
            rightTriggerReleased = true;
        }
    }

    public void ToggleMenu()
    {
        _menu.enabled = !_menu.enabled;
        /*if (_menu.enabled )
        {
            leftRayInteractor.maxRaycastDistance = 1f;
            rightRayInteractor.maxRaycastDistance = 1f;
        }
        else
        {
            leftRayInteractor.maxRaycastDistance = 0.5f;
            rightRayInteractor.maxRaycastDistance = 0.5f;
        }*/
    }
}
