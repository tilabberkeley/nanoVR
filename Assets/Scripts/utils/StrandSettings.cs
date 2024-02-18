/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using static GlobalVariables;
using static Utils;

public class StrandSettings : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    private static GameObject s_GO;
    private static Strand s_strand;
    private bool gripReleased = true;

    [SerializeField] private Canvas _menu;
    [SerializeField] private Canvas _strandSettings;
    [SerializeField] private Toggle _scaffoldTog;
    [SerializeField] private Toggle _tog7249;
    [SerializeField] private Toggle _tog8064;
    [SerializeField] private Toggle _customToggle;
    [SerializeField] private Button _OKButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TMP_InputField _input;

    private void Start()
    {
        _strandSettings.enabled = false;
        ToggleInputField();
        _OKButton.onClick.AddListener(() => HideStrandSettings());
        _OKButton.onClick.AddListener(() => SetSettings());
        _cancelButton.onClick.AddListener(() => HideStrandSettings());
        _input.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default); });
        _customToggle.onValueChanged.AddListener(delegate { ToggleInputField() ; });
    }


    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
        if (_devices.Count > 0)
        {
            _device = _devices[0];
        }
    }

    private void OnEnable()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }
    }

    private void Update()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }
        // Checks grab button to show strand settings.
        _device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue);
        if (gripValue && gripReleased
                && rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit s_hit))
        {
            gripReleased = false;
            if (s_hit.collider.gameObject.GetComponent<NucleotideComponent>() != null
                && !s_hit.collider.gameObject.GetComponent<NucleotideComponent>().IsInsertion
                && !s_hit.collider.gameObject.GetComponent<NucleotideComponent>().IsDeletion)
            {
                s_GO = s_hit.collider.gameObject;
                ShowStrandSettings();
            }
        }

        // Resets grips to avoid multiple selections.                                              
        if (!gripValue)
        {
            gripReleased = true;
        }
    }

    private void SetSettings()
    {
        int length = s_strand.Length;
        if (_tog7249.isOn)
        {
            s_strand.Sequence = DNA7249.Substring(0, length);
        }
        else if (_tog8064.isOn)
        {
            s_strand.Sequence = DNA8064.Substring(0, length);
        }
        else if (_customToggle.isOn)
        {
            string sequence = _input.text;
            if (sequence.Length < length)
            {
                Debug.Log("Input sequence not long enough. Appending ? until correct length.");
                for (int i = 0; i < length - sequence.Length; i++)
                {
                    sequence += "?";
                }
            }

            else if (sequence.Length > length)
            {
                sequence = sequence.Substring(0, length);
                Debug.Log("Input sequence too long. Using first " + length + " bases of sequence.");
            }
            s_strand.Sequence = sequence;
        }
    }

    private void ToggleInputField()
    {
        if (_customToggle.isOn)
        {
            _input.interactable = true;
        }
        else
        {
            _input.interactable = false;
        }
    }

    private void ShowStrandSettings()
    {
        //s_menuEnabled = _menu.enabled;
        _menu.enabled = false;
        _strandSettings.enabled = true;
        s_strand = s_strandDict[s_GO.GetComponent<NucleotideComponent>().StrandId];
        _scaffoldTog.isOn = s_strand.IsScaffold;
        _input.text = s_strand.Sequence;
    }

    private void HideStrandSettings()
    {
        _menu.enabled = true;
        _strandSettings.enabled = false;
    }

}
