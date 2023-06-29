using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPoint
{
    public int X { get; private set; }
    public int Y { get; private set; }

    /// <summary>
    /// Grid point constructor.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    public GridPoint(int x, int y)
    {
        X = x;
        Y = y;
    }
}
