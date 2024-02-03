/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
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
                AttachChildren(s_GO);
                CreateTransform(s_GO);
            }
        }

        if (leftGripValue && rightGripValue
               && leftGripReleased && rightGripReleased
               && (!rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit) && !leftRayInteractor.TryGetCurrent3DRaycastHit(out s_hit)))
        {
            leftGripReleased = false;
            rightGripReleased = false;
            DetachChildren(s_GO);
            HideTransform(s_GO);
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

    private void CreateTransform(GameObject go)
    {
        Debug.Log("Creating Transform");
        gizmos.SetActive(true);
        gizmos.transform.SetPositionAndRotation(go.transform.position + 0.5f * Vector3.back, go.transform.rotation);
        go.transform.parent = gizmos.transform; 
        Debug.Log("Finished creating Transform");
    }

    private void HideTransform(GameObject go)
    {
        gizmos.SetActive(false);
        go.transform.parent = null;
    }

    private void AttachChildren(GameObject go)
    {
        Debug.Log("Attaching Children");
        DNAGrid grid = go.GetComponent<GridComponent>().Grid;
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                if (!grid.Grid2D[i, j].Equals(go.GetComponent<GridComponent>()))
                {
                    Debug.Log("Attaching grid Component");
                    grid.Grid2D[i, j].gameObject.transform.parent = go.transform;
                    Debug.Log("Attaching helix objects");
                    if (grid.Grid2D[i, j].Helix != null)
                    {
                        grid.Grid2D[i, j].Helix.SetParent(go);
                    }
                    Debug.Log("Finished component: " + i + ", " + j);
                }
            }
        }
        Debug.Log("Finished attaching children");
    }

    private void DetachChildren(GameObject go)
    {
        DNAGrid grid = go.GetComponent<GridComponent>().Grid;
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