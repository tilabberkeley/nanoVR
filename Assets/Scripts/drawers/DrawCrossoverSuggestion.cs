using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using static GlobalVariables;
using static Highlight;

/// <summary>
/// Handles crossover suggestion interactions
/// </summary>
public class DrawCrossoverSuggestion : MonoBehaviour
{
    // Input controller variables
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool triggerReleased = true;
    bool triggerValue;

    // helper variables
    private RaycastHit _hit;
    private GameObject _currentXoverSuggestion = null;
    private bool _lookingAtXover = false;

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
        if (!s_drawTogOn)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool gotTriggerValue = _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue);
        bool hitFound = rightRayInteractor.TryGetCurrent3DRaycastHit(out _hit);

        // Set helper variables
        XoverSuggestionComponent xoverSuggestionComponent = null;
        bool hitIsXoverSuggestion = false;
        GameObject hitGO = null;
        bool isPrevXoverSuggestion = false;
        if (hitFound)
        {
            xoverSuggestionComponent = _hit.transform.GetComponent<XoverSuggestionComponent>();
            hitIsXoverSuggestion = xoverSuggestionComponent != null;
            hitGO = _hit.collider.gameObject;
            isPrevXoverSuggestion = ReferenceEquals(hitGO, _currentXoverSuggestion);
        }

        //Handles hovering over crossover suggestion
        if (hitFound)
        {
            if (hitIsXoverSuggestion)
            {
                if (!isPrevXoverSuggestion)
                {
                    _lookingAtXover = true;
                    _currentXoverSuggestion = hitGO;
                    HighlightXoverSuggestion(xoverSuggestionComponent);
                }
            }
            else
            {
                _lookingAtXover = false;
                if (_currentXoverSuggestion != null)
                {
                    UnhighlightXoverSuggestion(_currentXoverSuggestion.GetComponent<XoverSuggestionComponent>());
                }
            }
        }
        // Not currenlty looking at anything, so unhighlight if previously looking at crossover suggestion.
        else if (_lookingAtXover)
        {
            _lookingAtXover = false;
            UnhighlightXoverSuggestion(_currentXoverSuggestion.GetComponent<XoverSuggestionComponent>());
            _currentXoverSuggestion = null;
        }

        // Handles clicking on crossover suggestion.
        if (gotTriggerValue && triggerValue && triggerReleased)
        {
            triggerReleased = false;
            if (hitFound && hitIsXoverSuggestion)
            {
                _lookingAtXover = false;
                xoverSuggestionComponent.CreateXover();
            }
        }                                         
        // Trigger is released                                       
        else if (gotTriggerValue && !triggerReleased && !triggerValue)
        {
            triggerReleased = true;
        }
    }
}
