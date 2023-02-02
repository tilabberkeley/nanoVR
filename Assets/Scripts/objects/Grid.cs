using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Grid
{
    private int _id;
    private string _plane;
    private Vector3 _startPos;
    private List<Vector3> _nodes;
    private List<Line> _lines;
    private List<Helix> _helices;
    private int _numNodes;


    public Grid(string plane, Vector3 startPos) {
        _plane = plane;
        _startPos = startPos;
        _nodes = new List<Vector3>();
        _lines = new List<Line>();
        _helices = new List<Helix>();
        _numNodes = 100;
        Generate();
        DrawGrid();
    }

    public String Plane { get; }

    public Vector3 StartPos { get; }

    public List<Vector3> Nodes { get; }
    public List<Line> Lines { get; }

    public Line GetLine(int index) {return _lines[index];}

    private void Generate() 
    {
		for (int i = 0, secDim = 0; secDim < 10; secDim++) 
        {
			for (int firstDim = 0; firstDim < 10; firstDim++, i++) 
            {
                if (_plane.Equals("XY")) 
                {
                    _nodes.Add(new Vector3(_startPos.x + firstDim/20f, _startPos.y + secDim/20f, _startPos.z));
                } else if (this._plane.Equals("YZ")) 
                {
                    _nodes.Add(new Vector3(_startPos.x, _startPos.y + firstDim/20f, _startPos.z + secDim/20f));
                } else 
                {
                    _nodes.Add(new Vector3(_startPos.x + firstDim/20f, _startPos.y, _startPos.z + secDim/20f));
                }
			}
		}
	}

    private void DrawGrid () 
    {
		if (_nodes.Count == 0) 
        {
			return;
		}
		for (int i = 0; i < _nodes.Count; i++) 
        {
            GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            circle.AddComponent<NucleotideComponent>();
            circle.name = "grid";
            circle.transform.position = _nodes[i];
            circle.transform.localScale = new Vector3(0.03f, 0.0001f, 0.03f);
			circle.transform.Rotate(90f, 0f, 0f, 0);
            // sphere.AddComponent<XRGrabInteractable>();
            // var sphereXRGrab = sphere.GetComponent<XRGrabInteractable>();
            // sphereXRGrab.throwOnDetach = false;
            // var sphereRigidbody = sphere.GetComponent<Rigidbody>();
            // sphereRigidbody.useGravity = false;
            // sphereRigidbody.isKinematic = true;
            var circleRenderer = circle.GetComponent<Renderer>();
            circleRenderer.material.SetColor("_Color", Color.gray);
        }
	}

    public void AddLine(int id, Vector3 startPoint, Vector3 endPoint) 
    {
        Line line = new Line(id, startPoint, endPoint);
        _lines.Add(line);
    }

    public void AddHelix(int id, Vector3 startPoint, Vector3 endPoint, string orientation) 
    {
        Helix helix = new Helix(id, startPoint, endPoint, orientation);
        _helices.Add(helix);
    }

    public void ShowHelices() 
    {
        for (int i = 0; i < _lines.Count; i++) 
        {
              
            _lines[i].HideLine();
            _helices[i].ShowHelix();
        }
    }

    public void ShowLines() 
    {
        for (int i = 0; i < _lines.Count; i++) 
        {
            _lines[i].ShowLine();
            _helices[i].HideHelix();
        }
    }
 
}
