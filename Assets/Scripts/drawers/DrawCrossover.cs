/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Utils;

/// <summary>
/// Handles crossovers and necessary strand operations.
/// </summary>
public class DrawCrossover : MonoBehaviour
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
        if ((!s_drawTogOn && !s_eraseTogOn) || s_hideStencils)
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

                    if (s_drawTogOn)
                    {
                        DoCreateXover(s_startGO, s_endGO);
                        ResetNucleotides();
                    }
                }
            }
            else if (s_hit.collider.GetComponent<XoverComponent>() != null && s_eraseTogOn)
            {
                //Highlight(s_hit.collider.gameObject);
                DoEraseXover(s_hit.collider.gameObject);        
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

    public static void DoCreateXover(GameObject first, GameObject second)
    {
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
    /// Returns whether there can be a crossover between the two inputted nucleotides.
    /// The nucleotides must be going in different directions and be apart of a strand.
    /// TODO: Nucleotides must also not have a crossover on them already.
    /// </summary>
    /// <param name="startGO">First nucleotide game object.</param>
    /// <param name="endGO">Second nucleotide game object.</param>
    /// <returns>Whether there can be a crossover between the two nucleotide.</returns>
    public static bool IsValid(GameObject startGO, GameObject endGO)
    {
        var startNtc = startGO.GetComponent<NucleotideComponent>();
        int startId = startNtc.StrandId;
        int startDir = startNtc.Direction;

        var endNtc = endGO.GetComponent<NucleotideComponent>();
        int endId = endNtc.StrandId;
        int endDir = endNtc.Direction;

        if (startId == -1 || endId == -1)
        {
            return false;
        }

        if (startId == endId)
        {
            return false;
        }

        if (startDir == endDir)
        {
            return false;
        }

        /*Strand startStrand = s_strandDict[startNtc.StrandId];
        Strand endStrand = s_strandDict[endNtc.StrandId];


        //if (startDir != endDir && startHelix != endHelix)
        if (startGO == startStrand.GetHead() && endGO == endStrand.GetTail()
            || startGO == startStrand.GetTail() && endGO == endStrand.GetHead())
        {
            if (startDir != endDir)
            {
                return true;
            }
        }*/

        return true;
    }

    /// <summary>
    /// Splits strands, creates crossover, and merges strands.
    /// </summary>
    public static GameObject CreateXover(GameObject firstGO, GameObject secondGO)
    {
        if (!IsValid(firstGO, secondGO))
        {
            return null;
        }

        var firstNtc = firstGO.GetComponent<NucleotideComponent>();
        var secondNtc = secondGO.GetComponent<NucleotideComponent>();

        DrawSplit.SplitStrand(firstGO, s_numStrands, Strand.GetDifferentColor(firstNtc.Color), false);
        DrawSplit.SplitStrand(secondGO, s_numStrands, Strand.GetDifferentColor(secondNtc.Color), true);

        GameObject xover = CreateXoverHelper(firstGO, secondGO);
        MergeStrand(firstGO, secondGO, xover);
        return xover;
    }

    public static GameObject CreateXoverHelper(GameObject startGO, GameObject endGO)
    {
        int strandId = startGO.GetComponent<NucleotideComponent>().StrandId;
        //s_strandDict.TryGetValue(strandId, out Strand startStrand);

        // Create crossover.
        GameObject xover = DrawPoint.MakeXover(startGO, endGO, strandId);
        //startStrand.AddXover(xover);
        //startStrand.SetXoverColor(xover);
        return xover;
    }

    public static void DoEraseXover(GameObject xover)
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

    public static void MergeStrand(GameObject firstGO, GameObject secondGO, GameObject xover)
    {
        var firstNtc = firstGO.GetComponent<NucleotideComponent>();
        var secondNtc = secondGO.GetComponent<NucleotideComponent>();
        var xoverComp = xover.GetComponent<XoverComponent>();
        int firstStrandId = firstNtc.StrandId;
        int secondStrandId = secondNtc.StrandId;
        Strand firstStrand = s_strandDict[firstStrandId];
        Strand secondStrand = s_strandDict[secondStrandId];

        //if (firstStrandId < secondStrandId)
        //{
        //    firstStrand = s_strandDict[firstStrandId];
        //    secondStrand = s_strandDict[secondStrandId];
        //}
        //else
        //{
        //    firstStrand = s_strandDict[secondStrandId];
        //    secondStrand = s_strandDict[firstStrandId];

        //    GameObject temp = firstGO;
        //    firstGO = secondGO;
        //    secondGO = temp;
        //}

        if (firstStrand.Head == firstGO && secondStrand.Tail == secondGO)
        {
            xoverComp.PrevGO = secondGO;
            xoverComp.NextGO = firstGO;
            List<GameObject> nucleotides = secondStrand.Nucleotides;
            SelectStrand.RemoveStrand(nucleotides[0]);
            firstStrand.AddToHead(nucleotides);
        }
        else if (firstStrand.Tail == firstGO && secondStrand.Head == secondGO)
        {
            xoverComp.PrevGO = firstGO;
            xoverComp.NextGO = secondGO;
            List<GameObject> nucleotides = secondStrand.Nucleotides;
            SelectStrand.RemoveStrand(nucleotides[0]);
            firstStrand.AddToTail(nucleotides);
        }
        // xoverComp.PrevStrandId = secondStrandId;
        firstStrand.SetComponents();
    }

    public static void SetNucleotideDirection(GameObject firstGO, GameObject secondGO,
        out GameObject startGO, out GameObject endGO, out Strand startStrand, out Strand endStrand)
    {
        NucleotideComponent firstNtc = firstGO.GetComponent<NucleotideComponent>();
        NucleotideComponent secondNtc = secondGO.GetComponent<NucleotideComponent>();
        int firstStrandId = firstNtc.StrandId;
        int secondStrandId = secondNtc.StrandId;

        // Strand with smallest id will be start GO. This strand will also be preserved with the merge.
        if (firstStrandId < secondStrandId)
        {
            startStrand = s_strandDict[firstStrandId];
            endStrand = s_strandDict[secondStrandId];

            startGO = firstGO;
            endGO = secondGO;
        }
        else
        {
            startStrand = s_strandDict[secondStrandId];
            endStrand = s_strandDict[firstStrandId];

            startGO = secondGO;
            endGO = firstGO;
        }
    }

}
