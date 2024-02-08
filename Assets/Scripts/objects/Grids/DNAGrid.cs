/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Abstract class for Grid objects.
/// </summary>
public abstract class DNAGrid
{
    /* Constants. Increase GRIDCIRCLESIZEFACTOR to decrease distance between grid circles and vice versa. */
    protected const int STARTLENGTH = 5;
    protected const int STARTWIDTH = 5;
    protected const float GRIDCIRCLESIZEFACTOR = 7.0f;
    protected const float DIAMETER = 1 / GRIDCIRCLESIZEFACTOR;
    protected const float RADIUS = DIAMETER / 2;
    public abstract string Type { get; }

    protected int _id;
    public int Id { get { return _id; } }

    protected string _plane;
    public string Plane { get { return _plane; } }

    protected Vector3 _startPos;
    public Vector3 StartPos 
    { get { return _startPos; } 
        set 
        {
            Debug.Log("Setting grid start pos to " + value.ToString());
            _startPos = value; 
        } 
    }

    protected List<Vector3> _positions;

    protected GridComponent[,] _grid2D;
    public GridComponent[,] Grid2D { get { return _grid2D; } }

    protected List<Line> _lines;
    protected List<Helix> _helices;
    protected int _length;
    public int Length { get { return _length; } }

    protected int _width;
    public int Width { get { return _width; } }

    protected int _size;
    protected GridPoint _minimumBound;
    protected GridPoint _maximumBound;

    /* Need to keep track of the number of south and west positions because they are used to
     * calculate the offset from the _startPos to generate the grid circles in the scene. 
     * For example, if expandWest was called multiple times, expandNorth needs to know how 
     * many times that happened to correctly offset its new grid circle generations. 
     */
    protected int _numSouthExpansions;
    protected int _numWestExpansions;

