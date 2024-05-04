/*
 * nanoVR, a VR application for DNA nanostructures.
 * authors: David Yang <davidmyang@berkeley.edu and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using static GlobalVariables;

public class UpdateManager : MonoBehaviour
{
    // Input controller variables
    [SerializeField] private XRNode _xrNode;
    [SerializeField] private XRRayInteractor rayInteractor;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    private bool triggerReleased = true;

    // Helper variables
    private static GameObject s_startGO = null;
    private static GameObject s_endGO = null;
    private static GameObject s_GO = null;
    private static RaycastHit s_hit;
    private static List<GameObject> s_currentNucleotides;
    bool creatingStrand = false;

    // Draw insertion default length
    private const int INSERTION_LENGTH = 1;

    // Helix default length;
    private const int HELIX_LENGTH = 64;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
        if (_devices.Count > 0)
        {
            _device = _devices[0];
        }
    }

    void OnEnable()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }

        if (s_hideStencils || s_visualMode)
        {
            return;
        }

        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);
        _device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue);
        bool hit = rayInteractor.TryGetCurrent3DRaycastHit(out s_hit);


        // Handles drawing helix in DrawGrid
        if (triggerReleased && triggerValue && hit)
        {
            triggerReleased = false;

            // clicking grid circle
            GridComponent gc = s_hit.collider.GetComponent<GridComponent>();
            if (gc != null)
            {
                Vector3 startPos = s_hit.collider.bounds.center;
                int id = s_numHelices;
                DrawGrid.CreateHelix(id, startPos, HELIX_LENGTH, gc.Grid.Plane, gc);
            }
        }

        // Handles update for DrawSplit
        if (s_splitTogOn)
        {
            if (triggerValue && triggerReleased && hit)
            {
                triggerReleased = false;
                if (s_hit.collider.GetComponent<NucleotideComponent>() != null)
                {
                    s_GO = s_hit.collider.gameObject;
                    DrawSplit.DoSplitStrand(s_GO);
                }
            }
        }

        // Handles update for DrawMerge
        if (s_mergeTogOn)
        {
            if (triggerValue && triggerReleased && hit)
            {
                triggerReleased = false;
                if (s_hit.collider.GetComponent<NucleotideComponent>() != null)
                {
                    s_GO = s_hit.collider.gameObject;
                    DrawMerge.DoMergeStrand(s_GO);
                }
            }
        }


        // Handles update for DrawInsertion
        if (s_insTogOn)
        {
            if (triggerValue && triggerReleased && hit)
            {
                triggerReleased = false;
                if (s_hit.collider.GetComponent<NucleotideComponent>() != null)
                {
                    s_GO = s_hit.collider.gameObject;
                    DrawInsertion.DoInsertion(s_GO, INSERTION_LENGTH);
                }
            }
        }

        // Handles update for DrawDeletion
        if (s_delTogOn)
        {
            if (triggerValue && triggerReleased && hit)
            {
                triggerReleased = false;
                NucleotideComponent comp = s_hit.collider.gameObject.GetComponent<NucleotideComponent>();
                if (comp != null)
                {
                    s_GO = s_hit.collider.gameObject;
                    DrawDeletion.DoDeletion(s_GO);
                }
            }
        }









        // Resets triggers to avoid multiple selections.                                              
        if (!triggerValue)
        {
            triggerReleased = true;
        }
    }
}
