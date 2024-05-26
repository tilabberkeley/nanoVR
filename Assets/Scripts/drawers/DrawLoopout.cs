using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Utils;
using TMPro;
using System;

public class DrawLoopout : MonoBehaviour
{
    private const int DEFAULT_LENGTH = 1;
    private const string CURRENT_LENGTH_PREFIX = "Current Loopout Length: ";

    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    [SerializeField] private Canvas _menu;
    [SerializeField] private Canvas _editPanel;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TMP_Text _currLengthText;
    [SerializeField] private Button _OKButton;
    [SerializeField] private Button _cancelButton;
    private bool triggerReleased = true;
    private bool gripReleased = true;
    private static GameObject s_startGO = null;
    private static GameObject s_endGO = null;
    private static GameObject s_loopout = null;
    private static bool s_menuEnabled;
    private static RaycastHit s_hit;

    private static GameObject s_loopoutCheck;

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

    private void Start()
    {
        _editPanel.enabled = false;
        _OKButton.onClick.AddListener(() => HideEditPanel());
        _OKButton.onClick.AddListener(() => DoEditLoopout());
        _cancelButton.onClick.AddListener(() => HideEditPanel());
        _inputField.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
    }

    private void Update()
    {
        if (!(s_loopoutOn || s_eraseTogOn) || s_hideStencils)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        // Trigger on and there's a ray interaction
        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);
        if (triggerValue && triggerReleased
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            if (s_hit.collider.GetComponent<NucleotideComponent>() != null)
            {
                if (s_startGO == null)
                {
                    s_startGO = s_hit.collider.gameObject;
                    //Highlight(s_startGO);
                }
                else
                {
                    s_endGO = s_hit.collider.gameObject;
                    //Unhighlight(s_startGO);

                    DoCreateLoopout(s_startGO, s_endGO);
                    ResetNucleotides();
                }
            }
            else if (s_hit.collider.GetComponent<LoopoutComponent>() != null && s_eraseTogOn)
            {
                DoEraseLoopout(s_hit.collider.gameObject);
            }
            else
            {
                ResetNucleotides();
            }
        }

        // Resets triggers do avoid multiple selections.                                              
        if (!triggerValue)
        {
            triggerReleased = true;
        }

