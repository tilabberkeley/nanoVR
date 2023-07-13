/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using System.Linq;
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
    private static Color[] s_colors = { Color.blue, Color.magenta, Color.green, Color.red, Color.cyan, Color.yellow };

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
        if (!s_gridTogOn)
        {
            return;
        }

        if (!s_drawTogOn && !s_eraseTogOn)
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
            if (s_hit.collider.name.Contains("nucleotide"))
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
            else if (s_hit.collider.name.Equals("xover") && s_eraseTogOn)
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

    public static void Highlight(GameObject go, Color color)
    {
        
        var comp = go.GetComponent<NucleotideComponent>();
        comp.Highlight(color);
        
    }

    public static void Unhighlight(GameObject go)
    {
        
        var comp = go.GetComponent<NucleotideComponent>();
        comp.Highlight(Color.black);
      
    }

    public static void DoCreateXover(GameObject startGO, GameObject endGO)
    {
        if (!IsValid(startGO, endGO))
        {
            return;
        }
        Strand startStrand = s_strandDict[startGO.GetComponent<NucleotideComponent>().StrandId];
        Strand endStrand = s_strandDict[endGO.GetComponent<NucleotideComponent>().StrandId];
        bool isFirstEnd = startGO == startStrand.GetHead() || startGO == startStrand.GetTail();
        bool isSecondEnd = endGO == endStrand.GetHead() || endGO == endStrand.GetTail();
        ICommand command = new XoverCommand(startGO, endGO, startGO == startStrand.GetHead(), isFirstEnd, isSecondEnd);
        CommandManager.AddCommand(command);
        command.Do();
    }

    public static bool IsValid(GameObject startGO, GameObject endGO)
    {
        var startNtc = startGO.GetComponent<NucleotideComponent>();
        int startDir = startNtc.Direction;
        Strand startStrand = s_strandDict[startNtc.StrandId];

        var endNtc = endGO.GetComponent<NucleotideComponent>();
        int endDir = endNtc.Direction;
        Strand endStrand = s_strandDict[endNtc.StrandId];


        //if (startDir != endDir && startHelix != endHelix)
        if (startGO == startStrand.GetHead() && endGO == endStrand.GetTail()
            || startGO == startStrand.GetTail() && endGO == endStrand.GetHead())
        {
            if (startDir != endDir)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates crossover and splits strands as necessary.
    /// </summary>
    public static GameObject CreateXover(GameObject startGO, GameObject endGO)
    {
        
        if (!IsValid(startGO, endGO))
        {
            return null;
        }
        

        int strandId = startGO.GetComponent<NucleotideComponent>().StrandId;
        Strand startStrand = s_strandDict[strandId];
        //int endStrandId = endGO.GetComponent<NucleotideComponent>().StrandId;
         //Strand endStrand = s_strandDict[endStrandId];

        // Create crossover.
        GameObject xover = DrawPoint.MakeXover(startGO, endGO, strandId);
        startStrand.AddXover(xover);

        /*
        // Handle strand splitting.
        List<GameObject> newStrand = SplitStrand(startGO, false);
        if (newStrand != null)
        {
            CreateStrand(newStrand, s_numStrands, startStrand.GetColor());
        }

        if (startStrand.GetTail() == startGO)
        {
            newStrand = SplitStrand(endGO, false);
        }
        else
        {
            newStrand = SplitStrand(endGO, true);
        }

        if (newStrand != null)
        {
            CreateStrand(newStrand, s_numStrands, endStrand.GetColor());
        }
        */

        // Handle strand merging.
        MergeStrand(startGO, endGO, xover);
        return xover;

    }

    public static void DoEraseXover(GameObject xover)
    {
        ICommand command = new EraseXoverCommand(xover, s_numStrands, xover.GetComponent<XoverComponent>().GetColor());
        CommandManager.AddCommand(command);
        command.Do();
    }

    public static void EraseXover(GameObject xover, int strandId, Color color, bool splitBefore)
    {   
        var xoverComp = xover.GetComponent<XoverComponent>();
        GameObject go = null;

        if (splitBefore)
        {
            go = xoverComp.GetNextGO();
        }
        else
        {
            go = xoverComp.GetPrevGO();
        }
        Strand strand = s_strandDict[go.GetComponent<NucleotideComponent>().StrandId];
        strand.DeleteXover(xover);
        SplitStrand(go, strandId, color, splitBefore);
    }

    public static void SplitStrand(GameObject go, int id, Color color, bool splitBefore)
    {
        /*var ntc = go.GetComponent<NucleotideComponent>();
        if (!ntc.Selected)
        {
            return null;
        }
        int strandId = ntc.StrandId;
        Strand strand = s_strandDict[strandId];

        if (strand.GetHead() == go || strand.GetTail() == go)
        {
            return null;
        }
        if (splitAfter)
        {
            return strand.SplitAfter(go);

        }
        return strand.SplitBefore(go);*/

        var startNtc = go.GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);


        int goIndex = strand.GetIndex(go);
        if (splitBefore)
        {
            List<GameObject> xovers = strand.GetXoversBeforeIndex(goIndex);
            strand.RemoveXovers(xovers);
            CreateStrand(strand.SplitBefore(go), xovers, id, color);
        }
        else
        {
            List<GameObject> xovers = strand.GetXoversAfterIndex(goIndex);
            strand.RemoveXovers(xovers);
            CreateStrand(strand.SplitAfter(go), xovers, id, color);
        }
    }

    public static void CreateStrand(List<GameObject> nucleotides, List<GameObject> xovers, int strandId, Color color)
    {
        Strand strand = new Strand(nucleotides, xovers, strandId, color);
        strand.SetComponents();
        s_strandDict.Add(strandId, strand);
        DrawNucleotideDynamic.CreateButton(strandId);
        //DrawNucleotideDynamic.AddStrandToHelix(nucleotides[0]);
        s_numStrands += 1;
    }

    public static void CreateStrand(GameObject startGO, GameObject endGO, List<GameObject> xovers, int strandId, Color color)
    {
        List<GameObject> nucleotides = DrawNucleotideDynamic.MakeNuclList(startGO, endGO);
        Strand strand = new Strand(nucleotides, xovers, strandId, color);
        strand.SetComponents();
        s_strandDict.Add(strandId, strand);
        DrawNucleotideDynamic.CreateButton(strandId);
        //DrawNucleotideDynamic.AddStrandToHelix(nucleotides[0]);
        s_numStrands += 1;
    }

    public static void MergeStrand(GameObject firstGO, GameObject secondGO, GameObject backbone)
    {
        var firstNtc = firstGO.GetComponent<NucleotideComponent>();
        var secondNtc = secondGO.GetComponent<NucleotideComponent>();
        var xoverComp = backbone.GetComponent<XoverComponent>();
        int firstStrandId = firstNtc.StrandId;
        int secondStrandId = secondNtc.StrandId;
        Strand firstStrand = s_strandDict[firstStrandId];
        Strand secondStrand = s_strandDict[secondStrandId];

        /*
        if (secondGO == null)
        {
            firstStrand.SetComponents();
            return;
        } 
        */
        firstStrand.AddXovers(secondStrand.GetXovers());

        if (firstStrand.GetHead() == firstGO)
        {
            // Must add backbone between 2 strands.
            //firstStrand.AddToHead(backbone);
            xoverComp.SetPrevGO(secondGO);
            xoverComp.SetNextGO(firstGO);
            HandleCycle(firstStrand, secondStrand, true);
        }
        else if (firstStrand.GetTail() == firstGO)
        {
            // Must add backbone between 2 strands.
            //firstStrand.AddToTail(backbone);
            xoverComp.SetPrevGO(firstGO);
            xoverComp.SetNextGO(secondGO);
            HandleCycle(firstStrand, secondStrand, false);
        }
        //secondStrand.RemoveStrand();
        firstStrand.SetComponents();
        DrawNucleotideDynamic.AddStrandToHelix(secondGO);
    }

    public static void HandleCycle(Strand firstStrand, Strand secondStrand, bool addToHead)
    {
        // Handles cycles in strand.
        if (firstStrand.GetStrandId() != secondStrand.GetStrandId())
        {
            List<GameObject> nucleotides = secondStrand.GetNucleotides();
            SelectStrand.RemoveStrand(nucleotides[0]);
            // Add second strand
            if (addToHead)
            {
                firstStrand.AddToHead(nucleotides);
            }
            else
            {
                firstStrand.AddToTail(nucleotides);
            }
        }
        else
        {
            firstStrand.ShowHideCone(false);
        }
    }
}
