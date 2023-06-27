/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using System;
using UnityEngine;
using static GlobalVariables;
using JetBrains.Annotations;

/// <summary>
/// Grid object keeps track of its helices.
/// </summary>
public class Grid
{
    private const int s_startLength = 9;
    private const int s_startWidth = 9;

    private int _id;
    private string _plane;
    private Vector3 _startPos;
    private List<Vector3> _positions;
    private List<GameObject> _gridPoints;
    private List<Line> _lines;
    private List<Helix> _helices;
    private int _length;
    private int _width;
    private int _numNodes;

    public Grid(string plane, Vector3 startPos)
    {
        _plane = plane;
        _startPos = startPos;
        _positions = new List<Vector3>();
        _lines = new List<Line>();
        _helices = new List<Helix>();
        _gridPoints = new List<GameObject>();
        _numNodes = 0;
        _length = s_startLength;
        _width = s_startWidth;
        GeneratePositions();
        DrawGrid();
    }

    public String Plane { get; }
    public Vector3 StartPos { get; }
    public List<Vector3> GetNodes() { return _positions; }
    public List<Line> GetLines() { return _lines; }
    public Line GetLine(int index) { return _lines[index]; }
    public List<Helix> GetHelices() { return _helices; }
    public Helix GetHelix(int index) { return _helices[index]; }

    private void GeneratePositions()
    {
        for (int i = 0, secDim = 0; secDim < _length; secDim++)
        {
            for (int firstDim = 0; firstDim < _width; firstDim++, i++)
            {
                if (_plane.Equals("XY"))
                {
                    _positions.Add(new Vector3(_startPos.x + firstDim / 10f, _startPos.y + secDim / 10f, _startPos.z));
                }
                else if (this._plane.Equals("YZ"))
                {
                    _positions.Add(new Vector3(_startPos.x, _startPos.y + firstDim / 10f, _startPos.z + secDim / 10f));
                }
                else
                {
                    _positions.Add(new Vector3(_startPos.x + firstDim / 10f, _startPos.y, _startPos.z + secDim / 10f));
                }
            }
        }
    }

    private void DrawGrid()
    {
        if (_positions.Count == 0)
        {
            return;
        }
        int positionIndex = 0;
        int startX = -_length / 2;
        int endX = _length / 2;
        int startY = _width / 2;
        int endY = -_width / 2;
        for (int i = startX; i <= endX; i++)
        {
            for (int j = startY; j >= endY; j--)
            {
                _gridPoints.Add(DrawPoint.MakeGridPoint(_positions[positionIndex], i, j, "gridPoint"));
                _numNodes++;
                positionIndex++;
            }
        }
    }

    public void AddLine(int id, Vector3 startPoint, Vector3 endPoint)
    {
        Line line = new Line(id, startPoint, endPoint);
        _lines.Add(line);
    }

    public void DoAddHelix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, Vector3 gridPoint)
    {
        ICommand command = new CreateHelixCommand(this, id, startPoint, endPoint, orientation, gridPoint);
        CommandManager.AddCommand(command);
        command.Do();
    }

    public void AddHelix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, Vector3 gridPoint)
    {
        Helix helix = new Helix(id, startPoint, endPoint, orientation, gridPoint);
        _helices.Add(helix);
    }

    /// <summary>
    /// Changes rendering of the lines and helixes in grid.
    /// 
    /// Note: Only method that changes value of s_nucleotideView.
    /// </summary>
    public void ChangeRendering()
    {
        s_nucleotideView = !s_nucleotideView;
        for (int i = 0; i < _lines.Count; i++)
        {
            _lines[i].ChangeRendering();
            _helices[i].ChangeRendering();
        }
    }
}
