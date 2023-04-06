/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
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
                Highlight(s_hit.collider.gameObject);
                if (s_eraseTogOn)
                {
                    DoEraseXover(s_hit.collider.gameObject);
                }
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
        Unhighlight(s_startGO);
        s_startGO = null;
        s_endGO = null;
    }

    public static void Highlight(GameObject go)
    {
        if (go.GetComponent<NucleotideComponent>() != null)
        {
            var comp = go.GetComponent<NucleotideComponent>();
            comp.Highlight(Color.green);
        }
        else
        {
            var comp = go.GetComponent<XoverComponent>();
            comp.Highlight(Color.green);
        }
    }

    public static void Unhighlight(GameObject go)
    {
        if (go.GetComponent<NucleotideComponent>() != null)
        {
            var comp = go.GetComponent<NucleotideComponent>();
            comp.Highlight(Color.black);
        }
        else
        {
            var comp = go.GetComponent<XoverComponent>();
            comp.Highlight(Color.black);
        }
    }

    public void DoCreateXover(GameObject startGO, GameObject endGO)
    {
        if (!IsValid(startGO, endGO))
        {
            return;
        }
        Strand startStrand = s_strandDict[startGO.GetComponent<NucleotideComponent>().GetStrandId()];
        Strand endStrand = s_strandDict[endGO.GetComponent<NucleotideComponent>().GetStrandId()];
        bool isFirstEnd = startGO == startStrand.GetHead() || startGO == startStrand.GetTail();
        bool isSecondEnd = endGO == endStrand.GetHead() || endGO == endStrand.GetTail();
        ICommand command = new XoverCommand(startGO, endGO, startGO == startStrand.GetTail(), isFirstEnd, isSecondEnd);
        CommandManager.AddCommand(command);
        command.Do();
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

        int strandId = startGO.GetComponent<NucleotideComponent>().GetStrandId();
        Strand startStrand = s_strandDict[strandId];
        int endStrandId = endGO.GetComponent<NucleotideComponent>().GetStrandId();
        Strand endStrand = s_strandDict[endStrandId];

        // Create crossover.
        DrawPoint d = new DrawPoint();
        GameObject xover = d.MakeXover(startGO, endGO, strandId);
        startStrand.AddXover(xover);

        // Handle strand splitting.
        List<GameObject> newStrand = SplitStrand(startGO, false);
        if (newStrand != null)
        {
            CreateStrand(newStrand, s_numStrands, startStrand.GetColor());
        }

        newStrand = SplitStrand(endGO, true);
        if (newStrand != null)
        {
            CreateStrand(newStrand, s_numStrands, endStrand.GetColor());
        }

        // Handle strand merging.
        MergeStrand(startGO, endGO, xover);
        return xover;
        
    }

    public static void DoEraseXover(GameObject xover)
    {
        ICommand command = new EraseXoverCommand(xover);
        CommandManager.AddCommand(command);
        command.Do();
    }
    public static void EraseXover(GameObject xover)
    {
        var xoverComp = xover.GetComponent<XoverComponent>();
        GameObject nextGO = xoverComp.GetNextGO();
        List<GameObject> newStrand = SplitStrand(nextGO, true);
        if (newStrand != null)
        {
            CreateStrand(newStrand, s_numStrands, nextGO.GetComponent<NucleotideComponent>().GetColor());
        }
        GameObject.Destroy(xover);
    }

    public static bool IsValid(GameObject startGO, GameObject endGO)
    {
        var startNtc = startGO.GetComponent<NucleotideComponent>();
        int startDir = startNtc.GetDirection();
        int startHelix = startNtc.GetHelixId();

        var endNtc = endGO.GetComponent<NucleotideComponent>();
        int endDir = endNtc.GetDirection();
        int endHelix = endNtc.GetHelixId();

        if (startDir != endDir && startHelix != endHelix)
        {
            return true;
        }
        return false;
    }

    public static List<GameObject> SplitStrand(GameObject go, bool first)
    {
        var startNtc = go.GetComponent<NucleotideComponent>();
        if (!startNtc.IsSelected())
        {
            return null;
        }
        int strandId = startNtc.GetStrandId();
        Strand strand = s_strandDict[strandId];

        if (strand.GetHead() == go || strand.GetTail() == go)
        {
            return null;
        }
        if (first)
        {
            return strand.SplitAfter(go);

        }
        return strand.SplitBefore(go);
    }

    public static void CreateStrand(List<GameObject> nucleotides, int strandId, Color color)
    {
        Strand strand = new Strand(nucleotides, strandId, color);
        strand.SetComponents();
        s_strandDict.Add(strandId, strand);
        s_numStrands++;
    }

    public static void MergeStrand(GameObject firstGO, GameObject secondGO, GameObject backbone)
    {
        var firstNtc = firstGO.GetComponent<NucleotideComponent>();
        var secondNtc = secondGO.GetComponent<NucleotideComponent>();
        var xoverComp = backbone.GetComponent<XoverComponent>();
        int firstStrandId = firstNtc.GetStrandId();
        int secondStrandId = secondNtc.GetStrandId();
        Strand firstStrand = s_strandDict[firstStrandId];
        Strand secondStrand = s_strandDict[secondStrandId];

        if (secondGO == null)
        {
            firstStrand.SetComponents();
            return;
        }

        if (firstStrand.GetHead() == firstGO)
        {
            // Must add backbone between 2 strands.
            firstStrand.AddToHead(backbone);
            xoverComp.SetPrevGO(firstGO);
            xoverComp.SetNextGO(secondGO);
            HandleCycle(firstStrand, secondStrand, true);
        }
        else if (firstStrand.GetTail() == firstGO)
        {
            // Must add backbone between 2 strands.
            firstStrand.AddToTail(backbone);
            xoverComp.SetPrevGO(secondGO);
            xoverComp.SetNextGO(firstGO);
            HandleCycle(firstStrand, secondStrand, false);
        }
        firstStrand.SetComponents();
        secondStrand.RemoveStrand();
    }

    public static void HandleCycle(Strand firstStrand, Strand secondStrand, bool addToHead)
    {
        // Handles cycles in strand.
        if (firstStrand.GetStrandId() != secondStrand.GetStrandId())
        {
            // Add second strand
            if (addToHead)
            {
                firstStrand.AddToHead(secondStrand.GetNucleotides());
            }
            else
            {
                firstStrand.AddToTail(secondStrand.GetNucleotides());
            }
        }
        else
        {
            firstStrand.HideCone();
        }
    }
}
