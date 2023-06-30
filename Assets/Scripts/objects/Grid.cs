/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using System;
using UnityEngine;
using static GlobalVariables;
using JetBrains.Annotations;
using static UnityEditor.MaterialProperty;
using Newtonsoft.Json.Bson;

/// <summary>
/// Grid object keeps track of its helices.
/// </summary>
public class Grid
{
    private const int STARTLENGTH = 9;
    private const int STARTWIDTH = 9;
    private const float GRIDCIRCLESIZEFACTOR = 10.0f;

    private int _id;
    private string _plane;
    private Vector3 _startPos;
    private List<Vector3> _positions;
    private GridComponent[,] _grid2D;
    private List<Line> _lines;
    private List<Helix> _helices;
    private int _length;
    private int _width;
    private int _size;
    private GridPoint _minimumBound;
    private GridPoint _maximumBound;

    public Grid(string plane, Vector3 startPos)
    {
        _plane = plane;
        _startPos = startPos;
        _positions = new List<Vector3>();
        _lines = new List<Line>();
        _helices = new List<Helix>();
        _size = 0;
        setBounds();
        // 2D array with _length rows and _width columns
        _grid2D = new GridComponent[_length, _width];
        //GeneratePositions();
        DrawGrid();
    }

    private void setBounds()
    {
        _length = STARTLENGTH;
        _width = STARTWIDTH;
        _minimumBound = new GridPoint(-_length / 2, -_width / 2);
        _maximumBound = new GridPoint(_length / 2, _width / 2);
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
                    _positions.Add(new Vector3(_startPos.x + secDim / GRIDCIRCLESIZEFACTOR, _startPos.y + firstDim / GRIDCIRCLESIZEFACTOR, _startPos.z));
                }
                else if (this._plane.Equals("YZ"))
                {
                    _positions.Add(new Vector3(_startPos.x, _startPos.y + firstDim / GRIDCIRCLESIZEFACTOR, _startPos.z + secDim / GRIDCIRCLESIZEFACTOR));
                }
                else
                {
                    _positions.Add(new Vector3(_startPos.x + firstDim / GRIDCIRCLESIZEFACTOR, _startPos.y, _startPos.z + secDim / GRIDCIRCLESIZEFACTOR));
                }
            }
        }
    }

    private void DrawGrid()
    {
        if (_plane.Equals("XY"))
        {
            DrawGridXY();
        }
        else if (_plane.Equals("YZ"))
        {
            DrawGridYZ();
        }
        else
        {
            DrawGridXZ();
        }
    }

    private void DrawGridXY()
    {
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                int x = indexToGridX(i);
                int y = indexToGridY(j);
                GridPoint gridPoint = new GridPoint(x, y);
                Vector3 gamePosition = new Vector3(_startPos.x + i / GRIDCIRCLESIZEFACTOR, _startPos.y + j / GRIDCIRCLESIZEFACTOR, _startPos.z);
                GameObject gridGO = DrawPoint.MakeGridGO(gamePosition, gridPoint, "gridPoint");
                GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
                gridComponent.Grid = this;
                _grid2D[i, j] = gridComponent;
                _size++;
            }
        }
    }

    private void DrawGridYZ()
    {

    }

    private void DrawGridXZ() 
    { 

    }

    /// <summary>
    /// Converts 2D array row index to grid point x value.
    /// </summary>
    /// <param name="i">2D array row index.</param>
    /// <returns>grid point x value.</returns>
    private int indexToGridX(int i)
    {
        return i + _minimumBound.X;
    }

    /// <summary>
    /// Converts 2D array column index to grid point y value.
    /// </summary>
    /// <param name="j">2D array column index.</param>
    /// <returns>grid point y value.</returns>
    private int indexToGridY(int j)
    {
        return j + _minimumBound.Y; // j - _width / 2;
    }

    /// <summary>
    /// Converts grid point x value to 2D array row index.
    /// </summary>
    /// <param name="x">grid point x value.</param>
    /// <returns>2D array row index.</returns>
    private int gridXToIndex(int x)
    {
        return x - _minimumBound.X; // x + _length / 2;
    }

    /// <summary>
    /// Converts grid point y value to 2D array column index.
    /// </summary>
    /// <param name="y">grid point y value.</param>
    /// <returns>2D array column index.</returns>
    private int gridYToIndex(int y)
    {
        return y - _minimumBound.Y; // y + _width / 2;
    }

    public void checkExpansion(GridComponent gridComponent)
    {
        int x = gridComponent.GridPoint.X;
        int y = gridComponent.GridPoint.Y;
        int maxX = _maximumBound.X;
        int maxY = _maximumBound.Y;
        int minX = _minimumBound.X;
        int minY = _minimumBound.Y;
        if (x == maxX)
        {
            expandEast();
        }
        else if (x == minX)
        {
            expandWest();
        }
        if (y == maxY)
        {
            expandNorth();
        }
        else if(y == minY)
        {
            expandSouth();
        }
    }

    private void expandNorth()
    {
        // increase maximum y bound
        _width++;
        _maximumBound.Y++; //= new GridPoint(_maximumBound.X, _maximumBound.Y + 1);
        GridComponent[,] newGrid2D = new GridComponent[_length, _width];
        copyNorth(newGrid2D);
        _grid2D = newGrid2D;

        // create new grid components
        for (int i = 0; i < _length; i++)
        {
            int upperY = _width - 1;
            int x = indexToGridX(i);
            int y = indexToGridY(upperY);
            GridPoint gridPoint = new GridPoint(x, y);
            Vector3 gamePosition = new Vector3(_startPos.x + i / GRIDCIRCLESIZEFACTOR, _startPos.y + upperY / GRIDCIRCLESIZEFACTOR, _startPos.z);
            GameObject gridGO = DrawPoint.MakeGridGO(gamePosition, gridPoint, "gridPoint");
            GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
            gridComponent.Grid = this;
            _grid2D[i, upperY] = gridComponent;
            _size++;
        }
    }

    private void copyNorth(GridComponent[,] newGrid2D)
    {
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width - 1; j++)
            {
                newGrid2D[i, j] = _grid2D[i, j];
            }
        }
    }

    private void expandEast()
    {

    }

    private void expandSouth()
    {

    }

    private void expandWest()
    {

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
                    gridComponents.Add(_grid2D[k, l]);
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
