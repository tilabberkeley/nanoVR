/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NucleotideEdit : MonoBehaviour
{
    // UI elements
    [SerializeField] private Canvas _nuclEditMenu;
    [SerializeField] private Button _OKBtn;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Toggle _complementaryTog;
    [SerializeField] private TMP_InputField _sequenceInput;
    [SerializeField] private TMP_Text _nucleotideInfoText;
    public static GameObject s_nucleotide;

    /*public void ShowNuclEdit(GameObject nucleotide)
    {
        var ntc = nucleotide.GetComponent<NucleotideComponent>();
        _nuclEditMenu.enabled = true;
        s_nucleotide = nucleotide;

        // Set nucleotide info textbox
        string sequence = ntc.Sequence;
        int numberofBases = ntc.Insertion + 1;
        string text = "Number of bases: " + numberofBases + "\n";
        text += "Current DNA sequence: " + sequence;
        _nucleotideInfoText.text = text;

        _complementaryTog.isOn = true; // Default is to set complementary base. User must manually untoggle this.
    }*/

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
        ntc.Sequence = sequence;

        if (_complementaryTog.isOn)
        {
            // Set Complementary base
            if (!ValidComplementary(s_nucleotide.GetComponent<NucleotideComponent>())) return;
            SetComplementary(s_nucleotide, sequence);
        } 
        else
        {
            Utils.CheckMismatch(s_nucleotide.GetComponent<NucleotideComponent>());
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
            if (sequence[i] != 'A' && sequence[i] != 'T' && sequence[i] != 'G' && sequence[i] != 'C'
                && sequence[i] != 'a' && sequence[i] != 't' && sequence[i] != 'g' && sequence[i] != 'c')
            {
                Debug.Log(sequence[i] + " is not a valid DNA base. DNA sequence not assigned.");
                return false;
            }
        }
        return true;
    }

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

    public static void SetComplementary(GameObject nucleotide, string sequence)
    {
        var ntc = nucleotide.GetComponent<NucleotideComponent>();
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
