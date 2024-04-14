/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Edit menu that assigns individual nucleotide information.
/// </summary>
public class NucleotideEdit : MonoBehaviour
{
    // UI elements
    [SerializeField] private Canvas _nuclEditMenu;
    [SerializeField] private Button _OKBtn;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Toggle _complementaryTog;
    [SerializeField] private TMP_InputField _sequenceInput;
    [SerializeField] private TMP_Text _nucleotideInfoText;

    // Static variables
    private static NucleotideComponent s_nucleotide;
    public static NucleotideComponent Nucleotide { set { s_nucleotide = value; } }

    /// <summary>
    /// Sets nucleotide DNA sequence. Called by Nucleotide Edit OK button.
    /// </summary>
    public void SetNucleotide()
    {
        var ntc = s_nucleotide.GetComponent<NucleotideComponent>();
        string sequence = _sequenceInput.text;
        int length = ntc.Insertion + 1;

        if (!ValidateSequence(sequence))
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
        sequence = sequence.ToUpper();
        ntc.Sequence = sequence;

        if (_complementaryTog.isOn)
        {
            // Set Complementary base
            if (!ValidComplementary(s_nucleotide)) return;
            SetComplementary(s_nucleotide, sequence);
        } 
        else
        {
            Utils.CheckMismatch(s_nucleotide);
        }
    }

    /// <summary>
    /// Checks that DNA sequence only has A, T, G, and C.
    /// </summary>
    /// <param name="sequence">Custom DNA sequence that user inputs</param>
    /// <returns></returns>
    public static bool ValidateSequence(string sequence)
    {
        for (int i = 0; i < sequence.Length; i++)
        {
            if (sequence[i] != 'A' && sequence[i] != 'T' && sequence[i] != 'G' && sequence[i] != 'C')
            {
                Debug.Log(sequence[i] + " is not a valid DNA base. DNA sequence not assigned.");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns if nucleotide input has valid complementary nucleotide.
    /// </summary>
    public static bool ValidComplementary(NucleotideComponent nucleotide)
    {
        if (nucleotide != null)
        {
            var compNtc = nucleotide.Complement.GetComponent<NucleotideComponent>();
            if (compNtc.Selected)
            {
                if (nucleotide.IsDeletion && !compNtc.IsDeletion) return false;
                if (!nucleotide.IsDeletion && compNtc.IsDeletion) return false;
                if (nucleotide.IsInsertion && !compNtc.IsInsertion) return false;
                if (!nucleotide.IsInsertion && compNtc.IsInsertion) return false;
                if (nucleotide.Insertion != compNtc.Insertion) return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Sets given nucleotide with given DNA sequence.
    /// </summary>
    public static void SetComplementary(NucleotideComponent ntc, string sequence)
    {
        if (ntc != null)
        {
            NucleotideComponent compNtc = ntc.Complement.GetComponent<NucleotideComponent>();
            if (compNtc.Selected)
            {
                if (ntc.IsDeletion)
                {
                    compNtc.Sequence = "X";
                }
                else
                {
                    compNtc.Sequence = Utils.ComplementBase(sequence);
                }
                Debug.Log("Finished setting complement of " + ntc.gameObject.name);
            }
        }
    }

    // Called by cancel button in Unity Hierarchy
    public void HideNuclEdit()
    {
        _nuclEditMenu.enabled = false;
    }
}
