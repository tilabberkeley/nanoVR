/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public class BezierSpline
{
	private List<GameObject> _controlPoints;
	private Color _color;
	private static float WIDTH = 0.2f;
	private int _numberOfPoints;
	private List<Vector3> _positions;
	private List<GameObject> _cylinders;

	private int _curveCount = 0;
	private int layerOrder = 0;
	private int SEGMENT_COUNT = 50;

	public BezierSpline(List<GameObject> controlPoints, Color color)
	{
		_controlPoints = new List<GameObject>();
		for (int i = 0; i < controlPoints.Count; i += 1)
		{
			if (i % 2 == 0)
			{
				_controlPoints.Add(controlPoints[i]);
			}
		}
		_color = color;
		_numberOfPoints = (int) Math.Ceiling((double) controlPoints.Count / 2);
		_curveCount = (int) controlPoints.Count / 3;
		_positions = new List<Vector3>();
		_cylinders = new List<GameObject>();
		GeneratePoints();
		DrawCurve();
	}
	

	void GeneratePoints()
	{
		if (_numberOfPoints < 2)
		{
			_numberOfPoints = 2;
		}
		//lineRenderer.positionCount = _numberOfPoints * (_controlPoints.Count - 2);

		Vector3 p0, p1, p2;
		for (int j = 0; j < _controlPoints.Count - 2; j++)
		{
			// check control points
			if (_controlPoints[j] == null || _controlPoints[j + 1] == null
			|| _controlPoints[j + 2] == null)
			{
				return;
			}
			// determine control points of segment
			p0 = 0.5f * (_controlPoints[j].transform.position
			+ _controlPoints[j + 1].transform.position);
			p1 = _controlPoints[j + 1].transform.position;
			p2 = 0.5f * (_controlPoints[j + 1].transform.position
			+ _controlPoints[j + 2].transform.position);

			// set points of quadratic Bezier curve
			Vector3 position;
			float t;
			float pointStep = 1.0f / _numberOfPoints;
			if (j == _controlPoints.Count - 3)
			{
				pointStep = 1.0f / (_numberOfPoints - 1.0f);
				// last point of last segment should reach p2
			}
			for (int i = 0; i < _numberOfPoints; i++)
			{
				t = i * pointStep;
				position = (1.0f - t) * (1.0f - t) * p0
				+ 2.0f * (1.0f - t) * t * p1 + t * t * p2;
				_positions.Add(position);
			}
		}
	}

	Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u = 1 - t;
		float tt = t * t;
		float uu = u * u;
		float uuu = uu * u;
		float ttt = tt * t;

		Vector3 p = uuu * p0;
		p += 3 * uu * t * p1;
		p += 3 * u * tt * p2;
		p += ttt * p3;

		return p;
	}
	/*
	private List<GameObject> _controlPoints;
	private Color _color;
	private static float WIDTH = 0.2f;
	private int _numberOfPoints;
	private List<Vector3> _positions;
	private List<GameObject> _cylinders;

	public BezierSpline(List<GameObject> controlPoints, Color color)
    {
		for (int i = 0; i < controlPoints.Count; i += 1)
        {
			if (i % 2 == 0)
            {
				_controlPoints.Add(controlPoints[i]);
            }
        }
		_color = color;
		_numberOfPoints = (int) Math.Ceiling((double) controlPoints.Count / 2);
		GeneratePoints();
		DrawCurve();
    }
	/*
	void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.useWorldSpace = true;
		lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
	}*/

	/*
	void GeneratePoints()
	{
		if (null == lineRenderer || _controlPoints == null || _controlPoints.Count < 3)
		{
			return; // not enough points specified
		}
		// update line renderer
		
		
		lineRenderer.startColor = _color;
		lineRenderer.endColor = _color;
		lineRenderer.startWidth = WIDTH;
		lineRenderer.endWidth = WIDTH;
		

		if (_numberOfPoints < 2)
		{
			_numberOfPoints = 2;
		}
		//lineRenderer.positionCount = _numberOfPoints * (_controlPoints.Count - 2);

		Vector3 p0, p1, p2;
		for (int j = 0; j < _controlPoints.Count - 2; j++)
		{
			// check control points
			if (_controlPoints[j] == null || _controlPoints[j + 1] == null
			|| _controlPoints[j + 2] == null)
			{
				return;
			}
			// determine control points of segment
			p0 = 0.5f * (_controlPoints[j].transform.position
			+ _controlPoints[j + 1].transform.position);
			p1 = _controlPoints[j + 1].transform.position;
			p2 = 0.5f * (_controlPoints[j + 1].transform.position
			+ _controlPoints[j + 2].transform.position);

			// set points of quadratic Bezier curve
			Vector3 position;
			float t;
			float pointStep = 1.0f / _numberOfPoints;
			if (j == _controlPoints.Count - 3)
			{
				pointStep = 1.0f / (_numberOfPoints - 1.0f);
				// last point of last segment should reach p2
			}
			for (int i = 0; i < _numberOfPoints; i++)
			{
				t = i * pointStep;
				position = (1.0f - t) * (1.0f - t) * p0
				+ 2.0f * (1.0f - t) * t * p1 + t * t * p2;
				_positions.Add(position);
			}
		}
	}*/

	public void DrawCurve()
    {
		for (int i = 1; i < _positions.Count; i += 1)
        {
			GameObject cylinder = DrawPoint.MakeStrandCylinder(_positions[i], _positions[i - 1], _color);
			_cylinders.Add(cylinder);
        }
    }

	public void Delete()
    {
		foreach (GameObject go in _cylinders)
        {
			GameObject.Destroy(go);
        }
    }
}