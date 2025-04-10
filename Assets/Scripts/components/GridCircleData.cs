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

    // The local XY offset of this grid circle. Depends on the type of grid this circle is apart of.
    private Vector2 _localOffset;

    // Whether or not this grid component has been clicked on before.
    // public bool Selected { get; set; }

    public GridCircleData(string gridId, GridPoint gridPoint, Vector2 localOffset)
    {
        _gridId = gridId;
        _gridPoint = gridPoint;
        _localOffset = localOffset;
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
                _localOffset.x,
                _localOffset.y,
                0f
            );

            Quaternion localRotation = Quaternion.identity;
            Vector3 localScale = Vector3.one;   

            return Matrix4x4.TRS(localPosition, localRotation, localScale);
        }
    }

    /// <summary>
    /// This grid circle's world position.
    /// </summary>
    public Vector3 Position
    {
        get
        {
            DNAGrid parentGrid = dnaGrid;

            // Get the grid's world matrix
            Matrix4x4 gridMatrix = parentGrid.GridMatrix;

            // Get this circle's local matrix (local position in grid space)
            Matrix4x4 localMatrix = LocalMatrix;

            // Multiply to get the final world-space transform
            Matrix4x4 finalMatrix = gridMatrix * localMatrix;

            // Extract the translation from the final matrix
            Vector4 column3 = finalMatrix.GetColumn(3);
            return new Vector3(column3.x, column3.y, column3.z);
        }
    }
}
