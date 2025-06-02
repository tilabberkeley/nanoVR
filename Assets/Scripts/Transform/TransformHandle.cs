/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System.Linq;
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
    private static GameObject gizmos;
    private static Transform gizmosTransform;
    public static GameObject Gizmos { get { return gizmos; } }
    public static Transform GizmosTransform { get { return gizmosTransform; } }

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

    // These variables record the initial state when the transformation begins.
    private static Matrix4x4 initialGizmoMatrix = Matrix4x4.identity;
    public static Matrix4x4 InitialGizmoMatrix { get { return initialGizmoMatrix; } }

    private void Awake()
    {
        //Instance = this;
        gizmos = Instantiate(GlobalVariables.Gizmos);
        gizmosTransform = gizmos.transform;
        gizmos.SetActive(false);
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

        if ((leftTriggerValue || rightTriggerValue) && leftTriggerReleased && rightTriggerReleased && gizmos.activeSelf == true)
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
        //Debug.Log("Show transform");
        gizmos.SetActive(true);
        Transform transform = grid.GetTransform();
        Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
        gizmosTransform.SetPositionAndRotation(position, transform.rotation);

        initialGizmoMatrix = Matrix4x4.TRS(
                    gizmosTransform.position,
                    gizmosTransform.rotation,
                    Vector3.one);
    }

    /// <summary>
    /// Hides transform gizmo.
    /// </summary>
    private static void HideTransform()
    {
        //Debug.Log("Hide transform");
        gizmos.SetActive(false);
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
        /*for (int i = 0; i < grid.Length; i++)
        {            
            for (int j = 0; j < grid.Width; j++)
            {
                grid.Grid2D[i, j].transform.SetParent(gizmosTransform, true);
                grid.Grid2D[i, j].GetComponent<Collider>().enabled = false;
            }
        }*/
        grid.IsTransforming = true;
        foreach (GridComponent gc in grid.GridComponents)
        {
            gc.transform.SetParent(gizmosTransform, true);
            //gc.GetComponent<Collider>().enabled = false;
        }
    }

    public static void DetachChildren()
    {   
        //Debug.Log("Num children: " + gizmos.transform.childCount);
        //int n = gizmos.transform.childCount;

        Matrix4x4 gizmosMatrix = Matrix4x4.TRS(
                                    gizmosTransform.position,
                                    gizmosTransform.rotation,
                                    Vector3.one);
        Matrix4x4 delta = gizmosMatrix * initialGizmoMatrix.inverse;
        foreach (DNAGrid grid in translatedGrids)
        {
            grid.IsTransforming = false;
            grid.OldTransformOffset = delta * grid.OldTransformOffset;

            foreach (GridComponent gc in grid.GridComponents)
            {
                gc.transform.SetParent(null);
                //gc.GetComponent<Collider>().enabled = true;

                // Update helix bounding box
                Helix helix = gc.Helix;
                if (helix != null)
                {
                    helix.BoundingBox.Extend(helix.NucleotideDataA[0].GetPosition());
                    helix.BoundingBox.Extend(helix.NucleotideDataB[0].GetPosition());
                    helix.BoundingBox.Extend(helix.NucleotideDataA.Last().GetPosition());
                    helix.BoundingBox.Extend(helix.NucleotideDataB.Last().GetPosition());
                }
            }
        }
        //translatedGrids.Clear();
        initialGizmoMatrix = Matrix4x4.identity;
        HideTransform();
    }
}