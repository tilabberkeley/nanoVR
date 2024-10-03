/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class HexGrid : DNAGrid
{
    private const string TYPE = "hex";
    public override string Type { get { return TYPE; } }

    /// <summary>
    /// Square grid constructor. 
    /// </summary>
    /// <param name="id">Id number of this grid.</param>
    /// <param name="plane">Plane defintion.</param>
    /// <param name="startPos">3D location of where this grid starts.</param>
    public HexGrid(string id, string plane, Vector3 startPos) : base(id, plane, startPos) { }

    /// <summary>
    /// Generates a grid circle at the specified grid point.
    /// </summary>
    /// <param name="gridPoint">Grid point to generate circle at.</param>
    /// <param name="xOffset">x direction offset (depends on expansions).</param>
    /// <param name="yOffset">y direction offset (depends on expansions).</param>
    /// <param name="i">x memory location of grid circle in grid 2D.</param>
    /// <param name="j">j memory location of grid circle in grid 2D.</param>
    protected override GameObject CreateGridCircle(GridPoint gridPoint, int xOffset, int yOffset, int i, int j)
    {
        bool isXEven = gridPoint.X % 2 == 0;
        // bool isYEven = gridPoint.Y % 2 == 0;

        float xPosition = xOffset * (HELIX_GAP / 2 * Mathf.Sqrt(3.0f));
        float yPosition = yOffset * HELIX_GAP;

        if (!isXEven)
        {
            yPosition -= HELIX_GAP / 2;
        }

        GameObject gridGO = DrawPoint.MakeGridCircleGO(Position, StartGridCircle, xPosition, yPosition, _plane);
        GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
        gridComponent.Grid = this;
        gridComponent.GridPoint = gridPoint;
        _grid2D[i, j] = gridComponent;

        StaticBatchGridGO(gridGO);

        return gridGO;
    }

    /// <summary>
    /// Returns neighboring grid components of provided grid component.
    /// </summary>
    /// <param name="gridPoint">Location of grid component.</param>
    /// <returns>List of neighboring grid components.</returns>
    public override List<GridComponent> GetNeighborGridComponents(GridPoint gridPoint)
    {
        // TODO
        return null;
    }
}
