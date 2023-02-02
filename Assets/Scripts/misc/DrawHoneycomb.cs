using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawHoneycomb : MonoBehaviour
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
    private Honeycomb _honeycomb;
    bool honeycombExists = false;


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

    void Update() 
    {
        if (!GlobalVariables.s_honeycombTogOn) 
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
                && !honeycombExists) 
        {
            triggerReleased = false;
            honeycombExists = true;
            plane = dropdown.options[dropdown.value].text;
            Vector3 direction = transform.rotation * Vector3.forward;
            Vector3 currPoint = transform.position + direction * 0.07f ;
            Honeycomb honeycomb = new Honeycomb(plane, currPoint); 
        }
        if (triggerReleased 
                && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && triggerValue 
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            if (s_hit.collider.name.Equals("honeycomb")) 
            {
                Vector3 midpoint = s_hit.collider.bounds.center;
                Vector3 startPos;
                Vector3 endPos;
            
                if (plane.Equals("XY")) 
                {
                    startPos = new Vector3(midpoint.x, midpoint.y, midpoint.z - 1);
                    endPos = new Vector3(midpoint.x, midpoint.y, midpoint.z + 1);
                } 
                else if (plane.Equals("YZ")) 
                {
                    startPos = new Vector3(midpoint.x - 1, midpoint.y, midpoint.z);
                    endPos = new Vector3(midpoint.x + 1, midpoint.y, midpoint.z);
                } 
                else 
                {
                    startPos = new Vector3(midpoint.x, midpoint.y - 1, midpoint.z);
                    endPos = new Vector3(midpoint.x, midpoint.y + 1, midpoint.z);
                }
                _honeycomb.AddLine(_honeycomb.getLines().Count + 1, startPos, endPos);
                _honeycomb.AddHelix(_honeycomb.getLines().Count + 1, startPos, endPos, plane);
            }
        }

        bool gripValue;
        if (gripReleased 
                && _device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue) 
                && gripValue
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit)) 
        {
            //gripReleased = false;
            Vector3 spherePos = s_hit.collider.bounds.center;
            Vector3 direction = transform.rotation * Vector3.forward;
            Vector3 rayPos = transform.position + direction * 0.07f ;
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
                Line line = _honeycomb.getLine(index);
                line.SetStart(newPos);
            } 
            else if (s_hit.collider.name.Contains("endPoint")) 
            {
                int index = Int32.Parse(s_hit.collider.name.Substring(s_hit.collider.name.Length - 1));
                Line line = _honeycomb.getLine(index);
                line.SetEnd(newPos);
            }
        }

        if (!(_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)) 
        {
            triggerReleased = true;
        }

        bool primaryValue;
        if (!(_device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue) && primaryValue)) 
        {
            _honeycomb.ShowHelices();
        }

        // if (!(device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue) && gripValue)) {
        //     gripReleased = true;
        // }

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
