/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
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
    private const string CURRENT_LENGTH_PREFIX = "Current Loopout Length: ";

    // XR Controller elements
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    private static LoopoutComponent loopComp;
    private static NucleotideData nd;
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
    [SerializeField] private Button _domainExtensionBtn;
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

    // Loopout Length Edit UI
    [SerializeField] private TMP_InputField _loopLengthInputField;
    [SerializeField] private TMP_Text _loopCurrLengthText;
    [SerializeField] private Button _loopLengthOKButton;
    [SerializeField] private Button _loopLengthCancelButton;

    // Loopout Sequence Edit UI
    [SerializeField] private TMP_Text _loopoutInfoText;
    [SerializeField] private TMP_InputField _loopoutSequenceInput;

    // Insertion Edit UI
    [SerializeField] private TMP_InputField _insertionInputField;
    [SerializeField] private Button _insertionOKButton;
    [SerializeField] private Button _insertionCancelButton;


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
        _domainExtensionBtn.onClick.AddListener(() => MakeDomainExtension());
        _cancelButton.onClick.AddListener(() => HideEditMenu());

        _loopLengthOKButton.onClick.AddListener(() => HideLoopLengthMenu());
        _loopLengthOKButton.onClick.AddListener(() => DoEditLoopout());
        _loopLengthCancelButton.onClick.AddListener(() => HideLoopLengthMenu());
        _loopLengthInputField.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });

        _insertionOKButton.onClick.AddListener(() => HideInsertionMenu());
        _insertionOKButton.onClick.AddListener(() => DoEditInsertion());
        _insertionCancelButton.onClick.AddListener(() => HideInsertionMenu());
        _insertionInputField.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
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
        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);
        bool hit = rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit s_hit);

        if (gripValue && gripReleased && hit && !triggerValue)
        {
            gripReleased = false;
            if (s_hit.collider.GetComponent<NucleotideColliderComponent>() != null
                && s_hit.collider.GetComponent<NucleotideColliderComponent>().Data.IsSelected())
            {
                ShowEditMenu();
                nd = s_hit.collider.GetComponent<NucleotideColliderComponent>().Data;
                ShowNuclOptions(nd);
            }
            else if (s_hit.collider.GetComponent<LoopoutComponent>() != null)
            {
                ShowEditMenu();
                loopComp = s_hit.collider.GetComponent<LoopoutComponent>();
                ShowLoopoutOptions(loopComp);
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
        _menu.enabled = false;
        _editMenu.enabled = true;
    }

    private void HideEditMenu()
    {
        _menu.enabled = true;
        _editMenu.enabled = false;
        //Highlight.UnhighlightGO(s_GO, false);
    }

    private void ShowLoopoutOptions(LoopoutComponent loopoutComp)
    {
        //Highlight.UnhighlightGO(s_GO, false);
        //Highlight.HighlightGO(s_GO, Color.yellow);
        _loopoutLengthEditBtn.interactable = true;
        _insEditBtn.interactable = false;
        _loopoutSequenceEditBtn.interactable = true;
        _insEditBtn.interactable = false;
        _nuclEditBtn.interactable = false;
    }

    private void ShowNuclOptions(NucleotideData nd)
    {
        _loopoutLengthEditBtn.interactable = false;
        _loopoutSequenceEditBtn.interactable = false;

        if (nd.IsInsertion)
        {
            _insEditBtn.interactable = true;
        }
        else
        {
            _insEditBtn.interactable = false;
        }

        if (nd.IsDeletion)
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
        StrandSettings.Strand = nd.GetStrand();
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
        _nuclEditMenu.enabled = true;
        NucleotideEdit.Nucleotide = nd;

        // Set UI info
        int numberofBases = nd.Insertion + 1;
        string text = "Number of bases: " + numberofBases;
        _nucleotideInfoText.text = text;
        _nucleotideSequenceInput.text = nd.Sequence;
        _nucleotideComplementaryTog.isOn = true; // Default is to set complementary base. User must manually untoggle this.
    }

    private void ShowInsEdit()
    {
        _editMenu.enabled = false;
        _insEditMenu.enabled = true;
    }

    public void ShowLoopoutLengthEdit()
    {
        _editMenu.enabled = false;
        _loopoutLengthEditMenu.enabled = true;
        _loopCurrLengthText.SetText(CURRENT_LENGTH_PREFIX + loopComp.SequenceLength);
    }

    private void ShowLoopoutSequenceEdit()
    {
        _editMenu.enabled = false;
        _loopoutSequenceEditMenu.enabled = true;
        LoopoutSequenceEdit.Loopout = loopComp;

        // Set UI info
        int numberofBases = loopComp.SequenceLength;
        string text = "Number of bases: " + numberofBases;
        _loopoutInfoText.text = text;
        _loopoutSequenceInput.text = loopComp.Sequence;
    }

    private void MakeDomainExtension()
    {
        int numDomains = nd.GetStrand().Domains.Count;
        if (numDomains > 1 && (nd.GetDomain().Id == 0 || nd.GetDomain().Id == numDomains - 1))
        {
            nd.GetDomain().IsExtension = true;
        }
        else
        {
            Debug.Log("Cannot make domain extension. Strand must have more than one domain."); 
        }
    }

    private void HideInsertionMenu()
    {
        _menu.enabled = true;
        _insEditMenu.enabled = false;
        //Highlight.UnhighlightGO(EditOptionsManager.nd, false);
    }

    /// <summary>
    /// Returns whether new insertion length is valid.
    /// </summary>
    private bool ValidEdit()
    {
        try
        {
            int newLength = Int32.Parse(_insertionInputField.text);
            if (newLength <= 0)
            {
                Debug.Log("Insertion length must be positive.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Edits insertion length to something other than default of 1.
    /// </summary>
    private void DoEditInsertion()
    {
        if (ValidEdit())
        {
            int newLength = Int32.Parse(_insertionInputField.text);
            ICommand command = new EditInsertionCommand(nd, newLength);
            CommandManager.AddCommand(command);
        }
    }

    /// <summary>
    /// Actual method that edits insertion.
    /// </summary>
    /// <param name="go">Gameobject that is being edited.</param>
    /// <param name="length">New length of insertion.</param>
    public static void EditInsertion(NucleotideData nd, int length)
    {
        if (nd.IsInsertion)
        {
            nd.Insertion = length;
            string sequence = nd.GetStrand().Sequence;
            nd.GetStrand().SetSequenceRevamp(sequence);
        }
    }

    /// <summary>
    /// Returns whether the loopout length is valid. A valid length is strictly positive.
    /// </summary>
    /// <param name="length"></param>
    /// <returns>True if length is valid. Throws exception otherwise.</returns>
    private bool ValidLoopoutLength(int length)
    {
        if (length <= 0)
        {
            throw new Exception("Loopout length must be positive");
        }
        return true;
    }

    /// <summary>
    /// Returns inputted loopout length. Additionally, the input text is cleared.
    /// </summary>
    /// <returns>Loopout length. 0 if invalid.</returns>
    private int GetLengthFromText()
    {
        int length = int.Parse(_loopLengthInputField.text);
        // Clears input field.
        _loopLengthInputField.Select();
        _loopLengthInputField.text = "";
        if (ValidLoopoutLength(length))
        {
            return length;
        }
        return 0;
    }

    /// <summary>
    /// Does an edit loopout command.
    /// </summary>
    private void DoEditLoopout()
    {
        int length = GetLengthFromText();
        EditLoopoutCommand command = new EditLoopoutCommand(loopComp, length);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Edits given loopout to the given length.
    /// </summary>
    public static void EditLoopout(LoopoutComponent loopout, int length)
    {
        loopout.SequenceLength = length;
    }

    /// <summary>
    /// Hides edit panel for loopout editting.
    /// </summary>
    private void HideLoopLengthMenu()
    {
        _menu.enabled = true;
        _loopoutLengthEditMenu.enabled = false;
        //Highlight.UnhighlightGO(EditOptionsManager.s_GO, false);
    }
}
