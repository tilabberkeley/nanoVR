/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class ReflectGrid : MonoBehaviour
{
    public void ReflectVerticalGlobal()
    {
        DNAGrid grid = SelectGrid.Grid;
        if (grid == null)
        {
            Debug.Log("Please select a grid first.");
            return;
        }

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

    public void ReflectHorizontalGlobal()
    {
        DNAGrid grid = SelectGrid.Grid;
        if (grid == null)
        {
            Debug.Log("Please select a grid firt.");
            return;
        }

        int maxXIndex = grid.GridXToIndex(grid.MaximumBound.X);
        int maxYIndex = grid.GridYToIndex(grid.MaximumBound.Y);
        Vector3 maxPosition = grid.Grid2D[maxXIndex, maxYIndex].transform.position;

        int minXIndex = grid.GridXToIndex(grid.MinimumBound.X);
        int minYIndex = grid.GridYToIndex(grid.MinimumBound.Y);
        Vector3 minPosition = grid.Grid2D[minXIndex, minYIndex].transform.position;

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

    public void ReflectVerticalLocal()
    {
        DNAGrid grid = SelectGrid.Grid;
        if (grid == null)
        {
            Debug.Log("Please select a grid first.");
            return;
        }

        Vector3 midPoint = CalculateMidPoint(grid);
        Vector3 gridLocalYAxis = grid.Grid2D[0, 0].transform.up;

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

                // Calculate the new world position by adding the reflected vector to the reflection point
                transform.position = midPoint + reflectedPosition;

                /*float distY = (transform.position.y - midY) * 2; // Multiply by 2 to get total distance needed to reflect
                transform.position = new Vector3(transform.position.x, transform.position.y - distY, transform.position.z);
                transform.localEulerAngles = new Vector3(-transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);*/
                //transform.Rotate(new Vector3(-transform.rotation.x * 2, 0, 0));
                grid.Grid2D[i, j].Helix?.ReflectVerticalLocal(midPoint, gridLocalYAxis);
            }
        }
    }

    public void ReflectHorizontalLocal()
    {
        DNAGrid grid = SelectGrid.Grid;
        if (grid == null)
        {
            Debug.Log("Please select a grid first.");
            return;
        }

        Vector3 midPoint = CalculateMidPoint(grid);
        Vector3 gridLocalXAxis = grid.Grid2D[0, 0].transform.right;

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

                // Calculate the new world position by adding the reflected vector to the reflection point
                transform.position = midPoint + reflectedPosition;

                /*float distY = (transform.position.y - midY) * 2; // Multiply by 2 to get total distance needed to reflect
                transform.position = new Vector3(transform.position.x, transform.position.y - distY, transform.position.z);
                transform.localEulerAngles = new Vector3(-transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);*/
                //transform.Rotate(new Vector3(-transform.rotation.x * 2, 0, 0));
                grid.Grid2D[i, j].Helix?.ReflectHorizontalLocal(midPoint, gridLocalXAxis);
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
}
