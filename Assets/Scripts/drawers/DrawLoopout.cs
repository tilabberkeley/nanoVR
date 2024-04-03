using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Utils;
using static DrawCrossover;
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
        _inputField.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default); });
    }

    private void Update()
    {
        if (!s_loopoutOn || s_hideStencils)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        // SELECT LOOPOUT NUCLEOTIDE
        bool triggerValue;
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue
            && triggerReleased
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

                    DoCreateLoopout();
                }
            }
            else if (s_hit.collider.GetComponent<LoopoutComponent>() != null && s_eraseTogOn)
            {
                // Highlight(s_hit.collider.gameObject);
                // DoEraseLoopout(s_hit.collider.gameObject);
            }
            else
            {
                ResetNucleotides();
            }
        }

        // Resets triggers do avoid multiple selections.                                              
        if (!(_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue))
        {
            triggerReleased = true;
        }

        // Resets start and end nucleotide.
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue
            && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            ResetNucleotides();
        }

        // Grip handling
        bool gripValue;
        if (_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
                && gripValue
                && gripReleased
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            gripReleased = false;
            if (s_hit.collider.gameObject.GetComponent<LoopoutComponent>() != null)
            {
                s_loopout = s_hit.collider.gameObject;
                ShowEditPanel();
            }
        }

        // Resets grips to avoid multiple selections.                                              
        if (_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
                && !gripValue)
        {
            gripReleased = true;
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

    public void DoCreateLoopout()
    {
        Debug.Log("Doing loopout");
        CreateLoopout(s_startGO, s_endGO);
        // TODO: Add command
        //if (!IsValid(first, second))
        //{
        //    return;
        //}
        //Strand firstStr = s_strandDict[first.GetComponent<NucleotideComponent>().StrandId];
        //Strand secondStr = s_strandDict[second.GetComponent<NucleotideComponent>().StrandId];

        //// Bools help check if strands should merge with neighbors when xover is deleted or undo.
        //bool firstIsEnd = first == firstStr.Head || first == firstStr.Tail;
        //bool secondIsEnd = second == secondStr.Head || second == secondStr.Tail;
        //bool firstIsHead = first == firstStr.Head;
        //ICommand command = new XoverCommand(first, second, firstIsEnd, secondIsEnd, firstIsHead);
        //CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Splits strands, creates crossover, and merges strands.
    /// </summary>
    public GameObject CreateLoopout(GameObject firstGO, GameObject secondGO)
    {
        if (!IsValid(firstGO, secondGO))
        {
            Debug.Log("Invalid");
            return null;
        }

        Debug.Log("Valid");

        var firstNtc = firstGO.GetComponent<NucleotideComponent>();
        var secondNtc = secondGO.GetComponent<NucleotideComponent>();

        Debug.Log("Splitting");

        DrawSplit.SplitStrand(firstGO, s_numStrands, Strand.GetDifferentColor(firstNtc.Color), false);
        DrawSplit.SplitStrand(secondGO, s_numStrands, Strand.GetDifferentColor(secondNtc.Color), true);

        GameObject loopout = CreateLoopoutHelper(firstGO, secondGO, DEFAULT_LENGTH);
        MergeStrand(firstGO, secondGO, loopout);

        ResetNucleotides();

        return loopout;
    }

    public static GameObject CreateLoopoutHelper(GameObject startGO, GameObject endGO, int length)
    {
        Debug.Log("Creating Loopout");
        Debug.Log("Nuc 0 id:" + startGO.GetComponent<NucleotideComponent>().Id);
        Debug.Log("Nuc 1 id:" + startGO.GetComponent<NucleotideComponent>().Id);
        int strandId = startGO.GetComponent<NucleotideComponent>().StrandId;
        NucleotideComponent startNucleotide = startGO.GetComponent<NucleotideComponent>();
        NucleotideComponent endNucleotide = endGO.GetComponent<NucleotideComponent>();

        // Create loopout.
        LoopoutComponent loopout = DrawPoint.MakeLoopout(length, startNucleotide, endNucleotide, strandId);
        return loopout.gameObject;
    }

    // TODO: Erase stuff

    public static void DoEraseLoopout(GameObject xover)
    {
        ICommand command = new EraseXoverCommand(xover, s_numStrands, xover.GetComponent<XoverComponent>().Color);
        CommandManager.AddCommand(command);
    }

    public static void EraseXover(GameObject xover, int strandId, Color color, bool splitAfter)
    {
        var xoverComp = xover.GetComponent<XoverComponent>();
        GameObject go;

        if (splitAfter)
        {
            go = xoverComp.PrevGO;
        }
        else
        {
            go = xoverComp.NextGO;
        }
        Strand strand = s_strandDict[xoverComp.StrandId];
        strand.DeleteXover(xover);
        SplitStrand(go, strandId, color, splitAfter);
    }

    public static void SplitStrand(GameObject go, int id, Color color, bool splitAfter)
    {
        var startNtc = go.GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);

        if (splitAfter)
        {
            /*List<GameObject> xovers = strand.GetXoversBeforeIndex(goIndex);
            strand.RemoveXovers(xovers);*/
            CreateStrand(strand.SplitAfter(go), id, color);
        }
        else
        {
            /*List<GameObject> xovers = strand.GetXoversAfterIndex(goIndex);
            strand.RemoveXovers(xovers);*/
            /*List<GameObject> nucleotides = strand.SplitAfter(go);
            if (nucleotides.Count % 2 == 0) // Remove the trailing backbone
            {
                nucleotides.RemoveAt(nucleotides.Count - 1);
            }*/
            CreateStrand(strand.SplitBefore(go), id, color);
        }
    }

    // TODO: Edit stuff

    /// <summary>
    /// Edits insertion length to something other than default of 1.
    /// </summary>
    private void DoEditLoopout()
    {
        //if (ValidLoopout())
        //{
        //    int newLength = Int32.Parse(_inputField.text);
        //    ICommand command = new EditInsertionCommand(s_GO, newLength);
        //    CommandManager.AddCommand(command);
        //}
        int length = GetLengthFromText();
        EditLoopout(s_loopout, length);
    }

    /// <summary>
    /// Actual method that edits insertion.
    /// </summary>
    /// <param name="go">Gameobject that is being edited.</param>
    /// <param name="length">New length of insertion.</param>
    public static void EditLoopout(GameObject go, int length)
    {
        //var ntc = go.GetComponent<NucleotideComponent>();
        //if (ntc.IsInsertion)
        //{
        //    ntc.Insertion = length;
        //}
        //Debug.Log(ntc.Insertion);
        s_loopout.GetComponent<LoopoutComponent>().SequenceLength = length;
    }

    private void ShowEditPanel()
    {
        Debug.Log("Opening edit menu");
        s_menuEnabled = _menu.enabled;
        _menu.enabled = false;
        _editPanel.enabled = true;
        _currLengthText.SetText(CURRENT_LENGTH_PREFIX + s_loopout.GetComponent<LoopoutComponent>().SequenceLength);
    }

    private void HideEditPanel()
    {
        _menu.enabled = s_menuEnabled;
        _editPanel.enabled = false;
    }
}
