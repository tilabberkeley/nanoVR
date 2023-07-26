/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Grid object keeps track of its helices.
/// </summary>
public class Grid
{
    private const int STARTLENGTH = 5;
    private const int STARTWIDTH = 5;
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

    /* Need to keep track of the number of south and west positions because they are used to
     * calculate the offset from the _startPos to generate the grid circles in the scene. 
     * For example, if expandWest was called multiple times, expandNorth needs to know how 
     * many times that happened to correctly offset its new grid circle generations. 
     */
    private int _numSouthExpansions;
    private int _numWestExpansions;


    public Grid(int id, string plane, Vector3 startPos)
    {
        _id = id;
        _plane = plane;
        _startPos = startPos;
        _positions = new List<Vector3>();
        _lines = new List<Line>();
        _helices = new List<Helix>();
        _size = 0;
        SetBounds();
        // 2D array with _length rows and _width columns
        _grid2D = new GridComponent[_length, _width];
        //GeneratePositions();
        DrawGrid();
    }

    private void SetBounds()
    {
        _length = STARTLENGTH;
        _width = STARTWIDTH;
        _minimumBound = new GridPoint(-_length / 2, -_width / 2);
        _maximumBound = new GridPoint(_length / 2, _width / 2);
        _numWestExpansions = 0;
        _numSouthExpansions = 0;
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

    /// <summary>
    /// Draws the grid in the scene upon instantiation.
    /// </summary>
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

    /// <summary>
    /// Draws the grid in the XY direction.
    /// </summary>
    private void DrawGridXY()
    {
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                int x = IndexToGridX(i);
                int y = IndexToGridY(j);
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

    /// <summary>
    /// Draws the grid in the YZ direction.
    /// </summary>
    private void DrawGridYZ()
    {

    }

    /// <summary>
    /// Draws the grid in the XZ direction.
    /// </summary>
    private void DrawGridXZ() 
    { 

    }

    /// <summary>
    /// Converts 2D array row index to grid point x value.
    /// </summary>
    /// <param name="i">2D array row index.</param>
    /// <returns>grid point x value.</returns>
    private int IndexToGridX(int i)
    {
        return i + _minimumBound.X;
    }

    /// <summary>
    /// Converts 2D array column index to grid point y value.
    /// </summary>
    /// <param name="j">2D array column index.</param>
    /// <returns>grid point y value.</returns>
    private int IndexToGridY(int j)
    {
        return j + _minimumBound.Y; 
    }

    /// <summary>
    /// Converts grid point x value to 2D array row index.
    /// </summary>
    /// <param name="x">grid point x value.</param>
    /// <returns>2D array row index.</returns>
    private int GridXToIndex(int x)
    {
        return x - _minimumBound.X; 
    }

    /// <summary>
    /// Converts grid point y value to 2D array column index.
    /// </summary>
    /// <param name="y">grid point y value.</param>
    /// <returns>2D array column index.</returns>
    private int GridYToIndex(int y)
    {
        return y - _minimumBound.Y; 
    }

    /// <summary>
    /// Expands the grid if the position of the grid component is on the edge of the grid.
    /// </summary>
    /// <param name="gridComponent">Grid component to check if on the edge.</param>
    public void CheckExpansion(GridComponent gridComponent)
    {
        int x = gridComponent.GridPoint.X;
        int y = gridComponent.GridPoint.Y;
        int maxX = _maximumBound.X;
        int maxY = _maximumBound.Y;
        int minX = _minimumBound.X;
        int minY = _minimumBound.Y;
        if (x == maxX)
        {
            ExpandEast();
        }
        else if (x == minX)
        {
            ExpandWest();
        }
        if (y == maxY)
        {
            ExpandNorth();
        }
        else if (y == minY)
        {
            ExpandSouth();
        }
    }

    /// <summary>
    /// Expands this grid north.
    /// </summary>
    private void ExpandNorth()
    {
        // increase maximum y bound
        _width++;
        _maximumBound.Y++; 
        GridComponent[,] newGrid2D = new GridComponent[_length, _width];
        CopyNorth(newGrid2D);
        _grid2D = newGrid2D;

        // create new grid components
        for (int i = 0; i < _length; i++)
        {
            int newJ = _width - 1;
            int xCreationOffset = i - _numWestExpansions;
            int yCreationOffset = newJ - _numSouthExpansions;
            int x = IndexToGridX(i);
            int y = IndexToGridY(newJ);
            GridPoint gridPoint = new GridPoint(x, y);
            Vector3 gamePosition = new Vector3(_startPos.x + xCreationOffset / GRIDCIRCLESIZEFACTOR, _startPos.y + yCreationOffset / GRIDCIRCLESIZEFACTOR, _startPos.z);
            GameObject gridGO = DrawPoint.MakeGridGO(gamePosition, gridPoint, "gridPoint");
            GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
            gridComponent.Grid = this;
            _grid2D[i, newJ] = gridComponent;
            _size++;
        }
    }

    /// <summary>
    /// Helper method for expandNorth. Copies _grid2D in the bottom part of the given 2D array,
    /// leaving a row on the top empty.
    /// </summary>
    /// <param name="newGrid2D">2D array with the same length as _grid2D but with its width + 1.</param>
    private void CopyNorth(GridComponent[,] newGrid2D)
    {
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width - 1; j++)
            {
                newGrid2D[i, j] = _grid2D[i, j];
            }
        }
    }

