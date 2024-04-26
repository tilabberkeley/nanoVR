/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoopoutSequenceEdit : MonoBehaviour
{
    // UI elements
    [SerializeField] private Canvas _loopoutSequenceEditMenu;
    [SerializeField] private Button _OKBtn;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TMP_InputField _sequenceInput;
    [SerializeField] private TMP_Text _loopoutInfoText;

    // Static variables
    private static LoopoutComponent s_loopout;
    public static LoopoutComponent Loopout { set { s_loopout = value; } }

    /// <summary>
    /// Sets loopout DNA sequence. Called by Loopout Sequence Edit OK button.
    /// </summary>
    public void SetLoopout()
    {
        string sequence = _sequenceInput.text.ToUpper();
        int length = s_loopout.SequenceLength;

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

        s_loopout.Sequence = sequence;
    }



    /// <summary>
    /// Checks that DNA sequence only has A, T, G, and C.
    /// </summary>
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

    // Called by cancel button in Unity Hierarchy
    public void HideMenu()
    {
        _loopoutSequenceEditMenu.enabled = false;
    }
}
