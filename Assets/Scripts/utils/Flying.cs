using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Flying : MonoBehaviour
{
    [SerializeField]
    private XRNode _xrNode = XRNode.LeftHand;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    private float _speed = 0.008f;

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
        if (_device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out primary2DAxis))
        {
            if (Math.Abs(primary2DAxis.y) > 0)
            {
                transform.position += new Vector3(0, _speed, 0);
            }
        }
    }
}
