using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class IntersectPoint : MonoBehaviour
{
    [SerializeField] private XRNode xrNode;
    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;
    [SerializeField] public static XRRayInteractor rightRayInteractor;
    public static Vector3 pos = new Vector3();
    public static Vector3 norm = new Vector3();
    public static int index = 0;
    public static bool validTarget = false;

    void GetDevice() {
        InputDevices.GetDevicesAtXRNode(xrNode, devices);
        device = devices[0];
       
    }


    void OnEnable() {
        if (!device.isValid) 
        {
            GetDevice();
        }
    }

    public static bool isIntersecting() {
        return rightRayInteractor.TryGetHitInfo(out pos, out norm, out index, out validTarget);
    }
    public Vector3 FindIntersectionPoint() {
        return new Vector3(0, 0, 0);
    }

}
