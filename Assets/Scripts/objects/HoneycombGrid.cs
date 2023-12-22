using Facebook.WitAi.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoneycombGrid : Grid
{
    /// <summary>
    /// Honeycomb grid constructor. 
    /// </summary>
    /// <param name="id">Id number of this grid.</param>
    /// <param name="plane">Plane defintion.</param>
    /// <param name="startPos">3D location of where this grid starts.</param>
    public HoneycombGrid(int id, string plane, Vector3 startPos) : base(id, plane, startPos) { }

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
        bool isXEven = gridPoint.X % 2 == 0;
        bool isYEven = gridPoint.Y % 2 == 0;

        if (_plane.Equals("XY"))
        {
            float xPosition = _startPos.x + xOffset * (RADIUS * Mathf.Sqrt(3.0f));
            float yPosition = _startPos.y + (yOffset >> 1) * RADIUS * 6 + (!isYEven ? 2 * RADIUS : 0);

            if (!isXEven && isYEven)
            {
                yPosition -= RADIUS;
            }
            else if (!isXEven && !isYEven)
            {
                yPosition += RADIUS;
            }

            Vector3 gamePosition = new Vector3(xPosition, yPosition, _startPos.z);
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
        // TODO
        return null;
    }
}
