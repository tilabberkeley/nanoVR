/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static GlobalVariables;

/// <summary>
/// Handles Teleporting. When camera toggle is on, the user can use the right trigger to set their position as the camera (will create red cube in game). 
/// This is their saved position where they can then teleport to with both triggers when camera toggle is off. The next time both triggers are 
/// pressed (when camera toggle is off), the user is teleported back to the saved position.
/// </summary>
public class Teleport : MonoBehaviour
{
    [SerializeField] private XRNode _leftXRNode;
    [SerializeField] private XRNode _rightXRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice leftDevice;
    private InputDevice rightDevice;
    private bool leftTriggerReleased = true;
    private bool rightTriggerReleased = true;
    Vector3? lastPosition = null;
    Vector3? cameraPosition = null;
    private GameObject camera = null;
    //not implemented yet
    List<Vector3> cameraPositions = new List<Vector3>();

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_leftXRNode, _devices);
        if (_devices.Count > 0)
        {
            leftDevice = _devices[0];
        }

        InputDevices.GetDevicesAtXRNode(_rightXRNode, _devices);
        if (_devices.Count > 0)
        {
            rightDevice = _devices[0];
        }
    }

    private void OnEnable()
    {
        if (!leftDevice.isValid || !rightDevice.isValid)
        {
            GetDevice();
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        if (!leftDevice.isValid || !rightDevice.isValid)
        {
            GetDevice();
        }

        leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerValue);
        rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue);

        if (s_cameraTogOn)
        {
            // Setting camera position. Only when camera toggle is enabled.
            if (rightTriggerValue && rightTriggerReleased)
            {
                rightTriggerReleased = false;
                cameraPosition = transform.position;
                if (camera == null)
                {
                    //create camera
                    camera = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    camera.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                    camera.transform.position = transform.position;
                }
                else
                {
                    //move camera to new position
                    camera.transform.position = transform.position;
                }

            }
        }
        else
        {
            // Teleporting. Only when camera toggle is not enabled
            if (leftTriggerValue && rightTriggerValue && leftTriggerReleased && rightTriggerReleased)
            {
                leftTriggerReleased = false;
                rightTriggerReleased = false;
                if (cameraPosition != null)
                {
                    if (lastPosition == null)
                    {
                        lastPosition = transform.position;
                        transform.position = (Vector3)cameraPosition;
                    }
                    else
                    {
                        transform.position = (Vector3)lastPosition;
                        lastPosition = null;
                    }
                }

            }
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
}
