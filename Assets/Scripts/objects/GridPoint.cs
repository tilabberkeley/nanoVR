using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grid points are used to represent grid components in a 2D discrete grid.
/// </summary>
public class GridPoint
{
    private int _x;
    public int X { get { return _x; } set { _x = value; } }
    private int _y;
    public int Y { get { return _y; } set { _y = value; } }

    /// <summary>
    /// Grid point constructor.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    public GridPoint(int x, int y)
    {
        _x = x;
        _y = y;
    }
}
