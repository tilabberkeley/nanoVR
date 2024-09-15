using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

        int maxXIndex = grid.GridXToIndex(grid.MaximumBound.X);
        int maxYIndex = grid.GridYToIndex(grid.MaximumBound.Y);
        Debug.Log("Max X index: " + maxXIndex + ", Max Y index: " +  maxYIndex);
        Vector3 maxPosition = grid.Grid2D[maxXIndex, maxYIndex].transform.position;

        int minXIndex = grid.GridXToIndex(grid.MinimumBound.X);
        int minYIndex = grid.GridYToIndex(grid.MinimumBound.Y);
        Debug.Log("Min X index: " + minXIndex + ", Min Y index: " + minYIndex);

        Vector3 minPosition = grid.Grid2D[minXIndex, minYIndex].transform.position;

        float midY = Vector3.Lerp(minPosition, maxPosition, 0.5f).y;
        Debug.Log("midY: " + midY);

        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                Transform transform = grid.Grid2D[i, j].transform;
                float distY = (transform.position.y - midY) * 2; // Multiply by 2 to get total distance needed to reflect
                transform.position = new Vector3(transform.position.x, transform.position.y - distY, transform.position.z);
                //transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, -transform.localEulerAngles.y, transform.localEulerAngles.z);
                transform.Rotate(new Vector3(0, -transform.localEulerAngles.y * 2, 0));
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

        float midX = Vector3.Lerp(minPosition, maxPosition, 0.5f).x;


        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Width; j++)
            {
                Transform transform = grid.Grid2D[i, j].transform;
                float distX = (transform.position.x - midX) * 2; // Multiply by 2 to get total distance needed to reflect
                transform.position = new Vector3(transform.position.x - distX, transform.position.y, transform.position.z);
                transform.Rotate(new Vector3(-transform.localEulerAngles.x * 2, 0, 0));
                grid.Grid2D[i, j].Helix?.ReflectHorizontal(distX);
            }
        }
    }

    public void ReflectVerticalLocal()
    {
        DNAGrid grid = SelectGrid.Grid;
        TransformHandle.Instance.AttachChildren(grid);

        int maxXIndex = grid.GridXToIndex(grid.MaximumBound.X);
        int maxYIndex = grid.GridXToIndex(grid.MaximumBound.Y);
        Transform maxTransform = grid.Grid2D[maxXIndex, maxYIndex].transform;

        TransformHandle.Instance.gizmos.transform.SetPositionAndRotation(maxTransform.position, maxTransform.rotation);

        TransformHandle.Instance.DetachChildren(grid);
    }

    /*public void ReflectHorizontalLocal()
    {
        DNAGrid grid = SelectGrid.Grid;
        TransformHandle.Instance.AttachChildren(grid);
        int bottomRight

        int minXIndex = grid.GridXToIndex(grid.MinimumBound.X);
        int minYIndex = grid.GridYToIndex(grid.MinimumBound.Y);
        Transform maxTransform = grid.Grid2D[maxXIndex, maxYIndex].transform;

        TransformHandle.Instance.gizmos.transform.SetPositionAndRotation(maxTransform.position, maxTransform.rotation);

        TransformHandle.Instance.DetachChildren(grid);
    }*/
}
