/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Flying : MonoBehaviour
{
    [SerializeField]
    private XRNode _xrNode = XRNode.RightHand;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    private static float s_speed = 0.008f;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
        _device = _devices[0];
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
        Vector2 primary2DAxis;
        if (_device.TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxis))
        {
            if (primary2DAxis.y > 0)
            {
                transform.position += new Vector3(0, s_speed, 0);
            }
            if (primary2DAxis.y < 0)
            {
                transform.position -= new Vector3(0, s_speed, 0);
            }
        }
    }
}
