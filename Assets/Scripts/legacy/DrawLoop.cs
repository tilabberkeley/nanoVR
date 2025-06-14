/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawLoop : MonoBehaviour
{
    [SerializeField]
    private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    bool triggerReleased = true;
    bool primaryReleased = true;
    bool primaryPressed = false;
    List<GameObject> allCylinders = new List<GameObject>();
    Pointer temp;

    //  void Start()
    // {
    //     List<InputDevice> devices = new List<InputDevice>();
    //     InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
    //     InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);

    //     if (devices.Count > 0)
    //     {
    //         device = devices[0];
    //     }
    // }

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

    /// Update is called once per frame
    void Update() 
    {
        if (!GlobalVariables.s_curveTogOn) 
        {
            return;
        }

        if (!_device.isValid) {
            GetDevice();
        }

        bool triggerValue;
        if (triggerReleased 
            && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) 
            && triggerValue) 
        {
            triggerReleased = false;
            // primaryPressed = false;
            Vector3 direction = transform.rotation * Vector3.forward;
            DrawPoint.MakeSphere(transform.position + direction * 0.07f, "loop");
        }

        // if grip isn't pressed, keep rendering lines
        bool primaryValue;
        
        deletePreviousLoop();
        DrawGizmos();

        // if grip pressed, stop rendering and create line object
        if (primaryReleased 
            && _device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue) 
            && primaryValue) 
        {
            primaryReleased = false;
            primaryPressed = true;
            createLoopObject();
        }

        if (!(_device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue) && primaryValue)) 
        {
            primaryReleased = true;
        }

        if (!(_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)) 
        {
            triggerReleased = true;
            primaryPressed = false;
        }
    }


    void createLoopObject() 
    {
        deletePreviousLoop();
        DrawGizmos();
        Loop loop = new Loop(GlobalVariables.s_pointerList);
        //GlobalVariables.s_origamis.Add(loop);
        GlobalVariables.s_pointerList = new List<Pointer>();
        
    }

    void deletePreviousLoop() 
    {
        GlobalVariables.s_pointerList.Remove(temp);
        for(int i = 0; i <  allCylinders.Count; i++)
        {
            Destroy(allCylinders[i]);
        }
        allCylinders.Clear();
    }

    //Display without having to press play
	void DrawGizmos()
	{
        *//*List<Vector3> vectors = new List<Vector3>();
        GameObject newLine = new GameObject("Line");
        LineRenderer line= newLine.AddComponent<LineRenderer>();
        line.startColor = Color.red;
        line.endColor = Color.red;
        line.SetWidth(0.05f, 0.05f);
        line.positionCount = DrawPoints.pointerList.Count;
        for (int i = 0; i < DrawPoints.pointerList.Count; i++) {
            vectors.Add(DrawPoints.pointerList[i].getPosition());
        }
        line.SetPositions(vectors.ToArray());*//*
  
		//Draw the Catmull-Rom spline between the points
        if (!primaryPressed) 
        {
            Vector3 direction = transform.rotation * Vector3.forward;
            Vector3 currPoint = transform.position + direction * 0.07f;
            temp = new Pointer(currPoint.x, currPoint.y, currPoint.z);
            GlobalVariables.s_pointerList.Add(temp);
        }
		for (int i = 0; i < GlobalVariables.s_pointerList.Count; i++)
		{
            if ((i == 0 
                || i == GlobalVariables.s_pointerList.Count - 1) 
                && !GlobalVariables.s_loopTogOn)
			{
				continue;
			}
			DisplayCatmullRomSpline(i);
		}
	}

	//Display a spline between 2 points derived with the Catmull-Rom spline algorithm
	void DisplayCatmullRomSpline(int pos)
	{
		//The 4 points we need to form a spline between p1 and p2
		Vector3 p0 = GlobalVariables.s_pointerList[ClampListPos(pos - 1)].getPosition();
		Vector3 p1 = GlobalVariables.s_pointerList[pos].getPosition();
		Vector3 p2 = GlobalVariables.s_pointerList[ClampListPos(pos + 1)].getPosition();
		Vector3 p3 = GlobalVariables.s_pointerList[ClampListPos(pos + 2)].getPosition();

		//The start position of the line
		Vector3 lastPos = p1;

		//The spline's resolution
		//Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
		float resolution = 0.05f;

		//How many times should we loop?
		int loops = Mathf.FloorToInt(1f / resolution);

		for (int i = 1; i <= loops; i++)
		{
			//Which t position are we at?
			float t = i * resolution;

			//Find the coordinate between the end points with a Catmull-Rom spline
			Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

			//Draw this line segment
            if (primaryPressed) 
            {
                DrawRealLine(lastPos, newPos);
            } 
            else 
            {
                DrawLine(lastPos, newPos);
            }

			//Save this pos so we can draw the next line segment
			lastPos = newPos;
		}
	}

	//Clamp the list positions to allow looping
	int ClampListPos(int pos)
	{
		if (pos < 0)
		{
			pos = GlobalVariables.s_pointerList.Count - 1;
		}

		if (pos > GlobalVariables.s_pointerList.Count)
		{
			pos = 1;
		}
		else if (pos > GlobalVariables.s_pointerList.Count - 1)
		{
			pos = 0;
		}

		return pos;
	}

    void DrawRealLine(Vector3 startV, Vector3 endV) 
    {
        GameObject cylinder2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        var cylRenderer2 = cylinder2.GetComponent<Renderer>();
        cylRenderer2.material.SetColor("_Color", Color.green);
        Vector3 cylDefaultOrientation2 = new Vector3(0,1,0);
    
        // Position
        cylinder2.transform.position = (endV + startV)/2.0F;

        // Rotation
        Vector3 dirV2 = Vector3.Normalize(endV - startV);
        Vector3 rotAxisV2 = dirV2 + cylDefaultOrientation2;
        rotAxisV2 = Vector3.Normalize(rotAxisV2);
        cylinder2.transform.rotation = new Quaternion(rotAxisV2.x, rotAxisV2.y, rotAxisV2.z, 0);

        // Scale        
        float dist2 = Vector3.Distance(endV, startV);
        cylinder2.transform.localScale = new Vector3(0.007f, dist2/2, 0.007f);
    }

	void DrawLine(Vector3 startV, Vector3 endV) 
    {
        *//*GameObject newLine = new GameObject();
        LineRenderer line = newLine.AddComponent<LineRenderer>();
        line.startColor = Color.red;
        line.endColor = Color.red;
        line.SetWidth(0.01f, 0.01f);
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);*//*
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        var cylRenderer = cylinder.GetComponent<Renderer>();
        cylRenderer.material.SetColor("_Color", Color.red);
        Vector3 cylDefaultOrientation = new Vector3(0,1,0);
    
        // Position
        cylinder.transform.position = (endV + startV)/2.0F;

        // Rotation
        Vector3 dirV = Vector3.Normalize(endV - startV);
        Vector3 rotAxisV = dirV + cylDefaultOrientation;
        rotAxisV = Vector3.Normalize(rotAxisV);
        cylinder.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);

        // Scale        
        float dist = Vector3.Distance(endV, startV);
        cylinder.transform.localScale = new Vector3(0.007f, dist/2, 0.007f);

        
        allCylinders.Add(cylinder);
	}

	//Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
	//http://www.iquilezles.org/www/articles/minispline/minispline.htm
	Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		//The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
		Vector3 a = 2f * p1;
		Vector3 b = p2 - p0;
		Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

		//The cubic polynomial: a + b * t + c * t^2 + d * t^3
		Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

		return pos;
	}

    
}
*/