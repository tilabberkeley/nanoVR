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

/// <summary>
/// Manages all of the edit menus. This includes Strand Settings, Nucleotide Edit, and Insertion Edit.
/// </summary>
public class EditOptionsManager : MonoBehaviour
{
    // XR Controller elements
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    private static GameObject s_GO;
    private bool gripReleased = true;

    // Edit Options UI elements
    [SerializeField] private Canvas _menu;
    [SerializeField] private Canvas _editMenu;
    [SerializeField] private Canvas _strandSettings;
    [SerializeField] private Canvas _nuclEditMenu;
    [SerializeField] private Canvas _insEditMenu;
    [SerializeField] private Canvas _loopoutLengthEditMenu;
    [SerializeField] private Canvas _loopoutSequenceEditMenu;

    [SerializeField] private Button _strandSettingsBtn;
    [SerializeField] private Button _nuclEditBtn;
    [SerializeField] private Button _insEditBtn;
    [SerializeField] private Button _loopoutLengthEditBtn;
    [SerializeField] private Button _loopoutSequenceEditBtn;
    [SerializeField] private Button _cancelButton;

    // Strand Settings UI
    [SerializeField] private Toggle _scaffoldTog;
    [SerializeField] private Toggle _customTog;
    [SerializeField] private Toggle _strandComplementaryTog;
    [SerializeField] private TMP_InputField _strandSequenceInput;
    [SerializeField] private TMP_InputField _rotationInput;

    // Nucleotide Edit UI
    [SerializeField] private TMP_Text _nucleotideInfoText;
    [SerializeField] private Toggle _nucleotideComplementaryTog;
    [SerializeField] private TMP_InputField _nucleotideSequenceInput;

    // Loopout Sequence Edit UI
    [SerializeField] private TMP_Text _loopoutInfoText;
    [SerializeField] private TMP_InputField _loopoutSequenceInput;


    private void Start()
    {
        _editMenu.enabled = false;
        _strandSettings.enabled = false;
        _nuclEditMenu.enabled = false;
        _insEditMenu.enabled = false;
        _loopoutLengthEditMenu.enabled = false;
        _loopoutSequenceEditMenu.enabled = false;

        _strandSettingsBtn.onClick.AddListener(() => ShowStrandSettings());
        _nuclEditBtn.onClick.AddListener(() => ShowNuclEdit());
        _insEditBtn.onClick.AddListener(() => ShowInsEdit());
        _loopoutLengthEditBtn.onClick.AddListener(() => ShowLoopoutLengthEdit());
        _loopoutSequenceEditBtn.onClick.AddListener(() => ShowLoopoutSequenceEdit());
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
            if ((s_hit.collider.GetComponent<NucleotideComponent>() != null
                && s_hit.collider.GetComponent<NucleotideComponent>().Selected)
                || s_hit.collider.GetComponent<LoopoutComponent>() != null)
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
        Highlight.UnhighlightGO(s_GO, false);
        Highlight.HighlightGO(s_GO, Color.yellow);
        _menu.enabled = false;
        _editMenu.enabled = true;
    }

    private void HideEditMenu()
    {
        _menu.enabled = true;
        _editMenu.enabled = false;
        Highlight.UnhighlightGO(s_GO, false);
    }

    private void ShowCorrectButtons() // TODO: Add loopout button functionality
    {
        NucleotideComponent ntc = s_GO.GetComponent<NucleotideComponent>();
        LoopoutComponent loopoutComp = s_GO.GetComponent<LoopoutComponent>();
        if (loopoutComp != null)
        {
            _loopoutLengthEditBtn.interactable = true;
            _insEditBtn.interactable = false;
            _loopoutSequenceEditBtn.interactable = true;
            _insEditBtn.interactable = false;
            _nuclEditBtn.interactable = false;
            return;
        }

        _loopoutLengthEditBtn.interactable = false;
        _loopoutSequenceEditBtn.interactable = false;

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
        StrandSettings.Strand = Utils.GetStrand(s_GO);
        _strandComplementaryTog.isOn = true; // Always default to automatically assign complementary strand.
                                             // User needs to manually unselect this toggle to get DNA complement mismatch.
        _scaffoldTog.isOn = StrandSettings.Strand.IsScaffold;
        StrandSettings.IsScaffold = StrandSettings.Strand.IsScaffold;
        _strandSequenceInput.text = StrandSettings.Strand.Sequence;
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
        NucleotideComponent ntc = s_GO.GetComponent<NucleotideComponent>();
        _nuclEditMenu.enabled = true;
        NucleotideEdit.Nucleotide = ntc;

        // Set UI info
        int numberofBases = ntc.Insertion + 1;
        string text = "Number of bases: " + numberofBases;
        _nucleotideInfoText.text = text;
        _nucleotideSequenceInput.text = ntc.Sequence;
        _nucleotideComplementaryTog.isOn = true; // Default is to set complementary base. User must manually untoggle this.
    }

    private void ShowInsEdit()
    {
        _editMenu.enabled = false;
        _insEditMenu.enabled = true;
    }

    private void ShowLoopoutLengthEdit()
    {
        _editMenu.enabled = false;
        _loopoutLengthEditMenu.enabled = true;
    }

    private void ShowLoopoutSequenceEdit()
    {
        _editMenu.enabled = false;
        _loopoutSequenceEditMenu.enabled = true;
        LoopoutComponent loopComp = s_GO.GetComponent<LoopoutComponent>();
        LoopoutSequenceEdit.Loopout = loopComp;

        // Set UI info
        int numberofBases = loopComp.SequenceLength;
        string text = "Number of bases: " + numberofBases;
        _loopoutInfoText.text = text;
        _loopoutSequenceInput.text = loopComp.Sequence;
    }
}