    /// <summary>
    /// Expands this grid east.
    /// </summary>
    private void ExpandEast()
    {
        // increase maximum x bound
        _length++;
        _maximumBound.X++;
        GridComponent[,] newGrid2D = new GridComponent[_length, _width];
        CopyEast(newGrid2D);
        _grid2D = newGrid2D;

        // create new grid components
        for (int j = 0; j < _width; j++)
        {
            int newI = _length - 1;
            int xCreationOffset = newI - _numWestExpansions;
            int yCreationOffset = j - _numSouthExpansions;
            int x = IndexToGridX(newI);
            int y = IndexToGridY(j);
            GridPoint gridPoint = new GridPoint(x, y);
            Vector3 gamePosition = new Vector3(_startPos.x + xCreationOffset / GRIDCIRCLESIZEFACTOR, _startPos.y + yCreationOffset / GRIDCIRCLESIZEFACTOR, _startPos.z);
            GameObject gridGO = DrawPoint.MakeGridGO(gamePosition, gridPoint, "gridPoint");
            GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
            gridComponent.Grid = this;
            _grid2D[newI, j] = gridComponent;
            _size++;
        }
    }

    /// <summary>
    /// Helper method for expandEast. Copies _grid2D in the left part of the given 2D array,
    /// leaving a column on the right empty.
    /// </summary>
    /// <param name="newGrid2D">2D array with the same width as _grid2D but with its length + 1.</param>
    private void CopyEast(GridComponent[,] newGrid2D)
    {
        for (int i = 0; i < _length - 1; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                newGrid2D[i, j] = _grid2D[i, j];
            }
        }
    }

    /// <summary>
    /// Expands this grid south.
    /// </summary>
    private void ExpandSouth()
    {
        // decrease minimum y bound
        _width++;
        _minimumBound.Y--;
        GridComponent[,] newGrid2D = new GridComponent[_length, _width];
        CopySouth(newGrid2D);
        _grid2D = newGrid2D;

        // create new grid components
        for (int i = 0; i < _length; i++)
        {
            int newY = 0;
            int xCreationOffset = i - _numWestExpansions;
            int yCreationOffset = newY - _numSouthExpansions - 1;
            int x = IndexToGridX(i);
            int y = IndexToGridY(newY);
            GridPoint gridPoint = new GridPoint(x, y);
            Vector3 gamePosition = new Vector3(_startPos.x + xCreationOffset / GRIDCIRCLESIZEFACTOR, _startPos.y + yCreationOffset / GRIDCIRCLESIZEFACTOR, _startPos.z);
            GameObject gridGO = DrawPoint.MakeGridGO(gamePosition, gridPoint, "gridPoint");
            GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
            gridComponent.Grid = this;
            _grid2D[i, newY] = gridComponent;
            _size++;
        }
        _numSouthExpansions++;
    }

    /// <summary>
    /// Helper method for expandSouth. Copies _grid2D in the top part of the given 2D array,
    /// leaving a row on the bottom empty.
    /// </summary>
    /// <param name="newGrid2D">2D array with the same length as _grid2D but with its width + 1.</param>
    private void CopySouth(GridComponent[,] newGrid2D)
    {
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width - 1; j++)
            {
                newGrid2D[i, j + 1] = _grid2D[i, j];
            }
        }
    }

    /// <summary>
    /// Expands this grid west.
    /// </summary>
    private void ExpandWest()
    {
        // increase minimum x bound
        _length++;
        _minimumBound.X--;
        GridComponent[,] newGrid2D = new GridComponent[_length, _width];
        CopyWest(newGrid2D);
        _grid2D = newGrid2D;

        // create new grid components
        for (int j = 0; j < _width; j++)
        {
            int newI = 0;
            int xCreationOffset = newI - _numWestExpansions - 1;
            int yCreationOffset = j - _numSouthExpansions;
            int x = IndexToGridX(newI);
            int y = IndexToGridY(j);
            GridPoint gridPoint = new GridPoint(x, y);
            Vector3 gamePosition = new Vector3(_startPos.x + xCreationOffset / GRIDCIRCLESIZEFACTOR, _startPos.y + yCreationOffset / GRIDCIRCLESIZEFACTOR, _startPos.z);
            GameObject gridGO = DrawPoint.MakeGridGO(gamePosition, gridPoint, "gridPoint");
            GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
            gridComponent.Grid = this;
            _grid2D[newI, j] = gridComponent;
            _size++;
        }
        _numWestExpansions++;
    }

    /// <summary>
    /// Helper method for expandWest. Copies _grid2D in the right part of the given 2D array,
    /// leaving a column on the left empty.
    /// </summary>
    /// <param name="newGrid2D">2D array with the same width as _grid2D but with its length + 1.</param>
    private void CopyWest(GridComponent[,] newGrid2D)
    {
        for (int i = 0; i < _length - 1; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                newGrid2D[i + 1, j] = _grid2D[i, j];
            }
        }
    }

    /// <summary>
    /// Returns neighboring grid components of provided grid component.
    /// </summary>
    /// <param name="gridPoint">Location of grid component.</param>
    /// <returns>List of neighboring grid components.</returns>
    public List<GridComponent> GetNeighborGridComponents(GridPoint gridPoint)
    {
        List<GridComponent> gridComponents = new List<GridComponent>();
        // COME BACK AND FIX EDGE CASES
        int i = GridXToIndex(gridPoint.X);
        int j = GridYToIndex(gridPoint.Y);
        for (int k = i - 1; k <= i + 1; k++)
        {
            for (int l = j - 1; l <= j + 1; l++)
            {
                if (!(k == i && l == j))
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
        //_lines.Add(line);
    }

    public void DoAddHelix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, GridComponent gridComponent)
    {
        ICommand command = new CreateHelixCommand(id, startPoint, endPoint, orientation, gridComponent);
        CommandManager.AddCommand(command);
        command.Do();
    }

    public static void AddHelix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, int length, GridComponent gridComponent)
    {
        Helix helix = new Helix(id, startPoint, endPoint, orientation, length, gridComponent);
        s_helixDict.Add(id, helix);
        gridComponent.Helix = helix;
        gridComponent.Selected = true;
        s_numHelices += 1;

        //_helices.Add(helix);
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
