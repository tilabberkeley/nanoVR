/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class EditOptionsManager : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    private static GameObject s_GO;
    private bool gripReleased = true;

    // UI elements
    [SerializeField] private Canvas _menu;
    [SerializeField] private Canvas _editMenu;
    [SerializeField] private Canvas _strandSettings;
    [SerializeField] private Canvas _nuclEditMenu;
    [SerializeField] private Canvas _insEditMenu;
    [SerializeField] private Button _strandSettingsBtn;
    [SerializeField] private Button _nuclEditBtn;
    [SerializeField] private Button _insEditBtn;
    [SerializeField] private Button _cancelButton;

    // Strand settings UI
    [SerializeField] private Toggle _scaffoldTog;
    [SerializeField] private Toggle _customTog;
    [SerializeField] private Toggle _strandComplementaryTog;
    [SerializeField] private TMP_InputField _strandSequenceInput;
    [SerializeField] private TMP_InputField _rotationInput;

    // Nucleotide Edit UI
    [SerializeField] private TMP_Text _nucleotideInfoText;
    [SerializeField] private Toggle _nucleotideComplementaryTog;
    [SerializeField] private TMP_InputField _nucleotideSequenceInput;

    private void Start()
    {
        _editMenu.enabled = false;
        _strandSettings.enabled = false;
        _nuclEditMenu.enabled = false;
        _insEditMenu.enabled = false;
        _strandSettingsBtn.onClick.AddListener(() => ShowStrandSettings());
        _nuclEditBtn.onClick.AddListener(() => ShowNuclEdit());
        _insEditBtn.onClick.AddListener(() => ShowInsEdit());
        _cancelButton.onClick.AddListener(() => HideEditMenu());
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

        // Checks grab button to show edit menu.
        _device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue);
        if (gripValue && gripReleased
                && rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit s_hit))
        {
            gripReleased = false;
            if (s_hit.collider.GetComponent<NucleotideComponent>() != null
                && s_hit.collider.GetComponent<NucleotideComponent>().Selected)
            {
                s_GO = s_hit.collider.gameObject;
                ShowEditMenu();
            }
        }

        // Resets grips to avoid multiple selections.                                              
        if (!gripValue)
        {
            gripReleased = true;
        }
    }

    private void ShowEditMenu()
    {
        ShowCorrectButtons();
        _menu.enabled = false;
        _editMenu.enabled = true;
    }

    private void HideEditMenu()
    {
        _menu.enabled = true;
        _editMenu.enabled = false;
    }

    private void ShowCorrectButtons()
    {
        var ntc = s_GO.GetComponent<NucleotideComponent>();
        if (ntc.IsInsertion)
        {
            _insEditBtn.interactable = true;
        }
        else
        {
            _insEditBtn.interactable = false;
        }

        if (ntc.IsDeletion)
        {
            _nuclEditBtn.interactable = false;
        }
        else
        {
            _nuclEditBtn.interactable = true;
        }
    }

    private void ShowStrandSettings()
    {
        _editMenu.enabled = false;
        ToggleInputFields();
        _menu.enabled = false;
        _strandSettings.enabled = true;
        StrandSettings.s_strand = Utils.GetStrand(s_GO);
        _strandComplementaryTog.isOn = true; // Always default to automatically assign complementary strand.
                                       // User needs to manually unselect this toggle to get DNA complement mismatch.
        _scaffoldTog.isOn = StrandSettings.s_strand.IsScaffold;
        StrandSettings.s_isScaffold = StrandSettings.s_strand.IsScaffold;
        _strandSequenceInput.text = StrandSettings.s_strand.Sequence;
        _rotationInput.text = default;
    }

    public void ToggleInputFields()
    {
        _strandSequenceInput.interactable = _customTog.isOn;
        _rotationInput.interactable = !_customTog.isOn;
    }

    private void ShowNuclEdit()
    {
        _editMenu.enabled = false;
        var ntc = s_GO.GetComponent<NucleotideComponent>();
        _nuclEditMenu.enabled = true;
        NucleotideEdit.s_nucleotide = s_GO;

        // Set nucleotide info textbox
        string sequence = ntc.Sequence;
        int numberofBases = ntc.Insertion + 1;
        string text = "Number of bases: " + numberofBases + "\n";
        text += "Current DNA sequence: " + sequence;
        _nucleotideInfoText.text = text;
        _nucleotideSequenceInput.text = ntc.Sequence;
        _nucleotideComplementaryTog.isOn = true; // Default is to set complementary base. User must manually untoggle this.
    }

    private void ShowInsEdit()
    {
        _editMenu.enabled = false;
        _insEditMenu.enabled = true;
    }
}
