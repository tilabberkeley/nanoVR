/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NoneGrid is a grid with no set lattice (similar to scadnano's none grid type).
/// Because of this, many of the methods are not implemented.
/// </summary>
public class NoneGrid : DNAGrid
{
    private const string TYPE = "none";
    public override string Type { get { return TYPE; } }

    public override Vector3 Position => _position;

    public override GameObject StartGridCircle => null;

    /// <summary>
    /// Square grid constructor. 
    /// </summary>
    /// <param name="id">Id number of this grid.</param>
    /// <param name="plane">Plane defintion.</param>
    /// <param name="startPos">3D location of where this grid starts.</param>
    public NoneGrid(string id, string plane, Vector3 startPos) : base(id, plane, startPos) { }

    /// <summary>
    /// Generates a grid circle at the specified grid point.
    /// </summary>
    /// <param name="gridPoint">Grid point to generate circle at.</param>
    /// <param name="xOffset">x direction offset (depends on expansions).</param>
    /// <param name="yOffset">y direction offset (depends on expansions).</param>
    /// <param name="i">x memory location of grid circle in grid 2D.</param>
    /// <param name="j">j memory location of grid circle in grid 2D.</param>
    public override GameObject CreateGridCircle(GridPoint gridPoint, int xOffset, int yOffset, int i, int j)
    {
        throw new NotImplementedException("CreateGridCircle is not implemented for NoneGrid.");
    }

    public GameObject CreateNoneGridCircle(GridPoint gridPoint, Vector3 pos)
    {
        GameObject gridGO = DrawPoint.MakeGridCircleGO(pos, null, 0, 0, _plane, gridPoint);
        GridComponent gridComponent = gridGO.GetComponent<GridComponent>();
        gridComponent.Grid = this;
        gridComponent.GridPoint = gridPoint;
        _gridComponents.Add(gridComponent);
        StaticBatchGridGO(gridGO);

        return gridGO;
    }

    /// <summary>
    /// Returns an empty list.
    /// </summary>
    public override List<GridComponent> GetNeighborGridComponents(GridPoint gridPoint)
    {
        return new List<GridComponent>();
    }

    protected override void SetBounds()
    {
        return;
    }

    protected override void DrawGrid()
    {
        return;
    }

    public override void ExpandNorth()
    {
        return;
    }

    public override void ExpandSouth()
    {
        return;
    }

    public override void ExpandEast()
    {
        return;
    }

    public override void ExpandWest()
    {
        return;
    }

    public override int GridXToIndex(int x)
    {
        // TODO: Should this be overriden or just throw an error?
        throw new NotImplementedException("GridXToIndex is not implemented for NoneGrid.");
    }

    public override int GridYToIndex(int y)
    {
        throw new NotImplementedException("GridYToIndex is not implemented for NoneGrid.");
    }
}
