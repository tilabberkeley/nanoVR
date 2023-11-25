/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System;
using UnityEngine;
using static GlobalVariables;
using Oculus.Interaction;

/// <summary>
/// Grid object keeps track of its helices.
/// </summary>
public class SquareGrid : Grid
{
    public SquareGrid(int id, string plane, Vector3 startPos)
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
    /// Generates a grid circle at the specified grid point.
    /// </summary>
    /// <param name="gridPoint">Grid point to generate circle at.</param>
    /// <param name="xOffset">x direction offset (depends on expansions).</param>
    /// <param name="yOffset">y direction offset (depends on expansions).</param>
    /// <param name="i">x memory location of grid circle in grid 2D.</param>
    /// <param name="j">j memory location of grid circle in grid 2D.</param>
    /// <returns>Game object of grid circle that was created.</returns>
    private void CreateGridCircle(GridPoint gridPoint, int xOffset, int yOffset, int i, int j)
    {
        if (_plane.Equals("XY"))
        {
            Vector3 gamePosition = new Vector3(_startPos.x + xOffset / GRIDCIRCLESIZEFACTOR, _startPos.y + yOffset / GRIDCIRCLESIZEFACTOR, _startPos.z);
            GameObject gridGO = DrawPoint.MakeGridGO(gamePosition, gridPoint, "gridPoint");
            GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
            gridComponent.Grid = this;
            _grid2D[i, j] = gridComponent;
        }
        else if (_plane.Equals("YZ"))
        {
            // TODO
        }
        else
        {
            // TODO
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
                CreateGridCircle(gridPoint, i, j, i, j);
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
    /// Expands this grid north.
    /// </summary>
    protected override void ExpandNorth()
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
            CreateGridCircle(gridPoint, xCreationOffset, yCreationOffset, i, newJ);
            _size++;
        }
    }

    /// <summary>
    /// Expands this grid east.
    /// </summary>
    protected override void ExpandEast()
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
            CreateGridCircle(gridPoint, xCreationOffset, yCreationOffset, newI, j);
            _size++;
        }
    }

    /// <summary>
    /// Expands this grid south.
    /// </summary>
    protected override void ExpandSouth()
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
    protected override void ExpandWest()
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
            CreateGridCircle(gridPoint, xCreationOffset, yCreationOffset, newI, j);
            _size++;
        }
        _numWestExpansions++;
    }

    /// <summary>
    /// Returns neighboring grid components of provided grid component.
    /// </summary>
    /// <param name="gridPoint">Location of grid component.</param>
    /// <returns>List of neighboring grid components.</returns>
    public override List<GridComponent> GetNeighborGridComponents(GridPoint gridPoint)
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
}
