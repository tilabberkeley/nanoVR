/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Transforms Gizmos.
/// </summary>
public class TransformGrid : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private GameObject gizmos;
    [SerializeField] private GameObject xTransform;
    [SerializeField] private GameObject yTransform;
    [SerializeField] private GameObject zTransform;
    [SerializeField] private GameObject originTransform;
    private Vector3 distanceFromRay = Vector3.zero;
    private Vector3 normalDistance = Vector3.zero;
    private Vector3 distanceToGrid = Vector3.zero;
    private static RaycastHit s_hit;

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

    void Update()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }

        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);
        if (triggerValue && rayInteractor.TryGetCurrent3DRaycastHit(out s_hit) && s_hit.collider.gameObject.Equals(gizmos))
        {
            rayInteractor.TryGetHitInfo(out Vector3 reticlePosition, out _, out _, out _);
            if (distanceFromRay == Vector3.zero)
            {
                distanceFromRay = reticlePosition - gizmos.transform.position;
            }
            if (distanceToGrid == Vector3.zero)
            {
                distanceToGrid = gizmos.transform.position - gizmos.GetComponentInChildren<GridComponent>().Grid.StartPos;
            }

            if (s_hit.collider.gameObject.Equals(xTransform))
            {
                TransformX(reticlePosition);
            }
            if (s_hit.collider.gameObject.Equals(yTransform))
            {
                TransformY(reticlePosition);
            }
            if (s_hit.collider.gameObject.Equals(zTransform))
            {
                TransformZ(reticlePosition);
            }
/*            if (s_hit.collider.gameObject.Equals(originTransform))
            {
                TransformOrigin(reticlePosition);
            }*/
        }

        // When trigger is released
        if (!triggerValue && distanceToGrid != Vector3.zero)
        {
            var gc = gizmos.GetComponentInChildren<GridComponent>();
            if (gc != null)
            {
                gc.Grid.StartPos = gizmos.transform.position - distanceToGrid;
            }
            distanceFromRay = Vector3.zero;
            distanceToGrid = Vector3.zero;
        }
    }


    // TODO: Test these work
    private void TransformX(Vector3 reticalPos)
    {
        float magnitude = GetDistance(reticalPos, gizmos.transform.position);
        gizmos.transform.position += magnitude * gizmos.transform.right - distanceFromRay;
    }

    private void TransformY(Vector3 reticalPos)
    {
        float magnitude = GetDistance(reticalPos, gizmos.transform.position);
        gizmos.transform.position += magnitude * gizmos.transform.up - distanceFromRay;
    }

    private void TransformZ(Vector3 reticalPos)
    {
        float magnitude = GetDistance(reticalPos, gizmos.transform.position);
        gizmos.transform.position += magnitude * gizmos.transform.forward - distanceFromRay;
    }

    private void TransformOrigin(Vector3 reticalPos)
    {
        gizmos.transform.position = reticalPos;
    }

    private float GetDistance(Vector3 v1, Vector3 v2)
    {
        return Vector3.Distance(v1, v2);
    }
}
