/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using static GlobalVariables;

/// <summary>
/// Controls all logic for strand settings UI. This includes setting strand as scaffold and assigning DNA sequence.
/// </summary>
public class StrandSettings : MonoBehaviour
{
    /*[SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;*/
    public static Strand s_strand;
    public static bool s_isScaffold;
    //private bool gripReleased = true;

    // UI elements
    [SerializeField] private Canvas _menu;
    [SerializeField] private Canvas _strandSettings;
    [SerializeField] private Toggle _scaffoldTog;
    [SerializeField] private Toggle _tog7249;
    [SerializeField] private Toggle _tog7560;
    [SerializeField] private Toggle _tog8064;
    [SerializeField] private Toggle _tog8634;
    [SerializeField] private Toggle _customTog;
    [SerializeField] private Toggle _complementaryTog;
    [SerializeField] private Button _OKButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TMP_InputField _sequenceInput;
    [SerializeField] private TMP_InputField _rotationInput;

    /*private void Start()
    {
        _strandSettings.enabled = false;
        _OKButton.onClick.AddListener(() => HideStrandSettings());
        _OKButton.onClick.AddListener(() => SetSettings());
        _cancelButton.onClick.AddListener(() => HideStrandSettings());
        //_sequenceInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default); });
        //_rotationInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _customTog.onValueChanged.AddListener(delegate { ToggleInputFields(); });
    }*/

    /*private void GetDevice()
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
            if (s_hit.collider.GetComponent<NucleotideComponent>() != null
                && s_hit.collider.GetComponent<NucleotideComponent>().Selected
                && !s_hit.collider.GetComponent<NucleotideComponent>().IsInsertion
                && !s_hit.collider.GetComponent<NucleotideComponent>().IsDeletion)
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
    }*/

    public void SetSettings()
    {
        if ((s_isScaffold && !_scaffoldTog.isOn) || (!s_isScaffold && _scaffoldTog.isOn)) // Only update strand.IsScaffold if there is a change
        {
            s_strand.IsScaffold = _scaffoldTog.isOn;
        }

        // Get rotation amount
        int rotation;
        if (_rotationInput.text.Length == 0)
        {
            rotation = 0;
        }
        else
        {
            rotation = Convert.ToInt32(_rotationInput.text);
        }

        // Assigning DNA sequence to strand
        int length = s_strand.Length;
        string sequence = "";
        if (_tog7249.isOn)
        {
            if (rotation + length > DNA7249.Length)
            {
                Debug.Log("Rotation of DNA sequence out of bounds for strand length. DNA sequence not assigned.");
                return;
            }
            sequence = DNA7249.Substring(rotation, length);
        }
        else if (_tog7560.isOn)
        {
            if (rotation + length > DNA7560.Length)
            {
                Debug.Log("Rotation of DNA sequence out of bounds for strand length. DNA sequence not assigned.");
                return;
            }
            sequence = DNA7560.Substring(rotation, length);
        }
        else if (_tog8064.isOn)
        {
            if (rotation + length > DNA8064.Length)
            {
                Debug.Log("Rotation of DNA sequence out of bounds for strand length. DNA sequence not assigned.");
                return;
            }
            sequence = DNA8064.Substring(rotation, length);
        }
        else if (_tog8634.isOn)
        {
            if (rotation + length > DNA8634.Length)
            {
                Debug.Log("Rotation of DNA sequence out of bounds for strand length. DNA sequence not assigned.");
                return;
            }
            sequence = DNA8634.Substring(rotation, length);
        }
        else if (_customTog.isOn)
        {
            sequence = _sequenceInput.text;

            if (!NucleotideEdit.ValidateSequence(sequence))
            {
                return;
            }

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
        }

        s_strand.Sequence = sequence.ToUpper();
        Debug.Log("Finished setting this strand's sequence");
        if (_complementaryTog.isOn)
        {
            // If insertions and deletions are not aligned in current strand and complementary strand, we cannot assign DNA.
            if (!AlignedInsertionsDeletions(s_strand))
            {
                Debug.Log("Current strand has complementary nucleotides that do not match insertions/deletions. We cannot assign DNA.");
                return;
            }
            SetComplementary(sequence);
        }
        else
        {
            // User chose not to assign complementary strand sequence. This risks base mismatches, so we need to check.
            Utils.CheckMismatch(s_strand);
        }
    }
   
    private bool AlignedInsertionsDeletions(Strand strand)
    {
        bool isAligned = true;
        List<GameObject> nucleotides = strand.Nucleotides;
        for (int i = nucleotides.Count - 1; i >= 0; i--)
        {
            NucleotideComponent ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            isAligned = NucleotideEdit.ValidComplementary(ntc) && isAligned;
            if (!isAligned) return isAligned;
/*            if (ntc != null)
            {
                var compNtc = ntc.Complement.GetComponent<NucleotideComponent>();
                if (compNtc.Selected)
                {
                    if (ntc.IsDeletion && !compNtc.IsDeletion) return false;
                    if (!ntc.IsDeletion && compNtc.IsDeletion) return false;
                    if (ntc.IsInsertion && !compNtc.IsInsertion) return false;
                    if (!ntc.IsInsertion && compNtc.IsInsertion) return false;
                    if (ntc.Insertion != compNtc.Insertion) return false;
                }
            }*/
        }
        Debug.Log("Can assign complementary bases");
        return true;
    }

