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

    // Float coordinates to support none grid types.
    private float _floatx;
    public float FloatX { get { return _floatx; } set { _floatx = value; } }
    private float _floaty;
    public float FloatY { get { return _floaty; } set { _floaty = value; } }
    private float _floatz;
    public float FloatZ { get { return _floatz; } set { _floatz = value; } }

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

    public GridPoint(float x, float y, float z)
    {
        _floatx = x;
        _floaty = y;
        _floatz = z;
    }
}
