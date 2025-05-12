/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEditor;
using UnityEngine;

/// <summary>
/// Class to represent an AABB for each group of nucleotides in a helix.
/// A helix stores bounding box(es) for mesh collider assignment.
/// </summary>
public class HelixBoundingBox
{
    // The AABB corners in world space
    private Vector3 _boxMin;
    public Vector3 BoxMin { get => _boxMin; }

    private Vector3 _boxMax;
    public Vector3 BoxMax { get => _boxMax; }

    private Helix _helix;
    public Helix Helix { get => _helix; set => _helix = value; }

    /// <summary>
    /// Creates an "empty" bounding box that you can expand
    /// by calling Extend() with positions.
    /// </summary>
    public HelixBoundingBox()
    {
        Reset();
    }

    /// <summary>
    /// Reset to an extreme min/max so that subsequent Extend() calls can expand it properly.
    /// </summary>
    private void Reset()
    {
        _boxMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        _boxMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    }

    /// <summary>
    /// Expand the bounding box to include the given point.
    /// </summary>
    public void Extend(Vector3 point)
    {
        _boxMin = Vector3.Min(BoxMin, point);
        _boxMax = Vector3.Max(BoxMax, point);
    }

    /// <summary>
    /// True if a sphere (center + radius) intersects this helix’s bounding box,
    /// taking the helix’s current parent transform into account.
    /// </summary>
    public bool IntersectsSphere(Vector3 sphereCenter, float sphereRadius)
    {
        // 1  parent matrix (identity if the helix is still at its spawn position)
        Matrix4x4 m = _helix.CurrTransformOffset;

        // 2  local AABB centre/extents
        Vector3 localCenter = (BoxMin + BoxMax) * 0.5f;
        Vector3 localExtent = (BoxMax - BoxMin) * 0.5f;

        // 3  world-space centre
        Vector3 worldCenter = m.MultiplyPoint3x4(localCenter);

        // 4  world-space extents = |R| · localExtent  (R = rotational part of m)
        Vector3 e;
        e.x = Mathf.Abs(m.m00) * localExtent.x + Mathf.Abs(m.m01) * localExtent.y + Mathf.Abs(m.m02) * localExtent.z;
        e.y = Mathf.Abs(m.m10) * localExtent.x + Mathf.Abs(m.m11) * localExtent.y + Mathf.Abs(m.m12) * localExtent.z;
        e.z = Mathf.Abs(m.m20) * localExtent.x + Mathf.Abs(m.m21) * localExtent.y + Mathf.Abs(m.m22) * localExtent.z;

        Vector3 boxMin = worldCenter - e;
        Vector3 boxMax = worldCenter + e;

        // 5  sphere-vs-AABB test (squared distance)
        float distSqr = 0f;

        // X
        if (sphereCenter.x < boxMin.x) distSqr += (sphereCenter.x - boxMin.x) * (sphereCenter.x - boxMin.x);
        else if (sphereCenter.x > boxMax.x) distSqr += (sphereCenter.x - boxMax.x) * (sphereCenter.x - boxMax.x);

        // Y
        if (sphereCenter.y < boxMin.y) distSqr += (sphereCenter.y - boxMin.y) * (sphereCenter.y - boxMin.y);
        else if (sphereCenter.y > boxMax.y) distSqr += (sphereCenter.y - boxMax.y) * (sphereCenter.y - boxMax.y);

        // Z
        if (sphereCenter.z < boxMin.z) distSqr += (sphereCenter.z - boxMin.z) * (sphereCenter.z - boxMin.z);
        else if (sphereCenter.z > boxMax.z) distSqr += (sphereCenter.z - boxMax.z) * (sphereCenter.z - boxMax.z);

        return distSqr <= sphereRadius * sphereRadius;
    }
}
