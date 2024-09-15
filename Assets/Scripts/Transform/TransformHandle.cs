/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Attaches Gizmos to GameObjects for movement and rotation.
/// </summary>
public class TransformHandle : MonoBehaviour
{
    [SerializeField] private XRNode _leftXRNode;
    [SerializeField] private XRNode _rightXRNode;
    public GameObject gizmos;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _leftDevice;
    private InputDevice _rightDevice;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool leftGripReleased = true;
    private bool rightGripReleased = true;
    private static GameObject s_GO = null;
    private static RaycastHit s_hit;

    public static TransformHandle Instance;

    private void Awake()
    {
        Instance = this;
    }

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
                //Debug.Log("Hitting GridComponent");
                s_GO = s_hit.collider.gameObject;
                AttachChildren(s_GO.GetComponent<GridComponent>().Grid);
                ShowTransform();
            }
        }

        if ((leftTriggerValue || rightTriggerValue) && gizmos.activeSelf)
        {
            DetachChildren(s_GO.GetComponent<GridComponent>().Grid);
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
        gizmos.SetActive(true);
    }

    /// <summary>
    /// Hides transform gizmo.
    /// </summary>
    private void HideTransform()
    {
        gizmos.SetActive(false);
    }


    public void AttachChildren(DNAGrid grid)
    {
        // Position gizmos correctly
        int minXIndex = grid.GridXToIndex(grid.MinimumBound.X);
        int minYIndex = grid.GridYToIndex(grid.MinimumBound.Y);
        Transform transform = grid.Grid2D[minXIndex, minYIndex].transform;
        gizmos.transform.SetPositionAndRotation(transform.position - 0.2f * transform.forward, transform.rotation);

        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                grid.Grid2D[i, j].gameObject.transform.SetParent(gizmos.transform);
                grid.Grid2D[i, j].Helix?.SetParent(gizmos);
            }
        }
    }

    public void DetachChildren(DNAGrid grid)
    {
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                grid.Grid2D[i, j].gameObject.transform.parent = null;
                grid.Grid2D[i, j].Helix?.ResetParent();
            }
        }
    }
}