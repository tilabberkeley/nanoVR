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
    //public Dictionary<GridPoint, GameObject> GridPointDict { get; set; }
    private GridComponent[,] Grid2D { get; set; }
    private List<Line> _lines;
    private List<Helix> _helices;
    private int _length;
    private int _width;
    private int _numGridComponents;

    public Grid(string plane, Vector3 startPos)
    {
        _plane = plane;
        _startPos = startPos;
        _positions = new List<Vector3>();
        _lines = new List<Line>();
        _helices = new List<Helix>();
        //GridPointDict = new Dictionary<GridPoint, GameObject>();
        _numGridComponents = 0;
        _length = s_startLength;
        _width = s_startWidth;
        // 2D array with _length rows and _width columns
        Grid2D = new GridComponent[_length, _width];
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
        /*
        int startX = -_length / 2;
        int endX = _length / 2;
        int startY = _width / 2;
        int endY = -_width / 2;
        */
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                int x = indexToGridX(i);
                int y = indexToGridY(j);
                GridPoint gridPoint = new GridPoint(x, y);
                // Create game object and assign it to gridGO
                GameObject gridGO = DrawPoint.MakeGridGO(_positions[positionIndex], gridPoint, "gridPoint");
                GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
                gridComponent.Grid = this;
                Grid2D[i, j] = gridComponent;
                //GridPointDict.Add(gridPoint, gridGO);
                _numGridComponents++;  
                positionIndex++;
            }
        }
    }

    /// <summary>
    /// Converts 2D array row index to grid point x value.
    /// </summary>
    /// <param name="i">2D array row index.</param>
    /// <returns>grid point x value.</returns>
    private int indexToGridX(int i)
    {
        return i - _length / 2;
    }

    /// <summary>
    /// Converts 2D array column index to grid point y value.
    /// </summary>
    /// <param name="j">2D array column index.</param>
    /// <returns>grid point y value.</returns>
    private int indexToGridY(int j)
    {
        return j - _width / 2;
    }

    /// <summary>
    /// Converts grid point x value to 2D array row index.
    /// </summary>
    /// <param name="x">grid point x value.</param>
    /// <returns>2D array row index.</returns>
    private int gridXToIndex(int x)
    {
        return x + _length / 2;
    }

    /// <summary>
    /// Converts grid point y value to 2D array column index.
    /// </summary>
    /// <param name="y">grid point y value.</param>
    /// <returns>2D array column index.</returns>
    private int gridYToIndex(int y)
    {
        return y + _width / 2;
    }

    /// <summary>
    /// Returns neighboring grid components of provided grid component.
    /// </summary>
    /// <param name="gridPoint">Location of grid component.</param>
    /// <returns>List of neighboring grid components.</returns>
    public List<GridComponent> getNeighborGridComponents(GridPoint gridPoint)
    {
        List<GridComponent> gridComponents = new List<GridComponent>();
        // COME BACK AND FIX EDGE CASES
        int i = gridXToIndex(gridPoint.X);
        int j = gridYToIndex(gridPoint.Y);
        for (int k = i - 1; k <= i + 1; k++)
        {
            for (int l = j - 1; l <= j + 1; l++)
            {
                if (!(k == 0 && l == 0))
                {
                    gridComponents.Add(Grid2D[k, l]);
                }
            }
        }
        return gridComponents;
    }

    public void AddLine(int id, Vector3 startPoint, Vector3 endPoint)
    {
        Line line = new Line(id, startPoint, endPoint);
        _lines.Add(line);
    }

    public void DoAddHelix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, GridComponent gridComponent)
    {
        ICommand command = new CreateHelixCommand(this, id, startPoint, endPoint, orientation, gridComponent);
        CommandManager.AddCommand(command);
        command.Do();
    }

    public void AddHelix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, GridComponent gridComponent)
    {
        Helix helix = new Helix(id, startPoint, endPoint, orientation, gridComponent);
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
