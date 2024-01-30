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

    private bool triggerReleased = true;
    private static GameObject s_GO = null;
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
        if (triggerValue && rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            rayInteractor.TryGetHitInfo(out Vector3 reticlePosition, out _, out _, out _);
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
            if (s_hit.collider.gameObject.Equals(originTransform))
            {
                TransformOrigin(reticlePosition);
            }
        }
    }


    // TODO: Test these work
    private void TransformX(Vector3 reticalPos)
    {
        Vector3 distance = GetDistance(reticalPos);
        gizmos.transform.position -= distance;
        gizmos.transform.position = new Vector3(reticalPos.x, gizmos.transform.position.y, gizmos.transform.position.z);
    }

    private void TransformY(Vector3 reticalPos)
    {
        Vector3 distance = GetDistance(reticalPos);
        gizmos.transform.position -= distance;
        gizmos.transform.position = new Vector3(gizmos.transform.position.x, reticalPos.y, gizmos.transform.position.z);
    }

    private void TransformZ(Vector3 reticalPos)
    {
        Vector3 distance = GetDistance(reticalPos);
        gizmos.transform.position -= distance;
        gizmos.transform.position = new Vector3(gizmos.transform.position.x, gizmos.transform.position.y, reticalPos.z);
    }

    private void TransformOrigin(Vector3 reticalPos)
    {
        Vector3 distance = GetDistance(reticalPos);
        gizmos.transform.position -= distance;
        gizmos.transform.position = reticalPos;
    }

    private Vector3 GetDistance(Vector3 reticalPos)
    {
        return gizmos.transform.position - reticalPos;
    }
}
