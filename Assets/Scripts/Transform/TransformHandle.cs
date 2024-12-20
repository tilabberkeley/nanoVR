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
    private static GameObject gizmos = null;
    public static GameObject Gizmos { get { return gizmos; } }
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
    private static List<DNAGrid> translatedGrids = new List<DNAGrid>();
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
                && leftGripReleased && rightGripReleased)
        {
            leftGripReleased = false;
            rightGripReleased = false;

            //Debug.Log("Hitting GridComponent");
            translatedGrids = SelectGrid.Grids;
            if (translatedGrids.Count == 0)
            {
                Debug.Log("Please select at least one grid to translate/rotate.");
            }
            else
            {
                ShowTransform(translatedGrids[0]);
                AttachChildren(translatedGrids);
            }
        }
        
        if ((leftTriggerValue || rightTriggerValue) && leftTriggerReleased && rightTriggerReleased && gizmos != null)
        {
            leftTriggerReleased = false;
            rightTriggerReleased = false;

            //Debug.Log("Detach children");
            DetachChildren();

            //Debug.Log("done hiding transform");
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
    public static void ShowTransform(DNAGrid grid)
    {
        if (gizmos == null)
        {
            gizmos = Instantiate(GlobalVariables.Gizmos);
            Transform gizmosTransform = gizmos.transform;
            int minXIndex = grid.GridXToIndex(grid.MinimumBound.X);
            int minYIndex = grid.GridYToIndex(grid.MinimumBound.Y);
            Transform transform = grid.Grid2D[minXIndex, minYIndex].transform;
            Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
            gizmosTransform.SetPositionAndRotation(position, transform.rotation);
        }
    }

    /// <summary>
    /// Hides transform gizmo.
    /// </summary>
    private static void HideTransform()
    {
        if (gizmos != null)
        {
            gizmos.SetActive(false);
            GameObject.Destroy(gizmos);
        }
        gizmos = null;
    }

    public static void AttachChildren(List<DNAGrid> grids)
    {
        foreach (DNAGrid grid in grids)
        {
            AttachChildren(grid);
        }
    }

    private static void AttachChildren(DNAGrid grid)
    {
        //ShowTransform();
        //translatedGrids.Add(grid);
        if (gizmos == null)
        {
            return;
        }

        Transform gizmosTransform = gizmos.transform;

        // Position gizmos correctly
        /*if (translatedGrids.Count == 1)
        {
            int minXIndex = grid.GridXToIndex(grid.MinimumBound.X);
            int minYIndex = grid.GridYToIndex(grid.MinimumBound.Y);
            Transform transform = grid.Grid2D[minXIndex, minYIndex].transform;
            gizmosTransform.SetPositionAndRotation(transform.position - 0.2f * transform.forward, transform.rotation);
        }*/
        

        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                grid.Grid2D[i, j].transform.SetParent(gizmosTransform, true);
                grid.Grid2D[i, j].GetComponent<Collider>().enabled = false;
                grid.Grid2D[i, j].Helix?.SetParent(gizmosTransform);
            }
        }
    }

    public static void DetachChildren()
    {
        if (gizmos != null)
        {
            //Debug.Log("Num children: " + gizmos.transform.childCount);
            //int n = gizmos.transform.childCount;
            foreach (DNAGrid grid in translatedGrids)
            {
                for (int i = 0; i < grid.Length; i++)
                {
                    for (int j = 0; j < grid.Width; j++)
                    {
                        grid.Grid2D[i, j].transform.SetParent(null);
                        grid.Grid2D[i, j].GetComponent<Collider>().enabled = true;
                        grid.Grid2D[i, j].Helix?.SetParent(null);
                    }
                }
            }
        }
        //translatedGrids.Clear();
        HideTransform();
    }
}