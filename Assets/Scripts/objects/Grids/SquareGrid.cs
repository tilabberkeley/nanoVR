/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using static Utils;

/// <summary>
/// Grid object keeps track of its helices.
/// </summary>
public class SquareGrid : DNAGrid
{
    private const string TYPE = "square";
    public override string Type { get { return TYPE; } }

    /// <summary>
    /// Square grid constructor. 
    /// </summary>
    /// <param name="id">Id number of this grid.</param>
    /// <param name="plane">Plane defintion.</param>
    /// <param name="startPos">3D location of where this grid starts.</param>
    public SquareGrid(string id, string plane, Vector3 startPos) : base(id, plane, startPos) { }

    /// <summary>
    /// Computes the local local XY offset of the grid circle depending on the x and y offset in the grid. 
    /// </summary>
    /// <param name="gridPoint">Grid point to generate circle at.</param>
    /// <param name="xOffset">x direction offset (depends on expansions).</param>
    /// <param name="yOffset">y direction offset (depends on expansions).</param>
    /// <returns>Local XY offset of the grid circle.</returns>
    protected override Vector2 ComputeLocalOffset(GridPoint gridPoint, int xOffset, int yOffset)
    {
        float x = xOffset * HELIX_GAP;
        float y = yOffset * HELIX_GAP;

        return new Vector2(x, y);
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
