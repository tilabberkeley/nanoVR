/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public class BezierSpline
{
	private List<Vector3> _controlPoints;
	private Color _color;
	private List<GameObject> _cylinders;

	public BezierSpline(List<GameObject> controlPoints, Color color)
	{
		_controlPoints = new List<Vector3>();
		for (int i = 0; i < controlPoints.Count; i += 1)
		{
			if (i % 2 == 0)
			{
				_controlPoints.Add(controlPoints[i].transform.position);
			}
		}
		_color = color;
		//_numberOfPoints = (int) Math.Ceiling((double) controlPoints.Count / 2);
		//_curveCount = (int) controlPoints.Count / 3;
		//_positions = new List<Vector3>();
		_cylinders = new List<GameObject>();
		DrawGizmos();
		//DrawCurve();
	}
	
	private void DrawGizmos()
    {
		for (int i = 0; i < _controlPoints.Count; i++)
		{
			
			if (i == 0 || i == _controlPoints.Count - 1)
			{
				continue;
			}
			DisplayCatmullRomSpline(i);
		}
	}

	void DisplayCatmullRomSpline(int pos)
	{
		//The 4 points we need to form a spline between p1 and p2
		Vector3 p0 = _controlPoints[ClampListPos(pos - 1)];
		Vector3 p1 = _controlPoints[pos];
		Vector3 p2 = _controlPoints[ClampListPos(pos + 1)];
		Vector3 p3 = _controlPoints[ClampListPos(pos + 2)];

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
			//GameObject cylinder = DrawPoint.MakeStrandCylinder(lastPos, newPos, _color);
			//_cylinders.Add(cylinder);

			//Save this pos so we can draw the next line segment
			lastPos = newPos;
		}
	}

	//Clamp the list positions to allow looping
	int ClampListPos(int pos)
	{
		if (pos < 0)
		{
			pos = _controlPoints.Count - 1;
		}
		if (pos > _controlPoints.Count)
		{
			pos = 1;
		}
		else if (pos > _controlPoints.Count - 1)
		{
			pos = 0;
		}
		return pos;
	}

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

	public void Delete()
    {
		foreach (GameObject go in _cylinders)
        {
			GameObject.DestroyImmediate(go);
        }
    }
}