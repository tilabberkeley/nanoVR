/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

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
    // public static GameObject s_crossover;



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
        if (!GlobalVariables.s_gridTogOn)
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

    public void CreateXover()
    {
        if (isValid())
        {
            // Create crossover

            // Handle strand splitting
        }
    }

    public bool isValid()
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
