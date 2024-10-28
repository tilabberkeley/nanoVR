/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public class ReflectGrid : MonoBehaviour
{
    public void ReflectVerticalGlobal()
    {
        List<DNAGrid> grids = SelectGrid.Grids;
        if (grids.Count == 0)
        {
            Debug.Log("Please select a grid first.");
            return;
        }

        foreach (DNAGrid grid in grids)
        {
            float midY = CalculateMidPoint(grid).y;

            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid.Width; j++)
                {
                    Transform transform = grid.Grid2D[i, j].transform;
                    float distY = (transform.position.y - midY) * 2; // Multiply by 2 to get total distance needed to reflect
                    transform.position = new Vector3(transform.position.x, transform.position.y - distY, transform.position.z);
                    transform.localEulerAngles = new Vector3(-transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
                    //transform.Rotate(new Vector3(-transform.rotation.x * 2, 0, 0));
                    grid.Grid2D[i, j].Helix?.ReflectVectical(distY);
                }
            }
        }
    }

    public void ReflectHorizontalGlobal()
    {
        List<DNAGrid> grids = SelectGrid.Grids;
        if (grids.Count == 0)
        {
            Debug.Log("Please select a grid first.");
            return;
        }
        foreach (DNAGrid grid in grids)
        {
            float midX = CalculateMidPoint(grid).x;

            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid.Width; j++)
                {
                    Transform transform = grid.Grid2D[i, j].transform;
                    float distX = (transform.position.x - midX) * 2; // Multiply by 2 to get total distance needed to reflect
                    transform.position = new Vector3(transform.position.x - distX, transform.position.y, transform.position.z);
                    //transform.Rotate(new Vector3(0, -transform.localEulerAngles.y * 2, 0));
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, -transform.localEulerAngles.y, transform.localEulerAngles.z);
                    grid.Grid2D[i, j].Helix?.ReflectHorizontal(distX);
                }
            }
        }
    }

    public void ReflectVerticalLocal()
    {
        List<DNAGrid> grids = SelectGrid.Grids;
        if (grids.Count == 0)
        {
            Debug.Log("Please select a grid first.");
            return;
        }
        foreach (DNAGrid grid in grids)
        {
            Vector3 midPoint = CalculateMidPoint(grid);
            //Vector3 minPoint = grid.Grid2D[grid., 0].transform.up;
            //Vector3 distance = midPoint - grid.Grid2D[0, 0].transform.position;
            Vector3 gridLocalYAxis = CalculateMidAxis(grid, true);
            Debug.Log("Local y axis: " + gridLocalYAxis);

            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid.Width; j++)
                {
                    Transform transform = grid.Grid2D[i, j].transform;

                    // Get the object's current position relative to the reflection point
                    Vector3 objectPosition = transform.position;

                    // Calculate the vector from the reflection point to the object
                    Vector3 relativePosition = objectPosition - midPoint;

                    // Reflect the position vector around the grid's local Y-axis
                    Vector3 reflectedPosition = Vector3.Reflect(relativePosition, gridLocalYAxis);

                    Vector3 displacement = reflectedPosition - objectPosition;

                    // Calculate the new world position by adding the reflected vector to the reflection point
                    transform.position = reflectedPosition; // + midPoint;

                    /*float distY = (transform.position.y - midY) * 2; // Multiply by 2 to get total distance needed to reflect
                    transform.position = new Vector3(transform.position.x, transform.position.y - distY, transform.position.z);
                    transform.localEulerAngles = new Vector3(-transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);*/
                    //transform.Rotate(new Vector3(-transform.rotation.x * 2, 0, 0));
                    grid.Grid2D[i, j].Helix?.ReflectVerticalLocal(displacement);
                }
            }
        }
    }

    public void ReflectHorizontalLocal()
    {
        List<DNAGrid> grids = SelectGrid.Grids;
        if (grids.Count == 0)
        {
            Debug.Log("Please select a grid first.");
            return;
        }

        foreach (DNAGrid grid in grids)
        {
            Vector3 midPoint = CalculateMidPoint(grid);
            //Vector3 localXAxis = grid.Grid2D[0, 0].transform.right;
            //Vector3 distance = midPoint - grid.Grid2D[0, 0].transform.position;
            Vector3 gridLocalXAxis = CalculateMidAxis(grid, false);
            Debug.Log("Local x axis: " + gridLocalXAxis);

            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid.Width; j++)
                {
                    Transform transform = grid.Grid2D[i, j].transform;

                    // Get the object's current position relative to the reflection point
                    Vector3 objectPosition = transform.position;

                    // Calculate the vector from the reflection point to the object
                    Vector3 relativePosition = objectPosition - midPoint;

                    // Reflect the position vector around the grid's local Y-axis
                    Vector3 reflectedPosition = Vector3.Reflect(relativePosition, gridLocalXAxis);
                    Vector3 displacement = reflectedPosition - objectPosition;

                    // Calculate the new world position by adding the reflected vector to the reflection point
                    transform.position = reflectedPosition;

                    /*float distY = (transform.position.y - midY) * 2; // Multiply by 2 to get total distance needed to reflect
                    transform.position = new Vector3(transform.position.x, transform.position.y - distY, transform.position.z);
                    transform.localEulerAngles = new Vector3(-transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);*/
                    //transform.Rotate(new Vector3(-transform.rotation.x * 2, 0, 0));
                    grid.Grid2D[i, j].Helix?.ReflectHorizontalLocal(displacement);
                }
            }
        }
    }

    private Vector3 CalculateMidPoint(DNAGrid grid)
    {
        int maxXIndex = grid.GridXToIndex(grid.MaximumBound.X);
        int maxYIndex = grid.GridYToIndex(grid.MaximumBound.Y);
        Vector3 maxPosition = grid.Grid2D[maxXIndex, maxYIndex].transform.position;

        int minXIndex = grid.GridXToIndex(grid.MinimumBound.X);
        int minYIndex = grid.GridYToIndex(grid.MinimumBound.Y);
        Vector3 minPosition = grid.Grid2D[minXIndex, minYIndex].transform.position;
        
        return Vector3.Lerp(minPosition, maxPosition, 0.5f);
    }

    private Vector3 CalculateMidAxis(DNAGrid grid, bool verticalAxis)
    {
        int maxXIndex = grid.GridXToIndex(grid.MaximumBound.X);
        int maxYIndex = grid.GridYToIndex(grid.MaximumBound.Y);
        GridComponent maxGC = grid.Grid2D[maxXIndex, maxYIndex];

        int minXIndex = grid.GridXToIndex(grid.MinimumBound.X);
        int minYIndex = grid.GridYToIndex(grid.MinimumBound.Y);
        GridComponent minGC = grid.Grid2D[minXIndex, minYIndex];

        if (verticalAxis)
        {
            return Vector3.Lerp(maxGC.transform.up, minGC.transform.up, 0.5f);
        }
        return Vector3.Lerp(maxGC.transform.right, minGC.transform.right, 0.5f);
    }
}
