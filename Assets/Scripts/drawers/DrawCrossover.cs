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
/// Handles crossovers and respective strand operations.
/// </summary>
public class DrawCrossover : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] public XRRayInteractor rightRayInteractor;
    bool gripReleased = true;
  
    static GameObject s_startGO = null;
    static GameObject s_endGO = null;
    public static RaycastHit s_hit;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
        _device = _devices[0];
    }

    void OnEnable()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }
    }

    void Update()
    {
        if (!s_gridTogOn)
        {
            return;
        }

        if (!s_drawTogOn)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        // SELECT CROSSOVER NUCLEOTIDE
        bool gripValue;
        if (_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
            && gripValue
            && gripReleased
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            gripReleased = false;
            if (s_hit.collider.name.Contains("nucleotide"))
            {
                if (s_startGO == null)
                {
                    s_startGO = s_hit.collider.gameObject;
                }
                else
                {
                    s_endGO = s_hit.collider.gameObject;
                    if (s_drawTogOn)
                    {
                        CreateXover();
                    }
                    // ERASE XOVER
                }
            }
            else
            {
                ResetNucleotides();
            }
        }

        // Resets grips do avoid multiple selections.                                              
        if ((_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
                && !gripValue))
        {
            gripReleased = true;
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

    /// <summary>
    /// Creates crossover and splits strands as necessary.
    /// </summary>
    public void CreateXover()
    {
        if (IsValid())
        {
            int strandId = s_startGO.GetComponent<NucleotideComponent>().GetStrandId();
            Strand strand = s_strandDict[strandId];

            // Create crossover.
            GameObject xover = DrawPoint.MakeXover(s_startGO.transform.position, s_endGO.transform.position);
            strand.SetXover(xover);

            // Handle strand splitting.
            List<GameObject> newStrand = DrawSplit.SplitStrand(s_startGO);
            if (newStrand != null)
            {
                DrawSplit.CreateStrand(newStrand);
            }

            newStrand = DrawSplit.SplitStrand(s_endGO);
            if (newStrand != null)
            {
                DrawSplit.CreateStrand(newStrand);
            }

            // Handle strand merging.
            bool isHead = s_startGO == strand.GetHead();
            DrawMerge.MergeStrand(s_startGO, s_endGO, xover, isHead);
        }
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

}
