/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
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

                    if (s_drawTogOn)
                    {
                        DoCreateXover(s_startGO, s_endGO);
                        ResetNucleotides();
                    }
                }
            }

            // Also have to make sure this isn't a loopout, otherwise erase xover cammand will
            // be added to stack when erasing a loopout because of the inheritance.
            else if (s_hit.collider.GetComponent<XoverComponent>() != null
                && s_hit.collider.GetComponent<LoopoutComponent>() == null
                && s_eraseTogOn)
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
    /// Returns whether there can be a crossover between the two inputted nucleotides.
    /// The nucleotides must be going in different directions and be apart of a strand.
    /// Addiionally, there can't be a crossover or loopout already on either of the
    /// nucleotides.
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

        if (startDir == endDir)
        {
            return false;
        }

        if (startNtc.Xover != null || endNtc.Xover != null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Does a xover command.
    /// </summary>
    public static void DoCreateXover(GameObject first, GameObject second)
    {
        if (!IsValid(first, second))
        {
            return;
        }
        Strand firstStr = Utils.GetStrand(first);
        Strand secondStr = Utils.GetStrand(second);

        // Bools help check if strands should merge with neighbors when xover is deleted or undo.
        bool firstIsHead = first == firstStr.Head;
        bool firstIsTail = first == firstStr.Tail;
        bool secondIsHead = second == secondStr.Head;
        bool secondIsEnd = secondIsHead || second == secondStr.Tail;
        ICommand command = new XoverCommand(first, second, firstIsHead, firstIsTail, secondIsHead, secondIsEnd);
        CommandManager.AddCommand(command);
        //command.Do();
    }

    /// <summary>
    /// Splits strands (if necessary), draws xover, and merges strands connected by xover
    /// </summary>
    public static GameObject CreateXover(GameObject startGO, GameObject endGO, bool splitFirstAfter = false)
    {
        if (!IsValid(startGO, endGO))
        {
            return null;
        }

        NucleotideComponent firstNtc = startGO.GetComponent<NucleotideComponent>();
        NucleotideComponent secondNtc = endGO.GetComponent<NucleotideComponent>();

        DrawSplit.SplitStrand(startGO, s_numStrands, Strand.GetDifferentColor(firstNtc.Color), splitFirstAfter);
        DrawSplit.SplitStrand(endGO, s_numStrands, Strand.GetDifferentColor(secondNtc.Color), !splitFirstAfter);

        GameObject xover = CreateXoverHelper(startGO, endGO);

        // Create circular strand
        if (firstNtc.StrandId == secondNtc.StrandId)
        {
            HandleCycle(startGO);
        }
        else
        {
            MergeStrand(startGO, endGO, xover);
        }
        return xover;
    }

    /// <summary>
    /// Helper method to create a crossover between given nuleotides.
    /// </summary>
    public static GameObject CreateXoverHelper(GameObject startGO, GameObject endGO)
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
        GameObject xover = DrawPoint.MakeXover(prevGO, nextGO, strandId, prevStrandId);

        return xover;
    }

    /// <summary>
    /// Does a erase crossover command.
    /// </summary>
    public static void DoEraseXover(GameObject xover)
    {
        ICommand command = new EraseXoverCommand(xover, s_numStrands);
        CommandManager.AddCommand(command);
        //command.Do();
    }

    /// <summary>
    /// Removes given crossover and creates a new strand with given strand id and color due to crossover deletion.
    /// </summary>
    public static void EraseXover(GameObject xover, int strandId, Color color, bool splitAfter)
    {
        XoverComponent xoverComp = xover.GetComponent<XoverComponent>();
        GameObject nucleotide;

        if (splitAfter)
        {
            nucleotide = xoverComp.PrevGO;
        }
        else
        {
            nucleotide = xoverComp.NextGO;
        }
        Strand strand = s_strandDict[xoverComp.StrandId];
        strand.DeleteXover(xover);
        DrawSplit.SplitStrand(nucleotide, strandId, color, splitAfter); // CHECK THIS
    }

    /* public static void SplitStrand(GameObject go, int id, Color color, bool splitAfter)
     {
         var startNtc = go.GetComponent<NucleotideComponent>();
         int strandId = startNtc.StrandId;
         s_strandDict.TryGetValue(strandId, out Strand strand);

         if (splitAfter)
         {
             CreateStrand(strand.SplitAfter(go), id, color);
         }
         else
         {
             CreateStrand(strand.SplitBefore(go), id, color);
         }
     }*/

    /// <summary>
    /// Splits strand at given nucleotide.
    /// </summary>
    public static void SplitStrand(GameObject nucleotide, int id, Color color, bool splitAfter)
    {
        var startNtc = nucleotide.GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);

        if (splitAfter)
        {
            /*List<GameObject> xovers = strand.GetXoversBeforeIndex(goIndex);
            strand.RemoveXovers(xovers);*/
            CreateStrand(strand.SplitAfter(nucleotide), id, color);
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
            CreateStrand(strand.SplitBefore(nucleotide), id, color);
        }
    }

    /// <summary>
    /// Merges two strands specificed by the two given nucleotide gameobjects along with the given xover.
    /// (This differs from the standard merge by also considering a xover).
    /// </summary>
    public static void MergeStrand(GameObject firstGO, GameObject secondGO, GameObject xover)
    {
        var firstNtc = firstGO.GetComponent<NucleotideComponent>();
        var secondNtc = secondGO.GetComponent<NucleotideComponent>();
        var xoverComp = xover.GetComponent<XoverComponent>();
        int firstStrandId = firstNtc.StrandId;
        int secondStrandId = secondNtc.StrandId;
        Strand firstStrand = s_strandDict[firstStrandId];
        Strand secondStrand = s_strandDict[secondStrandId];

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
        firstStrand.SetComponents();
    }

    private static void HandleCycle(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        s_strandDict.TryGetValue(ntc.StrandId, out Strand strand);
        strand.ShowHideCone(false);
        strand.IsCircular = true;
    }

    /// <summary>
    /// Given two nucleotide game objects, the out variables are set such that the start variables hold the respective
    /// variables for the strand with the smallest strand id. The end variables hold the respective variables for the
    /// strand with the largest strand id.
    /// </summary>
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
