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
                    Highlight(s_startGO);
                }
                else
                {
                    s_endGO = s_hit.collider.gameObject;
                    Unhighlight(s_startGO);

                    if (s_drawTogOn)
                    {
                        CreateXover();
                        ResetNucleotides();
                    }
                }
            }
            else if (s_hit.collider.name.Equals("xover") && s_eraseTogOn)
            {
                EraseXover(s_hit.collider.gameObject);         
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
            ResetNucleotides();
        }
    }

    /// <summary>
    /// Resets the start and end nucleotides.
    /// </summary>
    public static void ResetNucleotides()
    {
        s_startGO = null;
        s_endGO = null;
    }

    public static void Highlight(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        ntc.Highlight();
    }

    public static void Unhighlight(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        ntc.Unhighlight();
    }

    /// <summary>
    /// Creates crossover and splits strands as necessary.
    /// </summary>
    public void CreateXover()
    {
        if (IsValid())
        {
            int strandId = s_startGO.GetComponent<NucleotideComponent>().GetStrandId();
            Strand startStrand = s_strandDict[strandId];
            int endStrandId = s_endGO.GetComponent<NucleotideComponent>().GetStrandId();
            Strand endStrand = s_strandDict[endStrandId];

            // Create crossover.
            DrawPoint d = new DrawPoint();
            GameObject xover = d.MakeXover(s_startGO, s_endGO, strandId);
            startStrand.AddXover(xover);

            // Handle strand splitting.
            List<GameObject> newStrand = SplitStrand(s_startGO, false);
            if (newStrand != null)
            {
                CreateStrand(newStrand, startStrand.GetColor());
            }

            newStrand = SplitStrand(s_endGO, true);
            if (newStrand != null)
            {
                CreateStrand(newStrand, endStrand.GetColor());
            }

            // Handle strand merging.
            MergeStrand(s_startGO, s_endGO, xover);
        }
    }

    public void EraseXover(GameObject xover)
    {
        var xoverComp = xover.GetComponent<XoverComponent>();
        GameObject nextGO = xoverComp.GetNextGO();
        List<GameObject> newStrand = SplitStrand(nextGO, true);
        if (newStrand != null)
        {
            CreateStrand(newStrand, nextGO.GetComponent<NucleotideComponent>().GetColor());
        }
        GameObject.Destroy(xover);
    }

    public bool IsValid()
    {
        var startNtc = s_startGO.GetComponent<NucleotideComponent>();
        int startDir = startNtc.GetDirection();
        int startHelix = startNtc.GetHelixId();

        var endNtc = s_endGO.GetComponent<NucleotideComponent>();
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

    public void CreateStrand(List<GameObject> nucleotides, Color color)
    {
        Strand strand = new Strand(nucleotides, s_numStrands, color);
        strand.SetComponents();
        s_strandDict.Add(s_numStrands, strand);
        s_numStrands++;
    }

    public void MergeStrand(GameObject firstGO, GameObject secondGO, GameObject backbone)
    {
        if (secondGO == null)
        {
            return;
        }

        var firstNtc = firstGO.GetComponent<NucleotideComponent>();
        var secondNtc = secondGO.GetComponent<NucleotideComponent>();
        var xoverComp = backbone.GetComponent<XoverComponent>();
        int firstStrandId = firstNtc.GetStrandId();
        int secondStrandId = secondNtc.GetStrandId();
        Strand firstStrand = s_strandDict[firstStrandId];
        Strand secondStrand = s_strandDict[secondStrandId];

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

    public void HandleCycle(Strand firstStrand, Strand secondStrand, bool addToHead)
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
