/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using System.Collections.Generic;

public class HelixManager : MonoBehaviour
{
    [Header("Meshes & Materials")]
    public Mesh nucleotideMesh;           // Mesh for nucleotides (e.g., a sphere)
    public Material material;             // Material with GPU instancing enabled
    public Mesh backboneMesh;             // Mesh for backbones (e.g., a cylinder)

    // List of Helix instances (could be plain classes or MonoBehaviours)
    public List<Helix> helixInstances = new List<Helix>();

    void Update()
    {
        // Iterate over each helix and draw its instances
        foreach (Helix helix in GlobalVariables.s_helixDict.Values)
        {
            // Draw nucleotides for helix A and helix B.
            DrawInstances(nucleotideMesh, material, helix.NucleotideMatricesA);
            DrawInstances(nucleotideMesh, material, helix.NucleotideMatricesB);

            // Draw backbones for helix A and helix B.
            DrawInstances(backboneMesh, material, helix.BackboneMatricesA);
            DrawInstances(backboneMesh, material, helix.BackboneMatricesB);
        }
    }

    /// <summary>
    /// Draws instances using GPU instancing in batches of 1023.
    /// </summary>
    private void DrawInstances(Mesh mesh, Material material, List<Matrix4x4> matrices)
    {
        int count = matrices.Count;
        if (count == 0)
            return;

        const int batchSize = 1023;

        foreach (Matrix4x4 matrix in matrices)
        {
            Debug.Log($"Position: {matrix.m13}, {matrix.m23}, {matrix.m33}");
        }

        for (int i = 0; i < count; i += batchSize)
        {
            int batchCount = Mathf.Min(batchSize, count - i);
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices.GetRange(i, batchCount).ToArray());
        }
    }
}