    private void SetComplementary(string sequence)
    {
        List<GameObject> nucleotides = s_strand.Nucleotides;
        int seqCount = 0;

        // TODO: Check endpoints of complements to see if they have tail nucleotides that need to be assigned "?"
        for (int i = nucleotides.Count - 1; i >= 0; i--)
        {
            if (i == nucleotides.Count - 1 || i == 0)
            {
                CheckTrailingNucls(nucleotides[i].GetComponent<NucleotideComponent>());
            }
            NucleotideComponent ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            if (ntc != null)
            {
                NucleotideEdit.SetComplementary(nucleotides[i], sequence.Substring(seqCount, ntc.Insertion + 1));
                if (!ntc.IsDeletion) seqCount += ntc.Insertion + 1;
            }
          
            /*var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            if (ntc != null)
            {
                var compNtc = ntc.Complement.GetComponent<NucleotideComponent>();
                if (compNtc.Selected)
                {
                    if (ntc.IsDeletion)
                    {
                        compNtc.Sequence = "X";
                    }
                    else
                    {
                        compNtc.Sequence = Utils.ComplementBase(sequence.Substring(seqCount, ntc.Insertion + 1));
                        seqCount += ntc.Insertion + 1;
                    }
                }
            }*/
        }
        Debug.Log("Finished setting complementary bases");
    }

    private void CheckTrailingNucls(NucleotideComponent ntc)
    {
        Strand strand = Utils.GetStrand(ntc.gameObject);
        NucleotideComponent compNtc = ntc.Complement.GetComponent<NucleotideComponent>();

        bool towardTail = true;
        if (ntc.gameObject == strand.Tail)
        {
           towardTail = false;
        }
        if (compNtc.Selected)
        {
            FillTrailingNucls(compNtc, towardTail);
        }
    }

    private void FillTrailingNucls(NucleotideComponent ntc, bool towardTail)
    {
        Strand strand = Utils.GetStrand(ntc.gameObject);
        int currIndex = strand.GetIndex(ntc.gameObject);
        int endIndex;
        if (towardTail) endIndex = strand.GetIndex(strand.Tail);
        else endIndex = strand.GetIndex(strand.Head);
        int i = currIndex + 1;
        if (currIndex > endIndex)
        {
            // Swap curr and end index
            int temp = currIndex;
            currIndex = endIndex;
            endIndex = temp;
            i = currIndex;
        }

        for (; i <= endIndex; i++)
        {
            NucleotideComponent currNtc = strand.Nucleotides[i].GetComponent<NucleotideComponent>();
            if (currNtc != null)
            {
                if (currNtc.Sequence.Equals("")) currNtc.Sequence = "?";
                else break;
            }
        }
    }

    /// <summary>
    /// Checks that DNA sequence only has A, T, G, and C.
    /// </summary>
    /// <param name="sequence">Custom DNA sequence that user inputs</param>
    /// <returns></returns>
    /*private bool ValidateSequence(string sequence)
    {
        for (int i = 0; i < sequence.Length; i++)
        {
            if (sequence[i] != 'A' && sequence[i] != 'T' && sequence[i] != 'G' && sequence[i] != 'C'
                && sequence[i] != 'a' && sequence[i] != 't' && sequence[i] != 'g' && sequence[i] != 'c')
            {
                Debug.Log(sequence[i] + " is not a valid DNA base. DNA sequence not assigned.");
                return false;
            }
        }
        return true;
    }*/

   /* public void ToggleInputFields()
    {
        _sequenceInput.interactable = _customTog.isOn;
        _rotationInput.interactable = !_customTog.isOn;
    }

    public void ShowStrandSettings(GameObject nucleotide)
    {
        //s_menuEnabled = _menu.enabled;
        ToggleInputFields();
        _menu.enabled = false;
        _strandSettings.enabled = true; 
        _nucleotide = nucleotide;

        s_strand = Utils.GetStrand(_nucleotide);

        _complementaryTog.isOn = true; // Always default to automatically assign complementary strand.
                                       // User needs to manually unselect this toggle to get DNA complement mismatch.
        _scaffoldTog.isOn = s_strand.IsScaffold;
        _isScaffold = s_strand.IsScaffold;
        _sequenceInput.text = s_strand.Sequence;
        _rotationInput.text = default;
    }*/

    // Called by cancel button in Unity Hierarchy
    public void HideStrandSettings()
    {
        _menu.enabled = true;
        _strandSettings.enabled = false;
    }
}
