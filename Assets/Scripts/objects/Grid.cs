using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

public abstract class Grid
{
    protected const int STARTLENGTH = 5;
    protected const int STARTWIDTH = 5;
    protected const float GRIDCIRCLESIZEFACTOR = 7.0f;

    protected int _id;
    protected string _plane;
    protected Vector3 _startPos;
    protected List<Vector3> _positions;

    protected GridComponent[,] _grid2D;
    public GridComponent[,] Grid2D { get { return _grid2D; } }

    protected List<Line> _lines;
    protected List<Helix> _helices;
    protected int _length;
    protected int _width;
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
    protected abstract void ExpandNorth();

    /// <summary>
    /// Expands this grid east.
    /// </summary>
    protected abstract void ExpandEast();

    /// <summary>
    /// Expands this grid south.
    /// </summary>
    protected abstract void ExpandSouth();

    // <summary>
    /// Expands this grid west.
    /// </summary>
    protected abstract void ExpandWest();

    /// <summary>
    /// Helper method for expandNorth. Copies _grid2D in the bottom part of the given 2D array,
    /// leaving a row on the top empty.
    /// </summary>
    /// <param name="newGrid2D">2D array with the same length as _grid2D but with its width + 1.</param>
    protected void CopyNorth(GridComponent[,] newGrid2D)
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
    /// Helper method for expandEast. Copies _grid2D in the left part of the given 2D array,
    /// leaving a column on the right empty.
    /// </summary>
    /// <param name="newGrid2D">2D array with the same width as _grid2D but with its length + 1.</param>
    protected void CopyEast(GridComponent[,] newGrid2D)
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
    /// Helper method for expandSouth. Copies _grid2D in the top part of the given 2D array,
    /// leaving a row on the bottom empty.
    /// </summary>
    /// <param name="newGrid2D">2D array with the same length as _grid2D but with its width + 1.</param>
    protected void CopySouth(GridComponent[,] newGrid2D)
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
    /// Helper method for expandWest. Copies _grid2D in the right part of the given 2D array,
    /// leaving a column on the left empty.
    /// </summary>
    /// <param name="newGrid2D">2D array with the same width as _grid2D but with its length + 1.</param>
    protected void CopyWest(GridComponent[,] newGrid2D)
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

    public void DoAddHelix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, GridComponent gridComponent)
    {
        ICommand command = new CreateHelixCommand(id, startPoint, endPoint, orientation, gridComponent, this);
        CommandManager.AddCommand(command);
        command.Do();
    }

    public void AddHelix(int id, Vector3 startPoint, string orientation, int length, GridComponent gridComponent)
    {
        Helix helix = new Helix(id, startPoint, orientation, length, gridComponent);
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
