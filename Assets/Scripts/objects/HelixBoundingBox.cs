/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
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
    /// Check if a sphere (center + radius) intersects (collides with) this axis-aligned box.
    /// </summary>
    /// <param name="sphereCenter">World position of sphere center</param>
    /// <param name="sphereRadius">Radius of the sphere</param>
    /// <returns>True if there's any intersection, false otherwise.</returns>
    public bool IntersectsSphere(Vector3 sphereCenter, float sphereRadius)
    {
        float distSqr = 0f;

        // X-axis
        if (sphereCenter.x < BoxMin.x) distSqr += (sphereCenter.x - BoxMin.x) * (sphereCenter.x - BoxMin.x);
        else if (sphereCenter.x > BoxMax.x) distSqr += (sphereCenter.x - BoxMax.x) * (sphereCenter.x - BoxMax.x);

        // Y-axis
        if (sphereCenter.y < BoxMin.y) distSqr += (sphereCenter.y - BoxMin.y) * (sphereCenter.y - BoxMin.y);
        else if (sphereCenter.y > BoxMax.y) distSqr += (sphereCenter.y - BoxMax.y) * (sphereCenter.y - BoxMax.y);

        // Z-axis
        if (sphereCenter.z < BoxMin.z) distSqr += (sphereCenter.z - BoxMin.z) * (sphereCenter.z - BoxMin.z);
        else if (sphereCenter.z > BoxMax.z) distSqr += (sphereCenter.z - BoxMax.z) * (sphereCenter.z - BoxMax.z);

        return distSqr <= sphereRadius * sphereRadius;
    }

    public bool IntersectRay(Ray ray, float maxDistance, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;

        Vector3 invDir = new Vector3(
            1.0f / ray.direction.x,
            1.0f / ray.direction.y,
            1.0f / ray.direction.z
        );

        Vector3 tMin = new Vector3(
            (_boxMin.x - ray.origin.x) * invDir.x,
            (_boxMin.y - ray.origin.y) * invDir.y,
            (_boxMin.z - ray.origin.z) * invDir.z
        );

        Vector3 tMax = new Vector3(
            (_boxMax.x - ray.origin.x) * invDir.x,
            (_boxMax.y - ray.origin.y) * invDir.y,
            (_boxMax.z - ray.origin.z) * invDir.z
        );

        float t1 = Mathf.Min(tMin.x, tMax.x);
        float t2 = Mathf.Max(tMin.x, tMax.x);
        float t3 = Mathf.Min(tMin.y, tMax.y);
        float t4 = Mathf.Max(tMin.y, tMax.y);
        float t5 = Mathf.Min(tMin.z, tMax.z);
        float t6 = Mathf.Max(tMin.z, tMax.z);

        float tNear = Mathf.Max(Mathf.Max(t1, t3), t5);
        float tFar = Mathf.Min(Mathf.Min(t2, t4), t6);

        if (tNear <= tFar && tFar >= 0f && tNear <= maxDistance)
        {
            hitPoint = ray.origin + ray.direction * tNear;
            return true;
        }

        return false;
    }

}
