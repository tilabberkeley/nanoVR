using Facebook.WitAi.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoneycombGrid : Grid
{
    /// <summary>
    /// Honeycomb Grid constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="plane"></param>
    /// <param name="startPos"></param>
    public HoneycombGrid(int id, string plane, Vector3 startPos) : base(id, plane, startPos) { }

    /// <summary>
    /// Generates a grid circle at the specified grid point.
    /// </summary>
    /// <param name="gridPoint">Grid point to generate circle at.</param>
    /// <param name="xOffset">x direction offset (depends on expansions).</param>
    /// <param name="yOffset">y direction offset (depends on expansions).</param>
    /// <param name="i">x memory location of grid circle in grid 2D.</param>
    /// <param name="j">j memory location of grid circle in grid 2D.</param>
    /// <returns>Game object of grid circle that was created.</returns>
    protected override void CreateGridCircle(GridPoint gridPoint, int xOffset, int yOffset, int i, int j)
    {
        bool isXEven = gridPoint.X % 2 == 0;
        bool isYEven = gridPoint.Y % 2 == 0;
        float r = (1 / GRIDCIRCLESIZEFACTOR) / 2;

        if (_plane.Equals("XY"))
        {
            float xPosition = _startPos.x + xOffset * (r * Mathf.Sqrt(3.0f));
            float yPosition = _startPos.y + yOffset / 2 * r * 6 + (!isYEven ? 2 * r : 0);

            if (!isXEven && isYEven)
            {
                yPosition -= r;
            }
            else if (!isXEven && !isYEven)
            {
                yPosition += r;
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

    public override List<GridComponent> GetNeighborGridComponents(GridPoint gridPoint) 
    {
        // TODO
        return null;
    }
}
