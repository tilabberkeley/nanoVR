/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
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

        DrawSplit.SplitStrand(firstGO, s_numStrands, firstNtc.Color, false);
        DrawSplit.SplitStrand(secondGO, s_numStrands, secondNtc.Color, true);

        GameObject xover = CreateXoverHelper(firstGO, secondGO);

        // Create circular strand
        if (firstNtc.Id == secondNtc.Id)
        {
            HandleCycle(firstGO);
        }
        else
        {
            MergeStrand(firstGO, secondGO, xover);
        }
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
        DrawSplit.SplitStrand(go, strandId, color, splitAfter); // CHECK THIS
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

    private static void MergeStrand(GameObject firstGO, GameObject secondGO, GameObject xover)
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
}
