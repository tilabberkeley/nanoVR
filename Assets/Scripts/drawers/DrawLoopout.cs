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

    // Device and UI fields
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
    public static GameObject s_loopout = null;
    private static bool s_menuEnabled;
    private static RaycastHit s_hit;
    private static bool drawTempXover = false;
    private static NucleotideData s_startNuc = null;
    private static NucleotideData s_endNuc = null;
    private static GameObject s_hitHelixGO;
    private static GameObject tempXover = null;

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

    private void Awake()
    {
        tempXover = Instantiate(Xover, Vector3.zero, Quaternion.identity) as GameObject;
        tempXover.SetActive(false);
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

        // Get trigger state.
        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);

        // Check if trigger is pressed and was previously released and if we have a valid raycast hit.
        if (triggerValue && triggerReleased && rightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            triggerReleased = false;
            s_hitHelixGO = hit.collider.gameObject;

            // Check for nucleotide collider.
            if (s_hitHelixGO.TryGetComponent<NucleotideColliderComponent>(out var nucComp))
            {
                //Debug.Log("Hit nucleotide collider");
                NucleotideData nd = nucComp.Data;
                if (nd == null)
                {
                    ResetNucleotides();
                }
                else if (s_startNuc == null)
                {
                    s_startNuc = nd;
                }
                else
                {
                    s_endNuc = nd;
                    DoCreateLoopout(s_startNuc, s_endNuc);
                    ResetNucleotides();
                }
            }
            else if (hit.collider.GetComponent<XoverComponent>() == null &&
                     hit.collider.GetComponent<LoopoutComponent>()!= null &&
                     s_eraseTogOn)
            {
                // If the hit is on an existing crossover (and not a loopout), erase it.
                DoEraseLoopout(hit.collider.GetComponent<LoopoutComponent>());
            }
            else
            {
                ResetNucleotides();
            }
        }

        // Update the temporary xover visualization when trigger is not pressed.
        bool isHit = rightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit2);
        if (triggerReleased && !triggerValue && s_startNuc != null && isHit && hit2.collider.gameObject == s_hitHelixGO)
        {
            Domain domain = s_startNuc.GetDomain();
            if (s_startNuc == domain.GetHeadData() || s_startNuc == domain.GetTailData())
            {
                tempXover.SetActive(true);
                drawTempXover = true;
            }
        }

        if (drawTempXover)
        {
            Vector3 startPos = s_startNuc.GetPosition();
            Vector3 currentPos = rightRayInteractor.transform.position + rightRayInteractor.transform.forward * 0.7f; // Use hit point or recalc from helix data.
            UpdateXover(startPos, currentPos);
        }

        if (!triggerValue)
        {
            triggerReleased = true;
        }

        // If trigger is pressed but there is no valid raycast hit, reset nucleotides.
        if (triggerValue && !rightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit _))
        {
            triggerReleased = false;
            ResetNucleotides();
        }
    }

    /// <summary>
    /// Updates the temporary crossover object's position, rotation, and scale.
    /// </summary>
    private void UpdateXover(Vector3 start, Vector3 end)
    {
        tempXover.transform.position = (start + end) / 2.0f;
        Vector3 dirV = Vector3.Normalize(end - start);
        tempXover.transform.rotation = Quaternion.FromToRotation(Vector3.up, dirV);
        float dist = Vector3.Distance(start, end);
        tempXover.transform.localScale = new Vector3(XOVER_RAD, dist, XOVER_RAD);
    }

    /// <summary>
    /// Resets the virtual nucleotide selections and hides the temporary crossover visualization.
    /// </summary>
    private static void ResetNucleotides()
    {
        s_startNuc = null;
        s_endNuc = null;
        tempXover.SetActive(false);
        drawTempXover = false;
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
    public static void DoCreateLoopout(NucleotideData first, NucleotideData second)
    {
        if (!DrawCrossover.IsValid(first, second))
        {
            return;
        }
        ICommand command = new LoopoutCommand(first, second, DEFAULT_LENGTH);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Splits strands (if necessary), draws loopout, and merges strands connected by loopout
    /// </summary>
    /*public static GameObject CreateLoopout(GameObject startGO, GameObject endGO, int sequenceLength)
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
    }*/

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

    public static LoopoutComponent CreateLoopoutHelper(Domain prevDomain, Domain nextDomain, int strandId, int loopoutLength, int prevStrandId = -1, bool showXover = true)
    {
        // Create crossover, assign appropiate prev and next properties.
        GameObject loopout = DrawPoint.MakeLoopout(prevDomain, nextDomain);
        //Debug.Log("Finished creating loopout");
        LoopoutComponent loopoutComponent = loopout.AddComponent<LoopoutComponent>();
        loopoutComponent.PrevNucl = prevDomain.GetTailData();
        loopoutComponent.NextNucl = nextDomain.GetHeadData();
        loopoutComponent.SequenceLength = loopoutLength;
        loopoutComponent.StrandId = strandId;
        loopoutComponent.PrevStrandId = prevStrandId;

        prevDomain.NextXover = loopoutComponent;
        nextDomain.PrevXover = loopoutComponent;

        prevDomain.GetTailData().Xover = loopoutComponent;
        nextDomain.GetHeadData().Xover = loopoutComponent;

        loopoutComponent.Color = prevDomain.Color;
        loopoutComponent.SavedColor = nextDomain.Color;

        // Adds xover to each endpoint's helix
        prevDomain.GetHelix().AddXover(loopoutComponent);
        nextDomain.GetHelix().AddXover(loopoutComponent);

        loopoutComponent.IsLoopout = true;

        loopout.SetActive(showXover);
        return loopoutComponent;
    }

    public static LoopoutComponent CreateLoopout(NucleotideData nd1, NucleotideData nd2, int length)
    {
        if (!DrawCrossover.IsValid(nd1, nd2))
            return null;

        DrawCrossover.CalcPrevNextDomains(nd1, nd2, out Domain prevDomain, out Domain nextDomain);
        LoopoutComponent loopComp = CreateLoopoutHelper(prevDomain, nextDomain, nd1.StrandId, length, prevStrandId: nd2.StrandId);
        DrawCrossover.MergeStrand(nd1, nd2);
        return loopComp;
    }

    /// <summary>
    /// Does a erase loopout command.
    /// </summary>
    public static void DoEraseLoopout(LoopoutComponent loopout)
    {
        ICommand command = new EraseLoopoutCommand(loopout);
        CommandManager.AddCommand(command);
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
    public void ShowEditPanel()
    {
        _currLengthText.SetText(CURRENT_LENGTH_PREFIX + s_loopout.GetComponent<LoopoutComponent>().SequenceLength);
    }

    /// <summary>
    /// Hides edit panel for loopout editting.
    /// </summary>
    private void HideEditPanel()
    {
        _menu.enabled = s_menuEnabled;
        _editPanel.enabled = false;
        Highlight.UnhighlightGO(EditOptionsManager.s_GO, false);
    }
}
