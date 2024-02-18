/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using static GlobalVariables;
using System;

/// <summary>
/// Controls all logic for strand settings UI. This includes setting strand as scaffold and assigning DNA sequence.
/// </summary>
public class StrandSettings : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    private static GameObject s_GO;
    private static Strand s_strand;
    private bool gripReleased = true;

    // UI elements
    [SerializeField] private Canvas _menu;
    [SerializeField] private Canvas _strandSettings;
    [SerializeField] private Toggle _scaffoldTog;
    [SerializeField] private Toggle _tog7249;
    [SerializeField] private Toggle _tog8064;
    [SerializeField] private Toggle _customToggle;
    [SerializeField] private Button _OKButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TMP_InputField _sequenceInput;
    [SerializeField] private TMP_InputField _rotationInput;


    private void Start()
    {
        _strandSettings.enabled = false;
        _OKButton.onClick.AddListener(() => HideStrandSettings());
        _OKButton.onClick.AddListener(() => SetSettings());
        _cancelButton.onClick.AddListener(() => HideStrandSettings());
        _sequenceInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default); });
        _rotationInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default); });
        _customToggle.onValueChanged.AddListener(delegate { ToggleInputFields(); });
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
        s_strand.IsScaffold = _scaffoldTog.isOn;

        // Assigning DNA sequence to strand
        int length = s_strand.Length;
        int rotation = Convert.ToInt32(_rotationInput.text);

        if (_tog7249.isOn)
        {
            if (rotation + length > DNA8064.Length)
            {
                Debug.Log("Rotation of DNA sequence out of bounds for strand length.");
                return;
            }
            s_strand.Sequence = DNA7249.Substring(rotation, length);
        }
        else if (_tog8064.isOn)
        {
            if (rotation + length > DNA8064.Length)
            {
                Debug.Log("Rotation of DNA sequence out of bounds for strand length.");
                return;
            }
            s_strand.Sequence = DNA8064.Substring(rotation, length);
        }
        else if (_customToggle.isOn)
        {
            string sequence = _sequenceInput.text;

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
                Debug.Log("Input sequence too long. Using first " + length + " bases of sequence.");
                sequence = sequence.Substring(0, length);
            }
            if (!ValidateSequence(sequence))
            {
                return;
            }
            s_strand.Sequence = sequence.ToUpper();
        }
    }

    /// <summary>
    /// Checks that DNA sequence only has A, T, G, and C.
    /// </summary>
    /// <param name="sequence">Custom DNA sequence that user inputs</param>
    /// <returns></returns>
    private bool ValidateSequence(string sequence)
    {
        for (int i = 0; i < sequence.Length; i++)
        {
            if (sequence[i] != 'A' && sequence[i] != 'T' && sequence[i] != 'G' && sequence[i] != 'C'
                && sequence[i] != 'a' && sequence[i] != 't' && sequence[i] != 'g' && sequence[i] != 'c')
            {
                return false;
            }
        }
        return true;
    }

    private void ToggleInputFields()
    {
        _sequenceInput.interactable = _customToggle.isOn;
        _rotationInput.interactable = !_customToggle.isOn;
    }

    private void ShowStrandSettings()
    {
        //s_menuEnabled = _menu.enabled;
        ToggleInputFields();
        _menu.enabled = false;
        _strandSettings.enabled = true;
        s_strand = s_strandDict[s_GO.GetComponent<NucleotideComponent>().StrandId];
        _scaffoldTog.isOn = s_strand.IsScaffold;
        _sequenceInput.text = s_strand.Sequence;
        _rotationInput.text = default;
    }

    private void HideStrandSettings()
    {
        _menu.enabled = true;
        _strandSettings.enabled = false;
    }
}
