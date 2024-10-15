/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

/// <summary>
/// Attaches Gizmos to GameObjects for movement and rotation.
/// </summary>
public class TransformHandle : MonoBehaviour
{
    [SerializeField] private XRNode _leftXRNode;
    [SerializeField] private XRNode _rightXRNode;
    private GameObject gizmos = null;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _leftDevice;
    private InputDevice _rightDevice;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool leftGripReleased = true;
    private bool rightGripReleased = true;
    private bool leftTriggerReleased = true;
    private bool rightTriggerReleased = true;
    private static GameObject s_GO = null;
    private static RaycastHit s_hit;

    public static TransformHandle Instance;

    /*private void Awake()
    {
        Instance = this;
    }*/

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
            var gc = s_hit.collider.gameObject.GetComponent<GridComponent>();
            if (gc)
            {
                //Debug.Log("Hitting GridComponent");
                //ShowTransform();
                AttachChildren(gc.Grid);
            }
        }
        
        if ((leftTriggerValue || rightTriggerValue) && leftTriggerReleased && rightTriggerReleased && gizmos != null)
        {
            leftTriggerReleased = false;
            rightTriggerReleased = false;

            Debug.Log("Detach children");
            DetachChildren();

            Debug.Log("done hiding transform");
        }

        if (!leftGripValue)
        {
            leftGripReleased = true;
        }

        if (!rightGripValue)
        {
            rightGripReleased = true;
        }

        if (!leftTriggerValue)
        {
            leftTriggerReleased = true;
        }

        if (!rightTriggerValue)
        {
            rightTriggerReleased = true;
        }
    }

    /// <summary>
    /// Shows transform gizmo at the lower left corner of selected grid.
    /// </summary>
    private void ShowTransform()
    {
        if (gizmos == null)
        {
            gizmos = Instantiate(GlobalVariables.Gizmos);
        }
    }

    /// <summary>
    /// Hides transform gizmo.
    /// </summary>
    private void HideTransform()
    {
        if (gizmos != null)
        {
            gizmos.SetActive(false);
            GameObject.Destroy(gizmos);
        }
        gizmos = null;
    }


    public void AttachChildren(DNAGrid grid)
    {
        ShowTransform();

        // Position gizmos correctly
        int minXIndex = grid.GridXToIndex(grid.MinimumBound.X);
        int minYIndex = grid.GridYToIndex(grid.MinimumBound.Y);
        Transform transform = grid.Grid2D[minXIndex, minYIndex].transform;
        Transform gizmosTransform = gizmos.transform;
        gizmosTransform.SetPositionAndRotation(transform.position - 0.2f * transform.forward, transform.rotation);

        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                grid.Grid2D[i, j].transform.parent = (gizmosTransform);
                grid.Grid2D[i, j].Helix?.SetParent(gizmosTransform);
            }
        }
    }

    public void DetachChildren()
    {
        if (gizmos != null)
        {
            Debug.Log("Num children: " + gizmos.transform.childCount);
            //int n = gizmos.transform.childCount;
            foreach (Transform child in gizmos.transform)
            {
                //Transform child = gizmos.transform.GetChild(0); // Children list dynamically shifts as children are removed, so just keep removing the element at idx 0
                child.parent = null;
            }
        }

        HideTransform();

    }
}