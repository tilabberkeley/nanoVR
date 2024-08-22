using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using UnityEngine;
using static GlobalVariables;

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

    /// <summary>
    /// https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    /// Cubic Bezier curves section. Variables are the same as provided.
    /// </summary>
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

    /// <summary>
    /// Returns the points of a bezier spline.
    /// </summary>
    /// <param name="dnaComponents">Initial points to generate the spline.</param>
    /// <param name="resolution">How smooth the spline is. Increase exponentially.</param>
    /// <returns>3D points of the bezier spline.</returns>
    public static Vector3[] GenerateBezierSpline(List<DNAComponent> dnaComponents, int resolution = 1)
    {
        int excessPoints = (dnaComponents.Count - 4) % 3;
        int pointsToAdd = (3 - excessPoints) % 3;
        int splineLength = dnaComponents.Count + pointsToAdd;

        Vector3[] points;

        // Spline very small edge case
        if (dnaComponents.Count <= 4)
        {
            points = HandleSmallSplineEdgecase(dnaComponents, resolution);
        }
        else
        {
            points = SetSplinePoints(dnaComponents, pointsToAdd, splineLength);
        }

        int totalCurves = (splineLength - 4) / 3 + 1;
        int newPointsPerCurve = 2; // (int)Math.Pow(2, resolution) - 1;
        int totalPoints = (totalCurves + 1) + newPointsPerCurve * totalCurves;
        Vector3[] interpolatedPoints = new Vector3[totalPoints];

        float tIncrement = 1.0f / 3.0f; // (float)Math.Pow(2, resolution);

        for (int i = 0; i < totalCurves; i++)
        {
            Vector3 p0 = points[4 * i - i];
            Vector3 p1 = points[4 * i + 1 - i]; // Control point
            Vector3 p2 = points[4 * i + 2 - i]; // Control point
            Vector3 p3 = points[4 * i + 3 - i];
            
            if (i > 0 && i < totalCurves - 1)
            {
                // Set endpoints to be the mid points of adjacent control points.
                Vector3 pMinus1 = points[4 * i - 1 - i]; // adjacent control point.
                p0 = (pMinus1 + p1) / 2;
                Vector3 p4 = points[4 * i + 4 - i]; // adjace control point,
                p3 = (p2 + p4) / 2;
            }

            // Interpolate points
            int startIndex = i * (newPointsPerCurve + 1);
            for (int j = 0; j <= newPointsPerCurve; j++)
            {
                interpolatedPoints[startIndex + j] = BezierCurveInterpolation(p0, p1, p2, p3, j * tIncrement);
            }
        }

        // Add last point edge case
        interpolatedPoints[interpolatedPoints.Length - 1] = points[points.Length - 1];

        if (pointsToAdd > 0 && dnaComponents.Count > 4)
        {
            interpolatedPoints = CutoffExcessPoints(interpolatedPoints, pointsToAdd, newPointsPerCurve);
        }

        return interpolatedPoints;
    }

    /// <summary>
    /// Generates the positions of the previous and next nucleotides and backbones in a helix for the given nucleotide.
    /// </summary>
    private static void GetAdjacentPositions(
        NucleotideComponent nucleotideComponent,
        out Vector3 prevNucleotidePosition,
        out Vector3 prevBackbonePosition,
        out Vector3 nextNuceotidePosition,
        out Vector3 nextBackbonePosition)
    {
        s_helixDict.TryGetValue(nucleotideComponent.HelixId, out Helix helix);
        int direction = nucleotideComponent.Direction;
        int index = nucleotideComponent.Id;
        Vector3 currentPosition = nucleotideComponent.gameObject.transform.position;

        helix.CalculateNextNucleotidePositions(index + 1, out Vector3 nextPositionA, out Vector3 nextPositionB);
        helix.CalculateNextNucleotidePositions(index - 1, out Vector3 prevPositionA, out Vector3 prevPositionB);

        // TODO: Fix this for rotations
        if (direction == 1) // 1 corresponds to position A - sorry for the magic numbers. Prob want an enum eventually.
        {
            nextNuceotidePosition = nextPositionA;
            prevNucleotidePosition = prevPositionA;
        }
        else // With the other direction, the positioning gets swapped.
        {
            nextNuceotidePosition = prevPositionB;
            prevNucleotidePosition = nextPositionB;
        }

        // Calculate where the back bone would be - in between the nucleotides.
        nextBackbonePosition = (nextNuceotidePosition + currentPosition) / 2.0f;
        prevBackbonePosition = (prevNucleotidePosition + currentPosition) / 2.0f;
    }

    /// <summary>
    /// Sets the bezier spline points, extending if necessary.
    /// </summary>
    private static Vector3[] SetSplinePoints(List<DNAComponent> dnaComponents, int pointsToAdd, int splineLength)
    {
        Vector3[] points = new Vector3[splineLength];

        // Add already existing nucleotide locations
        for (int i = 0; i < dnaComponents.Count; i += 1)
        {
            points[i] = dnaComponents[i].transform.position;
        }

        Vector3 prevNucleotidePosition;
        Vector3 prevBackbonePosition;
        Vector3 nextNuceotidePosition;
        Vector3 nextBackbonePosition;

        /* For the first curve, we need the position of the dnaComponent one before for a smooth spline.
         * So adjust the first point to be the midpoint of adjacent control points
         * This is needed for very large domains that have multiple bezier curves. If they're not adjusted,
         * the concatenation of the splines won't be smooth because the splines are adjusting to
         * midpoints as well as seen in GenerateIntermediatePointsBezier.
         */
        NucleotideComponent firstNucleotideComponent = (NucleotideComponent)dnaComponents[0];
        GetAdjacentPositions(
            firstNucleotideComponent,
            out prevNucleotidePosition,
            out prevBackbonePosition,
            out nextNuceotidePosition,
            out nextBackbonePosition);

        points[0] = (prevBackbonePosition + nextBackbonePosition) / 2.0f;

        if (pointsToAdd == 0)
        {
            /* For the same reason as listed above we need to adjust the endpoint to be a mid point
             * for spline concatenation. To avoid complexity, this was only implemented when the spline doesn't
             * need to be extend. It is crucial the DomainComponent.BEZIER_COUNT is some multiple of 3 + 4 and odd
             * for this to work properly. Otherwise the splines that get concatenated are extended, and to avoid 
             * that case, BEZIER_COUNT needs to be as described. It must be odd to avoid casting errors.
             * 
             * This isn't as crucial for the other strands that actually get extended because the slight differences 
             * in the the last point's position being adjusted to the midpoint isn't noticable when splines aren't concatenated.
             */
            NucleotideComponent lastNucleotideComponent = (NucleotideComponent)dnaComponents[dnaComponents.Count - 1];
            GetAdjacentPositions(
                lastNucleotideComponent,
                out prevNucleotidePosition,
                out prevBackbonePosition,
                out nextNuceotidePosition,
                out nextBackbonePosition);

            points[splineLength - 1] = (prevBackbonePosition + nextBackbonePosition) / 2.0f;

            return points;
        }

        // We need to extend, so get last nucleotide component.
        NucleotideComponent nucleotideComponent = (NucleotideComponent)dnaComponents[dnaComponents.Count - 1];
        GetAdjacentPositions(
            nucleotideComponent,
            out prevNucleotidePosition,
            out prevBackbonePosition,
            out nextNuceotidePosition,
            out nextBackbonePosition);

        // Either have to add one or two extra points to have a complete spline.
        if (pointsToAdd == 1)
        {
            points[points.Length - 1] = nextBackbonePosition;
        }
        else 
        {
            points[points.Length - 2] = nextBackbonePosition;
            points[points.Length - 1] = nextNuceotidePosition;
        }

        return points;
    }

    private static Vector3[] HandleSmallSplineEdgecase(List<DNAComponent> dnaComponents, int resolution)
    {
        Vector3[] points = new Vector3[dnaComponents.Count + 1];

        // There will either 1 or three nucleotids
        if (dnaComponents.Count == 1)
        {
            // TODO
            return points;
        }

        // Add already existing nucleotide locations
        for (int i = 0; i < dnaComponents.Count; i += 1)
        {
            points[i] = dnaComponents[i].transform.position;
        }

        /* For the first curve, we need the position of the dnaComponent one before for a smooth spline.
         * So adjust the first point to be the midpoint of adjacent control points
         * This is needed for very large domains that have multiple bezier curves. If they're not adjusted,
         * the concatenation of the splines won't be smooth because the splines are adjusting to
         * midpoints as well as seen in GenerateIntermediatePointsBezier.
         */
        NucleotideComponent firstNucleotideComponent = (NucleotideComponent)dnaComponents[0];
        GetAdjacentPositions(
            firstNucleotideComponent,
            out Vector3 prevNucleotidePosition,
            out Vector3 prevBackbonePosition,
            out Vector3 nextNuceotidePosition,
            out Vector3 nextBackbonePosition);

        points[0] = (prevBackbonePosition + nextBackbonePosition) / 2.0f;

        // Last DNA component will be a nucleotide 
        NucleotideComponent lastNucleotide = (NucleotideComponent) dnaComponents[dnaComponents.Count - 1];
        GetAdjacentPositions(
            lastNucleotide,
            out prevNucleotidePosition,
            out prevBackbonePosition,
            out nextNuceotidePosition,
            out nextBackbonePosition);

        // Extend spline to be four points total, using the next backbone.
        points[points.Length - 1] = nextBackbonePosition;

        return points;
    }

    private const float CUTOFF_FACTOR = 0.33f;

    /// <summary>
    /// Removes excess points generated from spline extension.
    /// </summary>
    private static Vector3[] CutoffExcessPoints(Vector3[] points, int pointsToAdd, int newPointsPerCurve)
    {
        int numPointsToCutoff = (int)Math.Round(CUTOFF_FACTOR * pointsToAdd * newPointsPerCurve);
        return points.Take(points.Length - numPointsToCutoff).ToArray();
    }
}

