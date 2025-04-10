/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

public class GridCircleData
{
    private string _gridId;
    public string GridId { get { return _gridId; } }

    // 2D point of grid component on grid.
    // This is the local coordinate. It's world coordinate is determined relative to the grid of this grid circle.
    private GridPoint _gridPoint;
    public GridPoint GridPoint { get { return _gridPoint; } }

    // Helix on grid component. Is -1 if grid doesn't have a helix. Assigned on helix creation.
    private int _helixId = -1;
    public int HelixId { get { return _helixId; } set { _helixId = value;  } }

    // Whether or not this grid component has been clicked on before.
    // public bool Selected { get; set; }

    public GridCircleData(string gridId, GridPoint gridPoint)
    {
        _gridId = gridId;
        _gridPoint = gridPoint;
    }

    public Helix Helix
    {
        get
        {
            if (_helixId == -1)
            {
                return null;
            }
            return GlobalVariables.s_helixDict[_helixId];
        }
    }

    public DNAGrid dnaGrid
    {
        get
        {
            return GlobalVariables.s_gridDict[_gridId];
        }
    }

    /// <summary>
    /// The *local* transform matrix for this grid circle.
    /// This places the circle at its (x,y) position in "grid space" (z=0).
    /// No local rotation or local scale is applied here.
    /// </summary>
    public Matrix4x4 LocalMatrix
    {
        get
        {
            // Representing the local position always in the XY plane.
            // Transforming to the other planes or a different rotation will be done with the parent matrix.
            Vector3 localPosition = new Vector3(
                _gridPoint.X,
                _gridPoint.Y,
                0f
            );

            Quaternion localRotation = Quaternion.identity;
            Vector3 localScale = Vector3.one;   

            return Matrix4x4.TRS(localPosition, localRotation, localScale);
        }
    }

    /// <summary>
    /// This grid circles world position.
    /// </summary>
    public Vector3 Position
    {
        get
        {
            // TODO
            return Vector3.zero;
        }
    }
}
