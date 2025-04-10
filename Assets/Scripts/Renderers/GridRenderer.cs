/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using System.Collections.Generic;
using static GlobalVariables;

public class GridRenderer : MonoBehaviour
{
    [Header("Meshes & Materials")]
    public Mesh gridCircleMesh;
    public Material gridCircleMaterial;

    private Matrix4x4[] _matrixBuffer = new Matrix4x4[BATCH_SIZE];

    void Update()
    {
        // Loop over each DNAGrid in your global dictionary
        foreach (DNAGrid grid in s_gridDict.Values)
        {
            List<Matrix4x4> circleMatrices = grid.GetCircleMatrices();

            DrawInstances(gridCircleMesh, gridCircleMaterial, circleMatrices);
        }
    }

    /// <summary>
    /// Draws the given list of instance matrices in batches using GPU instancing.
    /// </summary>
    private void DrawInstances(Mesh mesh, Material material, List<Matrix4x4> instanceMatrices)
    {
        int count = instanceMatrices.Count;
        if (count == 0) return;

        // In batches of up to 1023
        for (int i = 0; i < count; i += BATCH_SIZE)
        {
            int batchCount = Mathf.Min(BATCH_SIZE, count - i);

            // Copy the relevant slice of matrices into _matrixBuffer
            for (int j = 0; j < batchCount; j++)
            {
                _matrixBuffer[j] = instanceMatrices[i + j];
            }

            // Perform the actual instanced draw
            Graphics.DrawMeshInstanced(
                mesh,
                submeshIndex: 0,
                material,
                _matrixBuffer,
                batchCount
            );
        }
    }
}
