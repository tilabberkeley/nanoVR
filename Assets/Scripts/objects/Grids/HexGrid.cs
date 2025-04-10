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
    /// Computes the local local XY offset of the grid circle depending on the x and y offset in the grid. 
    /// </summary>
    /// <param name="gridPoint">Grid point to generate circle at.</param>
    /// <param name="xOffset">x direction offset (depends on expansions).</param>
    /// <param name="yOffset">y direction offset (depends on expansions).</param>
    /// <returns>Local XY offset of the grid circle.</returns>
    protected override Vector2 ComputeLocalOffset(GridPoint gridPoint, int xOffset, int yOffset)
    {
        bool isXEven = (gridPoint.X % 2 == 0);

        float xPosition = xOffset * (HELIX_GAP / 2f * Mathf.Sqrt(3.0f));
        float yPosition = yOffset * HELIX_GAP;

        if (!isXEven)
        {
            yPosition -= (HELIX_GAP / 2f);
        }

        return new Vector2(xPosition, yPosition);
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
