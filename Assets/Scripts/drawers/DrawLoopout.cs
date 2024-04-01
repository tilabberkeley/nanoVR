using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Utils;
using static DrawCrossover;

public class DrawLoopout : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool triggerReleased = true;
    private static GameObject s_startGO = null;
    private static GameObject s_endGO = null;
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

        // SELECT CROSSOVER NUCLEOTIDE
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

                    CreateLoopout(s_startGO, s_endGO);
                    ResetNucleotides();
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

    public static void DoCreateLoopout(GameObject first, GameObject second)
    {
        // TODO: Add command
        if (!IsValid(first, second))
        {
            return;
        }
        Strand firstStr = s_strandDict[first.GetComponent<NucleotideComponent>().StrandId];
        Strand secondStr = s_strandDict[second.GetComponent<NucleotideComponent>().StrandId];

        // Bools help check if strands should merge with neighbors when xover is deleted or undo.
        bool firstIsEnd = first == firstStr.Head || first == firstStr.Tail;
        bool secondIsEnd = second == secondStr.Head || second == secondStr.Tail;
        bool firstIsHead = first == firstStr.Head;
        ICommand command = new XoverCommand(first, second, firstIsEnd, secondIsEnd, firstIsHead);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Splits strands, creates crossover, and merges strands.
    /// </summary>
    public static GameObject CreateLoopout(GameObject firstGO, GameObject secondGO)
    {
        if (!IsValid(firstGO, secondGO))
        {
            return null;
        }

        var firstNtc = firstGO.GetComponent<NucleotideComponent>();
        var secondNtc = secondGO.GetComponent<NucleotideComponent>();

        DrawSplit.SplitStrand(firstGO, s_numStrands, Strand.GetDifferentColor(firstNtc.Color), false);
        DrawSplit.SplitStrand(secondGO, s_numStrands, Strand.GetDifferentColor(secondNtc.Color), true);

        GameObject loopout = CreateLoopoutHelper(firstGO, secondGO);
        MergeStrand(firstGO, secondGO, loopout);
        return loopout;
    }

    public static GameObject CreateLoopoutHelper(GameObject startGO, GameObject endGO)
    {
        int strandId = startGO.GetComponent<NucleotideComponent>().StrandId;
        NucleotideComponent startNucleotide = startGO.GetComponent<NucleotideComponent>();
        NucleotideComponent endNucleotide = endGO.GetComponent<NucleotideComponent>();

        // Create crossover.
        LoopoutComponent loopout = DrawPoint.MakeLoopout(0, startNucleotide, endNucleotide, strandId);
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
}
