/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using RTG;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Attaches Gizmos to gameObject for movement and rotation.
/// </summary>
public class TransformHandle : MonoBehaviour
{
    [SerializeField] private XRNode _leftXRNode;
    [SerializeField] private XRNode _rightXRNode;
    [SerializeField] private GameObject gizmos;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _leftDevice;
    private InputDevice _rightDevice;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool leftGripReleased = true;
    private bool rightGripReleased = true;
    private static GameObject s_GO = null;
    private static RaycastHit s_hit;

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_leftXRNode, _devices);
        if (_devices.Count > 0)
        {
            _leftDevice = _devices[0];
        }

        InputDevices.GetDevicesAtXRNode(_rightXRNode, _devices);
        if (_devices.Count > 0)
        {
            _rightDevice = _devices[0];
        }
    }

    private void OnEnable()
    {
        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }
    }

    private void Start()
    {
        gizmos.SetActive(false);
    }

    void Update()
    {
        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }

        _leftDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool leftGripValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool rightGripValue);
        _leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue);

        if (leftGripValue && rightGripValue
                && leftGripReleased && rightGripReleased
                && (rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit) || leftRayInteractor.TryGetCurrent3DRaycastHit(out s_hit)))
        {
            leftGripReleased = false;
            rightGripReleased = false;
            if (s_hit.collider.gameObject.GetComponent<GridComponent>())
            {
                Debug.Log("Hitting GridComponent");
                s_GO = s_hit.collider.gameObject;
                ShowTransform();
                AttachChildren();
            }
        }

        if (leftTriggerValue || rightTriggerValue)
        {
            DetachChildren();
            HideTransform();
        }

        if (!leftGripValue)
        {
            leftGripReleased = true;
        }

        if (!rightGripValue)
        {
            rightGripReleased = true;
        }
    }

    /// <summary>
    /// Shows transform gizmo at the lower left corner of selected grid.
    /// </summary>
    private void ShowTransform()
    {
        Debug.Log("Creating Transform");
        gizmos.SetActive(true);
        GridComponent gc = s_GO.GetComponent<GridComponent>();
        GameObject bottomLeftCorner = gc.Grid.Grid2D[0, 0].gameObject;
        gizmos.transform.SetPositionAndRotation(bottomLeftCorner.transform.position + 0.2f * Vector3.back, bottomLeftCorner.transform.rotation);
        Debug.Log("Finished creating Transform");

        /*ObjectTransformGizmo objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
        List<GameObject> targets = GetTargets(go);
        objectMoveGizmo.SetTargetObjects(targets);
        objectMoveGizmo.SetTargetPivotObject(go); // CHECK THIS WORKS?*/
    }

    /// <summary>
    /// Hides transform gizmo.
    /// </summary>
    private void HideTransform()
    {
        gizmos.SetActive(false);
    }

    /*private List<GameObject> GetTargets(GameObject go)
    {
        List<GameObject> targets = new List<GameObject>();
        targets.Add(go);
        DNAGrid grid = go.GetComponent<GridComponent>().Grid;
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                if (!grid.Grid2D[i, j].Equals(go.GetComponent<GridComponent>()))
                {
                    Debug.Log("Attaching grid Component");
                    targets.Add(grid.Grid2D[i, j].gameObject);
                    Debug.Log("Attaching helix objects");
                    if (grid.Grid2D[i, j].Helix != null)
                    {
                        targets.AddRange(grid.Grid2D[i, j].Helix.BackbonesA);
                        targets.AddRange(grid.Grid2D[i, j].Helix.BackbonesB);
                        targets.AddRange(grid.Grid2D[i, j].Helix.NucleotidesA);
                        targets.AddRange(grid.Grid2D[i, j].Helix.NucleotidesB);
                    }
                    //Debug.Log("Finished component: " + i + ", " + j);
                }
            }
        }
        return targets;
        //Debug.Log("Finished attaching children");
    }*/


    private void AttachChildren()
    {
        DNAGrid grid = s_GO.GetComponent<GridComponent>().Grid;
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                grid.Grid2D[i, j].gameObject.transform.parent = gizmos.transform;
                if (grid.Grid2D[i, j].Helix != null)
                {
                    grid.Grid2D[i, j].Helix.SetParent(gizmos);
                }
            }
        }
    }

    private void DetachChildren()
    {
        DNAGrid grid = s_GO.GetComponent<GridComponent>().Grid;
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                grid.Grid2D[i, j].gameObject.transform.parent = null;
                if (grid.Grid2D[i, j].Helix != null)
                {
                    grid.Grid2D[i, j].Helix.ResetParent();
                }
            }
        }
    }
}