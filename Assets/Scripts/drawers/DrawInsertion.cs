/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Highlight;

/// <summary>
/// Add insertion to selected nucleotide.
/// </summary>
public class DrawInsertion : MonoBehaviour
{
    [SerializeField] private Canvas _menu;
    [SerializeField] private Canvas _editPanel;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _OKButton;
    [SerializeField] private Button _cancelButton;
    private TouchScreenKeyboard _keyboard;

    private static GameObject s_GO = null;
    private static bool s_menuEnabled;
    private const int DEFAULT_LENGTH = 1;

    private void Start()
    {
        _editPanel.enabled = false;
        _OKButton.onClick.AddListener(() => HideEditPanel());
        _OKButton.onClick.AddListener(() => DoEditInsertion());
        _cancelButton.onClick.AddListener(() => HideEditPanel());
        _inputField.onSelect.AddListener(delegate {TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
    }

    /// <summary>
    /// Command method to create insertion.
    /// </summary>
    /// <param name="go">Gameobject nucleotide of insertion.</param>
    /// <param name="length">Length of insertion.</param>
    public static void DoInsertion(NucleotideData nd, int length)
    {
        ICommand command = new InsertionCommand(nd, length);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Actual method that creates insertion.
    /// </summary>
    public static void Insertion(NucleotideData nd, int length)
    {
        if (nd.IsDeletion)
        {
            Debug.Log("Cannot draw insertion over deletion.");
            return;
        }
        if (!nd.IsSelected())
        {
            Debug.Log("Cannot draw insertion on unbound nucleotide.");
            return;
        }

        Strand strand = nd.GetStrand();

        if (nd.IsInsertion)
        {
            nd.Insertion = 0;
            UnhighlightInsertion(nd);
            nd.GetDomain().Insertions.Remove(nd.Id);
        }
        else
        {
            nd.Insertion = length;
            HighlightInsertion(nd);
            nd.GetDomain().Insertions.Add(nd.Id, length);
        }

        // Update strand DNA sequence
        if (strand != null)
        {
            string sequence = strand.Sequence;
            if (sequence.Length > 0)
            {
                strand.SetSequenceRevamp(sequence);
                Utils.CheckMismatch(strand);
            }
        }
    }

    /// <summary>
    /// Returns whether new insertion length is valid.
    /// </summary>
    private bool ValidEdit()
    {
        try
        {
            int newLength = Int32.Parse(_inputField.text);
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
            int newLength = Int32.Parse(_inputField.text);
            ICommand command = new EditInsertionCommand(s_GO, newLength);
            CommandManager.AddCommand(command);
        }
    }

    /// <summary>
    /// Actual method that edits insertion.
    /// </summary>
    /// <param name="go">Gameobject that is being edited.</param>
    /// <param name="length">New length of insertion.</param>
    public static void EditInsertion(GameObject go, int length)
    {
       /* var ntc = go.GetComponent<NucleotideComponent>();
        if (ntc.IsInsertion)
        {
            ntc.Insertion = length;
        }
        Debug.Log(ntc.Insertion);*/
    }

    private void ShowEditPanel()
    {
        s_menuEnabled = _menu.enabled;
        _menu.enabled = false;
        _editPanel.enabled = true;
    }

    private void HideEditPanel()
    {
        _menu.enabled = s_menuEnabled;
        _editPanel.enabled = false;
        Highlight.UnhighlightGO(EditOptionsManager.s_GO, false);
    }
}
