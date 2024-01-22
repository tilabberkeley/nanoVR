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
        if (!s_gridTogOn || !s_drawTogOn || s_hideStencils)
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
            else if (s_hit.collider.gameObject.GetComponent<XoverComponent>() != null && s_eraseTogOn)
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

    /// <summary>
    /// Returns whether there can be a crossover between the two inputted nucleotides.
    /// The nucleotides must be going in different directions and either be head to tail or
    /// tail to head.
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

        Strand startStrand = s_strandDict[startNtc.StrandId];
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
        s_strandDict.TryGetValue(strandId, out Strand startStrand);

        // Create crossover.
        GameObject xover = DrawPoint.MakeXover(startGO, endGO, strandId);
        startStrand.AddXover(xover);

        MergeStrand(startGO, endGO, xover);
        return xover;
    }

    public static GameObject CreateXover(int strandId, GameObject startGO, GameObject endGO)
    {
        s_strandDict.TryGetValue(strandId, out Strand startStrand);

        // Create crossover.
        GameObject xover = DrawPoint.MakeXover(startGO, endGO, strandId);
        startStrand.AddXover(xover);
        startStrand.SetComponents();
        return xover;
    }

    public static void DoEraseXover(GameObject xover)
    {
        ICommand command = new EraseXoverCommand(xover, s_numStrands, xover.GetComponent<XoverComponent>().Color);
        CommandManager.AddCommand(command);
        command.Do();
    }

    public static void EraseXover(GameObject xover, int strandId, Color color, bool splitBefore)
    {   
        var xoverComp = xover.GetComponent<XoverComponent>();
        GameObject go = null;

        if (splitBefore)
        {
            go = xoverComp.NextGO;
        }
        else
        {
            go = xoverComp.PrevGO;
        }
        Strand strand = s_strandDict[go.GetComponent<NucleotideComponent>().StrandId];
        strand.DeleteXover(xover);
        SplitStrand(go, strandId, color, splitBefore);
    }

    public static void SplitStrand(GameObject go, int id, Color color, bool splitBefore)
    {
        var startNtc = go.GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);

        int goIndex = strand.GetIndex(go);
        if (splitBefore)
        {
            List<GameObject> xovers = strand.GetXoversBeforeIndex(goIndex);
            strand.RemoveXovers(xovers);
            CreateStrand(strand.SplitBefore(go, true), xovers, id, color);
        }
        else
        {
            List<GameObject> xovers = strand.GetXoversAfterIndex(goIndex);
            strand.RemoveXovers(xovers);
            CreateStrand(strand.SplitAfter(go, true), xovers, id, color);
        }
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
            xoverComp.PrevGO = secondGO;
            xoverComp.NextGO = firstGO;
            HandleCycle(firstStrand, secondStrand, true);
        }
        else if (firstStrand.GetTail() == firstGO)
        {
            // Must add backbone between 2 strands.
            //firstStrand.AddToTail(backbone);
            xoverComp.PrevGO = firstGO;
            xoverComp.NextGO = secondGO;
            HandleCycle(firstStrand, secondStrand, false);
        }
        //secondStrand.RemoveStrand();
        firstStrand.SetComponents();
        //firstStrand.AddHelixId(secondGO.GetComponent<NucleotideComponent>().HelixId);
        //DrawNucleotideDynamic.AddStrandToHelix(secondGO);
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
