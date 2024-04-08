/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public class HoneycombGrid : DNAGrid
{
    private const string TYPE = "honeycomb";
    public override string Type { get { return TYPE; } }

    /// <summary>
    /// Honeycomb grid constructor. 
    /// </summary>
    /// <param name="id">Id number of this grid.</param>
    /// <param name="plane">Plane defintion.</param>
    /// <param name="startPos">3D location of where this grid starts.</param>
    public HoneycombGrid(string id, string plane, Vector3 startPos) : base(id, plane, startPos) { }

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
        bool isYEven = gridPoint.Y % 2 == 0;

        float xPosition = xOffset * (RADIUS * Mathf.Sqrt(3.0f));
        // Doing the bit shift right once is the same as floor div 2, but C# has weird behavior with negatives, so bit shift fixes it. 
        float yPosition = (yOffset >> 1) * RADIUS * 6 + (!isYEven ? 2 * RADIUS : 0);

        if (!isXEven && isYEven)
        {
            yPosition -= RADIUS;
        }
        else if (!isXEven && !isYEven)
        {
            yPosition += RADIUS;
        }
            
        GameObject gridGO = DrawPoint.MakeGridCircleGO(StartGridCircle, xPosition, yPosition, _plane);
        GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
        gridComponent.Grid = this;
        gridComponent.GridPoint = gridPoint;
        _grid2D[i, j] = gridComponent;
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
