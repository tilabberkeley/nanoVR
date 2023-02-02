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
    private float _speed = 0.005f;

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
        bool secondaryValue;
        if (_device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondaryValue) 
                && secondaryValue) 
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + _speed, transform.position.z);
        } 

        bool primaryValue;
        if (_device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryValue) 
                && primaryValue) 
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - _speed, transform.position.z);
        }
    }
}
