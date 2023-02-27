/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

/// <summary>
/// Handles all the operations for creating and interacting with grid objects.
/// </summary>
public class DrawGrid : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] public XRRayInteractor rightRayInteractor;
    bool triggerReleased = true;
    bool gripReleased = true;
    public static RaycastHit s_hit;
    public Dropdown dropdown;
    string plane;
    private Grid _grid;
    bool gridExists = false;

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
        /*var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);

        if (leftHandDevices.Count == 1)
        {
            UnityEngine.XR.InputDevice device = leftHandDevices[0];
        }*/
    }

    void Update()
    {
        if (!s_gridTogOn)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool triggerValue;
        if (triggerReleased
            && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue
            && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit)
            && !gridExists)
        {
            triggerReleased = false;
            gridExists = true;
            plane = dropdown.options[dropdown.value].text;
            Vector3 direction = transform.rotation * Vector3.forward;
            Vector3 currPoint = transform.position + direction * 0.07f;
            _grid = new Grid(plane, currPoint);
            s_gridList.Add(_grid);
        }
        else if (triggerReleased
            && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit)
            && gridExists)
        {
            triggerReleased = false;

            // clicking grid sphere
            if (s_hit.collider.name.Equals("grid"))
            {
                Vector3 midpoint = s_hit.collider.bounds.center;
                Vector3 startPos = CalculateStartPoint(midpoint);
                Vector3 endPos = CalculateEndPoint(midpoint);
                int id = _grid.GetLines().Count;
                _grid.AddLine(id, startPos, endPos);
                _grid.AddHelix(id, startPos, endPos, plane, midpoint);
            }

        }
        else if (!(_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue))
        {
            triggerReleased = true;
        }

        bool gripValue;
        if (_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
            && gripValue
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            if (s_hit.collider.name.Contains("startPoint") || s_hit.collider.name.Contains("endPoint"))
            {
                //gripReleased = false;
                Vector3 spherePos = s_hit.collider.bounds.center;
                Vector3 direction = transform.rotation * Vector3.forward;
                Vector3 rayPos = transform.position + direction * 0.07f;
                Vector3 newPos;
                if (plane.Equals("XY"))
                {
                    newPos = new Vector3(spherePos.x, spherePos.y, rayPos.z);
                }
                else if (plane.Equals("YZ"))
                {
                    newPos = new Vector3(rayPos.x, spherePos.y, spherePos.z);
                }
                else
                {
                    newPos = new Vector3(spherePos.x, rayPos.y, spherePos.z);
                }

                s_hit.collider.gameObject.transform.position = Vector3.MoveTowards(spherePos, newPos, 50f);
                if (s_hit.collider.name.Contains("startPoint"))
                {
                    int index = Int32.Parse(s_hit.collider.name.Substring(s_hit.collider.name.Length - 1));
                    Line line = _grid.GetLine(index);
                    line.SetStart(newPos);
                }
                else if (s_hit.collider.name.Contains("endPoint"))
                {
                    int index = Int32.Parse(s_hit.collider.name.Substring(s_hit.collider.name.Length - 1));
                    Line line = _grid.GetLine(index);
                    line.SetEnd(newPos);
                }
            }
        }

        bool primaryValue;
        if ((_device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue) && primaryValue))
        {
            _grid.ShowHelices();
        }

        bool secondaryValue;
        if ((_device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryValue) && secondaryValue))
        {
            _grid.ShowLines();
        }

        if (!(_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue) && gripValue))
        {
            gripReleased = true;
        }

    }

    public Vector3 CalculateStartPoint(Vector3 midpoint)
    {
        if (plane.Equals("XY"))
        {
            return new Vector3(midpoint.x, midpoint.y, midpoint.z - 1);
        }
        else if (plane.Equals("YZ"))
        {
            return new Vector3(midpoint.x - 1, midpoint.y, midpoint.z);
        }
        else
        {
            return new Vector3(midpoint.x, midpoint.y - 1, midpoint.z);
        }
    }

    public Vector3 CalculateEndPoint(Vector3 midpoint)
    {
        if (plane.Equals("XY"))
        {
            return new Vector3(midpoint.x, midpoint.y, midpoint.z + 1);
        }
        else if (plane.Equals("YZ"))
        {
            return new Vector3(midpoint.x + 1, midpoint.y, midpoint.z);
        }
        else
        {
            return new Vector3(midpoint.x, midpoint.y + 1, midpoint.z);
        }
    }


    // public void ShowLength(Vector3 pos) {
    //     GameObject playerText = new GameObject("Text");
    //     TextMesh uiText = playerText.AddComponent<TextMesh>();
    //     playerText.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
    //     playerText.transform.position = new Vector3(pos.x, pos.y, pos.z-0.05f);
    //     uiText.fontSize = 100;
    //     uiText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
    //     uiText.text = "60";  
    //     uiText.color = Color.black;              
    // }
}
