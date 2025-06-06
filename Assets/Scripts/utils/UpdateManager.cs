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
    private bool gripReleased = true;

    // Helper variables
    //private static GameObject s_startGO = null;
    //private static GameObject s_endGO = null;
    //private static GameObject s_GO = null;
    private static RaycastHit s_hit;
    //private static List<GameObject> s_currentNucleotides;
    //bool creatingStrand = false;

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

    // Update is called once per frame
    void Update()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }

        /*if (s_hideStencils || s_visualMode)
        {
            return;
        }*/

        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);
        _device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue);
        bool hit = rayInteractor.TryGetCurrent3DRaycastHit(out s_hit);

        // Handles update for DrawSplit
        if (s_splitTogOn)
        {
            //Debug.Log("Split on");

            if (triggerValue && triggerReleased && hit)
            {
                //Debug.Log("Split hit");

                triggerReleased = false;
                if (s_hit.collider.TryGetComponent<NucleotideColliderComponent>(out var comp))
                {
                    //s_GO = s_hit.collider.gameObject;
                    //DrawSplit.DoSplitStrand(s_GO);
                    DrawSplit.SplitStrand(comp.Data);
                }
            }
        }

        // Handles update for DrawMerge
        if (s_mergeTogOn)
        {
            //Debug.Log("Merge on");

            if (triggerValue && triggerReleased && hit)
            {
                //Debug.Log("Merge hit");

                triggerReleased = false;
                if (s_hit.collider.TryGetComponent<NucleotideColliderComponent>(out var comp))
                {
                    //s_GO = s_hit.collider.gameObject;
                    //DrawMerge.DoMergeStrand(s_GO);

                    DrawMerge.MergeStrand(comp.Data);
                }
            }
        }


        // Handles update for DrawInsertion
        if (s_insTogOn) {

            //Debug.Log("Insertion on");

            if (triggerValue && triggerReleased && hit)
            {
                //Debug.Log("Insertion hit");

                triggerReleased = false;
                if (s_hit.collider.TryGetComponent<NucleotideColliderComponent>(out var comp))
                {
                    //s_GO = s_hit.collider.gameObject;
                    //DrawInsertion.DoInsertion(s_GO, INSERTION_LENGTH);
                    DrawInsertion.Insertion(comp.Data, INSERTION_LENGTH);
                }
            }
        }

        // Handles update for DrawDeletion
        if (s_delTogOn)
        {
            //Debug.Log("Deletion on");
            if (triggerValue && triggerReleased && hit)
            {
                //Debug.Log("Deletion hit");
                triggerReleased = false;
                if (s_hit.collider.TryGetComponent<NucleotideColliderComponent>(out var comp))
                {
                    //s_GO = s_hit.collider.gameObject;
                    // DrawDeletion.DoDeletion(s_GO);
                    DrawDeletion.Deletion(comp.Data);
                }
            }
        }


        if (triggerReleased && triggerValue && hit)
        {
            if (s_hit.collider.TryGetComponent<HelixComponent>(out var hc))
            {
                triggerReleased = false;
                Debug.Log("Hit helix cylinder");
                hc.Helix.ToNucleotideView();
            }
            else if (s_hit.collider.TryGetComponent<GridComponent>(out var gc))
            {
                triggerReleased = false;
                Vector3 startPos = s_hit.collider.bounds.center;
                int id = s_numHelices;
                DrawGrid.CreateHelix(id, startPos, HELIX_LENGTH, gc.Grid.Plane, gc);
            }
        }

        if (gripReleased && gripValue && triggerReleased && triggerValue && hit)
        {
            gripReleased = false;
            triggerReleased = false;
            if (s_hit.collider.TryGetComponent<NucleotideColliderComponent>(out var nc))
            {
                nc.Data.GetHelix().ToHelixView();
            }
        }


        // Resets triggers to avoid multiple selections.                                              
        if (!triggerValue)
        {
            triggerReleased = true;
        }

        if (!gripValue)
        {
            gripReleased = true;
        }
    }
}
