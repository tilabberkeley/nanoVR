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

    // Static variables
    private static Strand s_strand;
    public static Strand Strand { get { return s_strand; } set { s_strand = value; } }
    private static bool s_isScaffold;
    public static bool IsScaffold { set { s_isScaffold = value; } }


    /// <summary>
    /// Sets strand settings such as DNA sequence and scaffold.
    /// Called by Strand Settings OK button.
    /// </summary>
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
        sequence = sequence.ToUpper();
        s_strand.Sequence = sequence;
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
   
    /// <summary>
    /// Checks if strands insertions and deletions are aligned with its complement nucleotides.
    /// </summary>
    private bool AlignedInsertionsDeletions(Strand strand)
    {
        bool isAligned = true;
        List<GameObject> nucleotides = strand.Nucleotides;
        for (int i = nucleotides.Count - 1; i >= 0; i--)
        {
            NucleotideComponent ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            isAligned = NucleotideEdit.ValidComplementary(ntc) && isAligned;
            if (!isAligned) return isAligned;
        }
        Debug.Log("Can assign complementary bases");
        return true;
    }

    /// <summary>
    /// Sets DNA sequence of complementary nucleotides
    /// </summary>
    private void SetComplementary(string sequence)
    {
        List<GameObject> nucleotides = s_strand.Nucleotides;
        int seqCount = 0;

        for (int i = nucleotides.Count - 1; i >= 0; i--)
        {
            NucleotideComponent ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            if (i == nucleotides.Count - 1 || i == 0)
            {
                CheckTrailingNucls(ntc);
            }
            if (ntc != null)
            {
                NucleotideEdit.SetComplementary(ntc, sequence.Substring(seqCount, ntc.Insertion + 1));
                if (!ntc.IsDeletion) seqCount += ntc.Insertion + 1;
            }
        }
        Debug.Log("Finished setting complementary bases");
    }

    /// <summary>
    /// Checks if any complementary strands have nucleotides that haven't been assigned a base yet.
    /// If they haven't, assigns "?" to them.
    /// </summary>
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

    /// <summary>
    /// Helper method that fills in all unassigned nucleotides with the "?" base.
    /// </summary>
    /// <param name="ntc"></param>
    /// <param name="towardTail">Boolean indicating whether or not the unassigned nucleotides are towards the tail of the strand.</param>
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

    
    // Called by cancel button in Strand Settings menu.
    public void HideStrandSettings()
    {
        _menu.enabled = true;
        _strandSettings.enabled = false;
    }
}