    /// <summary>
    /// Grid constructor. 
    /// </summary>
    /// <param name="id">Id number of this grid.</param>
    /// <param name="plane">Plane defintion.</param>
    /// <param name="startPos">3D location of where this grid starts.</param>
    public DNAGrid(int id, string plane, Vector3 startPos)
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
        DrawGrid();
    }

    /// <summary>
    /// Sets fields for bounds and expansions.
    /// </summary>
    private void SetBounds()
    {
        _length = STARTLENGTH;
        _width = STARTWIDTH;
        _minimumBound = new GridPoint(-_length / 2, -_width / 2);
        _maximumBound = new GridPoint(_length / 2, _width / 2);
        _numWestExpansions = 0;
        _numSouthExpansions = 0;
    }

    /// <summary>
    /// Generates a grid circle at the specified grid point.
    /// </summary>
    /// <param name="gridPoint">Grid point to generate circle at.</param>
    /// <param name="xOffset">x direction offset (depends on expansions).</param>
    /// <param name="yOffset">y direction offset (depends on expansions).</param>
    /// <param name="i">x memory location of grid circle in grid 2D.</param>
    /// <param name="j">j memory location of grid circle in grid 2D.</param>
    protected abstract void CreateGridCircle(GridPoint gridPoint, int xOffset, int yOffset, int i, int j);

    /// <summary>
    /// Draws the grid in the XY direction.
    /// </summary>
    protected void DrawGrid()
    {
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                int x = IndexToGridX(i);
                int y = IndexToGridY(j);
                GridPoint gridPoint = new GridPoint(x, y);
                CreateGridCircle(gridPoint, i, j, i, j);
                _size++;
            }
        }
    }

    /// <summary>
    /// Converts 2D array row index to grid point x value.
    /// </summary>
    /// <param name="i">2D array row index.</param>
    /// <returns>grid point x value.</returns>
    public int IndexToGridX(int i)
    {
        return i + _minimumBound.X;
    }

    /// <summary>
    /// Converts 2D array column index to grid point y value.
    /// </summary>
    /// <param name="j">2D array column index.</param>
    /// <returns>grid point y value.</returns>
    public int IndexToGridY(int j)
    {
        return j + _minimumBound.Y;
    }

    /// <summary>
    /// Converts grid point x value to 2D array row index.
    /// </summary>
    /// <param name="x">grid point x value.</param>
    /// <returns>2D array row index.</returns>
    public int GridXToIndex(int x)
    {
        return x - _minimumBound.X;
    }

    /// <summary>
    /// Converts grid point y value to 2D array column index.
    /// </summary>
    /// <param name="y">grid point y value.</param>
    /// <returns>2D array column index.</returns>
    public int GridYToIndex(int y)
    {
        return y - _minimumBound.Y;
    }

    /// <summary>
    /// Expands this grid north.
    /// </summary>
    protected void ExpandNorth()
    {
        CopyNorth();

        // create new grid components
        for (int i = 0; i < _length; i++)
        {
            int newJ = _width - 1;
            int xCreationOffset = i - _numWestExpansions;
            int yCreationOffset = newJ - _numSouthExpansions;
            int x = IndexToGridX(i);
            int y = IndexToGridY(newJ);
            GridPoint gridPoint = new GridPoint(x, y);
            CreateGridCircle(gridPoint, xCreationOffset, yCreationOffset, i, newJ);
            _size++;
        }
    }

    /// <summary>
    /// Expands this grid east.
    /// </summary>
    protected void ExpandEast()
    {
        CopyEast();

        // create new grid components
        for (int j = 0; j < _width; j++)
        {
            int newI = _length - 1;
            int xCreationOffset = newI - _numWestExpansions;
            int yCreationOffset = j - _numSouthExpansions;
            int x = IndexToGridX(newI);
            int y = IndexToGridY(j);
            GridPoint gridPoint = new GridPoint(x, y);
            CreateGridCircle(gridPoint, xCreationOffset, yCreationOffset, newI, j);
            _size++;
        }
    }

    /// <summary>
    /// Expands this grid south.
    /// </summary>
    protected void ExpandSouth()
    {
        CopySouth();

        // create new grid components
        for (int i = 0; i < _length; i++)
        {
            int newJ = 0;
            int xCreationOffset = i - _numWestExpansions;
            int yCreationOffset = newJ - _numSouthExpansions - 1;
            int x = IndexToGridX(i);
            int y = IndexToGridY(newJ);
            GridPoint gridPoint = new GridPoint(x, y);
            CreateGridCircle(gridPoint, xCreationOffset, yCreationOffset, i, newJ);
            _size++;
        }
        _numSouthExpansions++;
    }

    /// <summary>
    /// Expands this grid west.
    /// </summary>
    protected void ExpandWest()
    {
        CopyWest();

        // create new grid components
        for (int j = 0; j < _width; j++)
        {
            int newI = 0;
            int xCreationOffset = newI - _numWestExpansions - 1;
            int yCreationOffset = j - _numSouthExpansions;
            int x = IndexToGridX(newI);
            int y = IndexToGridY(j);
            GridPoint gridPoint = new GridPoint(x, y);
            CreateGridCircle(gridPoint, xCreationOffset, yCreationOffset, newI, j);
            _size++;
        }
        _numWestExpansions++;
    }

    /// <summary>
    /// Helper method for expandNorth. Expands _grid2D to become large enough for a north expansion.
    /// </summary>
    protected void CopyNorth()
    {
        // increase maximum y bound
        _width++;
        _maximumBound.Y++;
        GridComponent[,] newGrid2D = new GridComponent[_length, _width];

        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width - 1; j++)
            {
                newGrid2D[i, j] = _grid2D[i, j];
            }
        }

        _grid2D = newGrid2D;
    }

    /// <summary>
    /// Helper method for expandEast. Expands _grid2D to become large enough for a east expansion.
    /// </summary>
    protected void CopyEast()
    {
        // increase maximum x bound
        _length++;
        _maximumBound.X++;
        GridComponent[,] newGrid2D = new GridComponent[_length, _width];
        
        for (int i = 0; i < _length - 1; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                newGrid2D[i, j] = _grid2D[i, j];
            }
        }

        _grid2D = newGrid2D;
    }

    /// <summary>
    /// Helper method for expandSouth. Expands _grid2D to become large enough for a south expansion.
    /// </summary>
    protected void CopySouth()
    {
        // decrease minimum y bound
        _width++;
        _minimumBound.Y--;
        GridComponent[,] newGrid2D = new GridComponent[_length, _width];

        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width - 1; j++)
            {
                newGrid2D[i, j + 1] = _grid2D[i, j];
            }
        }

        _grid2D = newGrid2D;
    }

    /// <summary>
    /// Helper method for expandWest. Expands _grid2D to become large enough for a west expansion.
    /// </summary>
    protected void CopyWest()
    {
        // increase minimum x bound
        _length++;
        _minimumBound.X--;
        GridComponent[,] newGrid2D = new GridComponent[_length, _width];

        for (int i = 0; i < _length - 1; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                newGrid2D[i + 1, j] = _grid2D[i, j];
            }
        }

        _grid2D = newGrid2D;
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
    /// Returns neighboring grid components of provided grid component.
    /// </summary>
    /// <param name="gridPoint">Location of grid component.</param>
    /// <returns>List of neighboring grid components.</returns>
    public abstract List<GridComponent> GetNeighborGridComponents(GridPoint gridPoint);

    public void AddLine(int id, Vector3 startPoint, Vector3 endPoint)
    {
        Line line = new Line(id, startPoint, endPoint);
        //_lines.Add(line);
    }

    public void DoAddHelix(int id, Vector3 startPoint, int length, string orientation, GridComponent gridComponent)
    {
        ICommand command = new CreateHelixCommand(id, startPoint, length, orientation, gridComponent, this);
        CommandManager.AddCommand(command);
        command.Do();
    }

    public void AddHelix(int id, Vector3 startPoint, int length, string orientation, GridComponent gridComponent)
    {
        Helix helix = new Helix(id, startPoint, orientation, length, gridComponent);
        s_helixDict.Add(id, helix);
        gridComponent.Helix = helix;
        gridComponent.Selected = true;
        s_numHelices += 1;

        //_helices.Add(helix);
    }

    public void ChangeStencilView()
    {
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                GameObject go = _grid2D[i, j].gameObject;
                go.SetActive(s_hideStencils);
                if (_grid2D[i, j].Helix != null)
                {
                    _grid2D[i, j].Helix.ChangeStencilView();
                }
            }
        }
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
