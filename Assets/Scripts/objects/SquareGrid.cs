/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System;
using UnityEngine;
using static GlobalVariables;
using Oculus.Interaction;
using Facebook.WitAi.Utilities;

/// <summary>
/// Grid object keeps track of its helices.
/// </summary>
public class SquareGrid : Grid
{
    /// <summary>
    /// Square grid constructor. 
    /// </summary>
    /// <param name="id">Id number of this grid.</param>
    /// <param name="plane">Plane defintion.</param>
    /// <param name="startPos">3D location of where this grid starts.</param>
    public SquareGrid(int id, string plane, Vector3 startPos) : base(id, plane, startPos) { }

    /// <summary>
    /// Generates a grid circle at the specified grid point.
    /// </summary>
    /// <param name="gridPoint">Grid point to generate circle at.</param>
    /// <param name="xOffset">x direction offset (depends on expansions).</param>
    /// <param name="yOffset">y direction offset (depends on expansions).</param>
    /// <param name="i">x memory location of grid circle in grid 2D.</param>
    /// <param name="j">j memory location of grid circle in grid 2D.</param>
    protected override void CreateGridCircle(GridPoint gridPoint, int xOffset, int yOffset, int i, int j)
    {
        if (_plane.Equals("XY"))
        {
            Vector3 gamePosition = new Vector3(_startPos.x + xOffset * DIAMETER, _startPos.y + yOffset * DIAMETER, _startPos.z);
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