        // Resets start and end nucleotide.
        if (triggerValue && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            ResetNucleotides();
        }
    }

    /// <summary>
    /// Resets the start and end nucleotides.
    /// </summary>
    public static void ResetNucleotides()
    {
        //Unhighlight(s_startGO);
        s_startGO = null;
        s_endGO = null;
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
        int length = int.Parse(_inputField.text);
        // Clears input field.
        _inputField.Select();
        _inputField.text = "";
        if (ValidLoopoutLength(length))
        {
            return length;
        }
        return 0;
    }

    /// <summary>
    /// Does a loopout command.
    /// </summary>
    public static void DoCreateLoopout(GameObject first, GameObject second)
    {
        if (!DrawCrossover.IsValid(first, second))
        {
            return;
        }
        Strand firstStr = Utils.GetStrand(first);
        Strand secondStr = Utils.GetStrand(second);

        // Bools help check if strands should merge with neighbors when xover is deleted or undo.
        bool firstIsEnd = first == firstStr.Head || first == firstStr.Tail;
        bool secondIsEnd = second == secondStr.Head || second == secondStr.Tail;
        bool firstIsHead = first == firstStr.Head;
        ICommand command = new LoopoutCommand(first, second, firstIsEnd, secondIsEnd, firstIsHead, DEFAULT_LENGTH);
        CommandManager.AddCommand(command);
        //command.Do();
    }

    /// <summary>
    /// Splits strands (if necessary), draws loopout, and merges strands connected by loopout
    /// </summary>
    public static GameObject CreateLoopout(GameObject startGO, GameObject endGO, int sequenceLength)
    {
        if (!DrawCrossover.IsValid(startGO, endGO))
        {
            return null;
        }

        NucleotideComponent firstNtc = startGO.GetComponent<NucleotideComponent>();
        NucleotideComponent secondNtc = endGO.GetComponent<NucleotideComponent>();

        DrawSplit.SplitStrand(startGO, s_numStrands, Strand.GetDifferentColor(firstNtc.Color), false);
        DrawSplit.SplitStrand(endGO, s_numStrands, Strand.GetDifferentColor(secondNtc.Color), true);

        GameObject loopout = CreateLoopoutHelper(startGO, endGO, sequenceLength);

        // TODO: Handle circular loopouts (also should be able to be circular with xovers)

        DrawCrossover.MergeStrand(startGO, endGO, loopout);
        return loopout;
    }

    /// <summary>
    /// Helper method to create a loopout between given nuleotides.
    /// </summary>
    public static GameObject CreateLoopoutHelper(GameObject startGO, GameObject endGO, int sequenceLength)
    {
        int strandId = startGO.GetComponent<NucleotideComponent>().StrandId;
        int prevStrandId = endGO.GetComponent<NucleotideComponent>().StrandId;

        // Create crossover, assign appropiate prev and next properties.
        Strand startStr = Utils.GetStrand(startGO);
        Strand endStr = Utils.GetStrand(endGO);
        GameObject prevGO = startGO;
        GameObject nextGO = endGO;
        if (startGO == startStr.Head)
        {
            nextGO = startGO;
        }
        if (endGO == endStr.Tail)
        {
            prevGO = endGO;
        }
        GameObject loopout = DrawPoint.MakeLoopout(prevGO, nextGO, strandId, prevStrandId, sequenceLength);

        return loopout;
    }

    /// <summary>
    /// Does a erase loopout command.
    /// </summary>
    public static void DoEraseLoopout(GameObject loopout)
    {
        ICommand command = new EraseLoopoutCommand(loopout, s_numStrands);
        CommandManager.AddCommand(command);
        //command.Do();
    }

    /// <summary>
    /// Removes given loopout and creates a new strand with given strand id and color due to loopout deletion.
    /// </summary>
    public static void EraseLoopout(GameObject loopout, int strandId, Color color, bool splitBefore)
    {
        XoverComponent xoverComp = loopout.GetComponent<XoverComponent>();
        GameObject nucleotide;

        if (splitBefore)
        {
            nucleotide = xoverComp.NextGO;
        }
        else
        {
            nucleotide = xoverComp.PrevGO;
        }
        Strand strand = s_strandDict[xoverComp.StrandId];
        strand.DeleteXover(loopout);
        DrawSplit.SplitStrand(nucleotide, strandId, color, !splitBefore); // CHECK THIS
    }

    /// <summary>
    /// Does an edit loopout command.
    /// </summary>
    private void DoEditLoopout()
    {
        int length = GetLengthFromText();
        EditLoopoutCommand command = new EditLoopoutCommand(s_loopout, length);
        CommandManager.AddCommand(command);
        //command.Do();
    }

    /// <summary>
    /// Edits given loopout to the given length.
    /// </summary>
    public static void EditLoopout(GameObject loopout, int length)
    {
        loopout.GetComponent<LoopoutComponent>().SequenceLength = length;
    }

    /// <summary>
    /// Displays edit panel for loopout editting.
    /// </summary>
    private void ShowEditPanel()
    {
        s_menuEnabled = _menu.enabled;
        _menu.enabled = false;
        _editPanel.enabled = true;
        _currLengthText.SetText(CURRENT_LENGTH_PREFIX + s_loopout.GetComponent<LoopoutComponent>().SequenceLength);
    }

    /// <summary>
    /// Hides edit panel for loopout editting.
    /// </summary>
    private void HideEditPanel()
    {
        _menu.enabled = s_menuEnabled;
        _editPanel.enabled = false;
    }
}
