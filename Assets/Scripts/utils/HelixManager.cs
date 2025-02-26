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
    const int BATCH_SIZE = 1023;

    private Matrix4x4[] _matrixBuffer = new Matrix4x4[BATCH_SIZE];


    void Update()
    {
        // Iterate over each helix and draw its instances
        foreach (Helix helix in GlobalVariables.s_helixDict.Values)
        {
            // Draw nucleotides for helix A and helix B.

            //List<Matrix4x4> helixA = new List<Matrix4x4>();
            //helixA.AddRange(helix.NucleotideMatricesA);
            //helixA.AddRange(helix.BackboneMatricesA);

            //List<Matrix4x4> helixB = new List<Matrix4x4>();
            //helixA.AddRange(helix.NucleotideMatricesB);
            //helixA.AddRange(helix.BackboneMatricesB);

            //DrawInstances(nucleotideMesh, material, helixA);
            //DrawInstances(nucleotideMesh, material, helixB);

            List<Color> nucleotideColorsA = helix.GetNucleotideColors(direction: 1);
            List<Color> nucleotideColorsB = helix.GetNucleotideColors(direction: 0);
            List<Color> backboneColorsA = helix.GetBackboneColors(direction: 1);
            List<Color> backboneColorsB = helix.GetBackboneColors(direction: 0);


            DrawInstances(nucleotideMesh, material, helix.NucleotideMatricesA, nucleotideColorsA);
            DrawInstances(nucleotideMesh, material, helix.NucleotideMatricesB, nucleotideColorsB);

            // Draw backbones for helix A and helix B.
            DrawInstances(backboneMesh, material, helix.BackboneMatricesA, backboneColorsA);
            DrawInstances(backboneMesh, material, helix.BackboneMatricesB, backboneColorsB);
        }
    }

    /// <summary>
    /// Draws instances using GPU instancing in batches of 1023.
    /// </summary>
    private void DrawInstances(Mesh mesh, Material material, List<Matrix4x4> matrices, List<Color> colors)
    {
        int count = matrices.Count;
        if (count == 0)
            return;

        // Create a MaterialPropertyBlock that will carry our per-instance colors.
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        // Temporary arrays for a batch.
        Vector4[] colorBuffer = new Vector4[BATCH_SIZE];

        for (int i = 0; i < count; i += BATCH_SIZE)
        {
            int batchCount = Mathf.Min(BATCH_SIZE, count - i);

            for (int j = 0; j < batchCount; j++)
            {
                _matrixBuffer[j] = matrices[i + j];
                // Convert Color to Vector4 (Color implicitly converts to Vector4)
                colorBuffer[j] = colors[i + j];
            }
            // Set the per-instance color array (property name "_Color" must match your shader)
            mpb.SetVectorArray("_Color", colorBuffer);

            // Draw the batch with the MaterialPropertyBlock.
            Graphics.DrawMeshInstanced(mesh, 0, material, _matrixBuffer, batchCount, mpb);
        }
    }

}
