using System;
using UnityEngine;

public static class SplineInterpolation
{
    private const float INTERMIDIATE_FACTOR = 0.5f;

    /// <summary>
    /// https://cs184.eecs.berkeley.edu/sp24/lecture/7-35/bezier-curves-and-surfaces
    /// </summary>
    /// <param name="p0">First Point.</param>
    /// <param name="p1">Second Point.</param>
    /// <param name="p2">First Point Gradient.</param>
    /// <param name="p3">Second Point Gradient.</param>
    /// <param name="t">t</param>
    /// <returns>Interpolated Point.</returns>
	private static Vector3 CubicHermiteInterpolation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        // Cubic Hermite basic functions
        float H_0t = 2 * t3 - 3 * t2 + 1;
        float H_1t = -2 * t3 + 3 * t2;
        float H_2t = t3 - 2 * t2 + t;
        float H_3t = t3 - t2;

        return p0 * H_0t + p1 * H_1t + p2 * H_2t + p3 * H_3t;
        // return 0.5f * ((2f * p1) + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    private static Vector3 BezierCurveInterpolation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        // Bezier Curve basic functions
        float H_0t = (float) Math.Pow(1 - t, 3);
        float H_1t = 3 * ((float) Math.Pow(1 - t, 2)) * t;
        float H_2t = 3 * (1 - t) * t2;
        float H_3t = t3;

        return p0 * H_0t + p1 * H_1t + p2 * H_2t + p3 * H_3t;
    }

    /// <summary>
    /// https://cs184.eecs.berkeley.edu/sp24/lecture/7-43/bezier-curves-and-surfaces
    /// </summary>
    private static Vector3 CatmollRomGradientEstimation(Vector3 leftAdjacent, Vector3 rightAdjacent)
    {
        return (rightAdjacent - leftAdjacent) * 0.5f;
    }

    /// <summary>
    /// https://cs184.eecs.berkeley.edu/sp24/lecture/7-43/bezier-curves-and-surfaces
    /// </summary>
    /// <param name="startAdjacent">y0</param>
    /// <param name="start">y1</param>
    /// <param name="end">y2</param>
    /// <param name="endAdjacet">y3</param>
    /// <param name="t">t</param>
    /// <returns>Interpolated Point.</returns>
    private static Vector3 CatmollRomInterpolation(Vector3 startAdjacent, Vector3 start, Vector3 end, Vector3 endAdjacet, float t)
    {
        Vector3 startGradient = CatmollRomGradientEstimation(startAdjacent, end);
        Vector3 endGradient = CatmollRomGradientEstimation(start, endAdjacet);
        return CubicHermiteInterpolation(start, end, startGradient, endGradient, t);
    }

    /// <summary>
    /// Estimates a point at the endpoints for Catmoll Rom Interpolation.
    /// Note this assumes the nice ordering and positioning of nucleotides.
    /// </summary>
    /// <param name="first">First point (in order).</param>
    /// <param name="second">Second point (in order).</param>
    /// <param name="before">Whether to make the point before the first point or after the second point.</param>
    /// <returns>Estimated point.</returns>
    private static Vector3 EstimateAdjacentPoint(Vector3 first, Vector3 second, bool before)
    {
        Vector3 firstToSecond = second - first;
        if (before)
        {
            return first - firstToSecond;
        }
        return second + firstToSecond;
    }

    /// <summary>
    /// Uses Catmoll-Rom Interpolation to generate intermediate points between the all the given points.
    /// </summary>
    /// <param name="points">Points to generate new points between.</param>
    /// <returns>Intermiade points including the points inputted (order preserved).</returns>
    public static Vector3[] GenerateIntermediatePoints(Vector3[] points, int depth = 1)
    {
        if (points.Length == 1)
        {
            return points;
        }
        Vector3 startAdjacent = EstimateAdjacentPoint(points[0], points[1], true);
        Vector3 endAdjacent = EstimateAdjacentPoint(points[points.Length - 2], points[points.Length - 1], false);
        return GenerateIntermediatePoints(points, startAdjacent, endAdjacent, depth);
    }

    /// <summary>
    /// Uses Catmoll-Rom Interpolation to generate intermediate points between the all the given points.
    /// </summary>
    /// <param name="points">Points to generate new points between.</param>
    /// <param name="startAdjacent">Extra point to estimate start gradient.</param>
    /// <param name="endAdjacent">Extra point to estimate end gradient.</param>
    /// <returns>Intermiade points including the points inputted (order preserved).</returns>
    private static Vector3[] GenerateIntermediatePoints(Vector3[] points, Vector3 startAdjacent, Vector3 endAdjacent, int depth)
    {
        // Base case
        if (depth == 0)
        {
            return points;
        }

        Vector3[] newPoints = new Vector3[points.Length * 2 - 1];

        // Handle first point edge case
        newPoints[0] = points[0];
        newPoints[1] = CatmollRomInterpolation(startAdjacent, points[0], points[1], points[2], INTERMIDIATE_FACTOR);

        // Handle middle points
        for (int i = 1; i < points.Length - 2; i++)
        {
            newPoints[i * 2] = points[i];
            newPoints[i * 2 + 1] = CatmollRomInterpolation(points[i - 1], points[i], points[i + 1], points[i + 2], INTERMIDIATE_FACTOR);
        }

        // Handle last point edge case
        newPoints[newPoints.Length - 3] = points[points.Length - 2];
        newPoints[newPoints.Length - 2] = CatmollRomInterpolation(points[points.Length - 3], points[points.Length - 2], points[points.Length - 1], endAdjacent, INTERMIDIATE_FACTOR);
        newPoints[newPoints.Length - 1] = points[points.Length - 1];

        Vector3 newStartAdjacent = EstimateAdjacentPoint(newPoints[0], newPoints[1], true);
        Vector3 newEndAdjacent = EstimateAdjacentPoint(newPoints[newPoints.Length - 2], newPoints[newPoints.Length - 1], false);

        return GenerateIntermediatePoints(newPoints, newStartAdjacent, newEndAdjacent, depth - 1);
    }

    public static Vector3[] GenerateIntermediatePointsBezier(Vector3[] points, int resolution = 1)
    {
        int totalCurves = (points.Length + 1) / 4;
        int newPointsPerCurve = (int)Math.Pow(2, resolution) - 1;
        int totalPoints = (totalCurves + 1) + newPointsPerCurve * totalCurves;
        Vector3[] newPoints = new Vector3[totalPoints];

        float tIncrement = 1 / (float)Math.Pow(2, resolution);

        for (int i = 0; i < totalCurves; i++)
        {
            Vector3 p0 = points[4 * i - i];
            Vector3 p1 = points[4 * i + 1 - i]; // Control point
            Vector3 p2 = points[4 * i + 2 - i]; // Control point
            Vector3 p3 = points[4 * i + 3 - i];

            // Interpolate points
            int startIndex = i * (newPointsPerCurve + 1);
            for (int j = 0; j <= newPointsPerCurve; j++)
            {
                newPoints[startIndex + j] = BezierCurveInterpolation(p0, p1, p2, p3, j * tIncrement);
            }
        }

        // Add last point edge case
        newPoints[newPoints.Length - 1] = points[points.Length - 1];

        return newPoints;
    }
}

