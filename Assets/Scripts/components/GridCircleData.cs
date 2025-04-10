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

    /// <summary>
    /// Returns the pure world rotation of this grid circle (Quaternion),
    /// factoring out any scaling in the parent grid's matrix.
    /// </summary>
    public Quaternion Rotation
    {
        get
        {
            DNAGrid parentGrid = dnaGrid;

            // Multiply the grid's world matrix by this circle's local matrix
            Matrix4x4 finalMatrix = parentGrid.GridMatrix * LocalMatrix;

            // Convert that final matrix to a pure rotation quaternion (removing non-uniform scale).
            return ExtractPureRotation(finalMatrix);
        }
    }

    /// <summary>
    /// Returns the forward direction of this grid circle.
    /// </summary>
    public Vector3 Forward
    {
        get
        {
            DNAGrid parentGrid = dnaGrid;
            Matrix4x4 finalMatrix = parentGrid.GridMatrix * LocalMatrix;

            // Extract the pure rotation (strips out non-uniform scale).
            Quaternion pureRotation = ExtractPureRotation(finalMatrix);

            // The forward direction is rotation * (0,0,1).
            return pureRotation * Vector3.forward;
        }
    }

    /// <summary>
    /// Returns the right direction of this grid circle.
    /// </summary>
    public Vector3 Right
    {
        get
        {
            DNAGrid parentGrid = dnaGrid;
            Matrix4x4 finalMatrix = parentGrid.GridMatrix * LocalMatrix;

            // Extract the pure rotation (strips out non-uniform scale).
            Quaternion pureRotation = ExtractPureRotation(finalMatrix);

            // Multiply that rotation by Vector3.right to get the world +X axis
            return pureRotation * Vector3.right;
        }
    }


    /// <summary>
    /// Extracts a pure rotation (Quaternion) from a 4x4 transform matrix,
    /// removing any non-uniform scaling by normalizing each axis in the top-left 3x3.
    /// </summary>
    private static Quaternion ExtractPureRotation(Matrix4x4 matrix)
    {
        // 1) Pull out each basis vector (the columns) from the 3×3 part
        Vector3 col0 = new Vector3(matrix.m00, matrix.m10, matrix.m20);
        Vector3 col1 = new Vector3(matrix.m01, matrix.m11, matrix.m21);
        Vector3 col2 = new Vector3(matrix.m02, matrix.m12, matrix.m22);

        // 2) Compute their magnitudes (the scale factors)
        float len0 = col0.magnitude;
        float len1 = col1.magnitude;
        float len2 = col2.magnitude;

        // Avoid divide-by-zero if scale is extremely small
        if (len0 < 1e-5f || len1 < 1e-5f || len2 < 1e-5f)
        {
            // Fallback: no valid rotation
            return Quaternion.identity;
        }

        // 3) Normalize each column to remove scaling
        col0 /= len0;
        col1 /= len1;
        col2 /= len2;

        // 4) Rebuild a 3×3 rotation matrix with no scale
        Matrix4x4 rotationOnly = Matrix4x4.identity;
        rotationOnly.m00 = col0.x; rotationOnly.m01 = col1.x; rotationOnly.m02 = col2.x;
        rotationOnly.m10 = col0.y; rotationOnly.m11 = col1.y; rotationOnly.m12 = col2.y;
        rotationOnly.m20 = col0.z; rotationOnly.m21 = col1.z; rotationOnly.m22 = col2.z;

        // 5) Extract a Quaternion from that pure rotation matrix
        return ExtractRotation(rotationOnly);
    }

    /// <summary>
    /// Converts a pure rotation matrix (no scale/shear) to a Quaternion.
    /// </summary>
    private static Quaternion ExtractRotation(Matrix4x4 m)
    {
        // Formula from: https://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/
        float w = Mathf.Sqrt(1f + m.m00 + m.m11 + m.m22) / 2f;
        float w4 = 4f * w;
        float x = (m.m21 - m.m12) / w4;
        float y = (m.m02 - m.m20) / w4;
        float z = (m.m10 - m.m01) / w4;
        return new Quaternion(x, y, z, w);
    }
}
