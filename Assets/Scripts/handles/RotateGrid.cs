/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Rotates Gizmos.
/// </summary>
public class RotateGrid : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private GameObject gizmos;
    [SerializeField] private GameObject xRotate;
    [SerializeField] private GameObject yRotate;
    [SerializeField] private GameObject zRotate;

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
            if (s_hit.collider.gameObject.Equals(xRotate))
            {
                RotateX(reticlePosition);
            }
            if (s_hit.collider.gameObject.Equals(xRotate))
            {
                RotateY(reticlePosition);
            }
            if (s_hit.collider.gameObject.Equals(xRotate))
            {
                RotateZ(reticlePosition);
            }
        }
    }


    // TODO: Test these work
    private void RotateX(Vector3 reticalPos)
    {
        Vector3 distance = GetDistance(reticalPos);
        gizmos.transform.position -= distance;
        gizmos.transform.position = new Vector3(reticalPos.x, gizmos.transform.position.y, gizmos.transform.position.z);
    }

    private void RotateY(Vector3 reticalPos)
    {
        Vector3 distance = GetDistance(reticalPos);
        gizmos.transform.position -= distance;
        gizmos.transform.position = new Vector3(gizmos.transform.position.x, reticalPos.y, gizmos.transform.position.z);
    }

    private void RotateZ(Vector3 reticalPos)
    {
        Vector3 distance = GetDistance(reticalPos);
        gizmos.transform.position -= distance;
        gizmos.transform.position = new Vector3(gizmos.transform.position.x, gizmos.transform.position.y, reticalPos.z);
    }

    private Vector3 GetDistance(Vector3 reticalPos)
    {
        return gizmos.transform.position - reticalPos;
    }
}
