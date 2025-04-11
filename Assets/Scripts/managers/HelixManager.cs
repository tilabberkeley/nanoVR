/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using System.Collections.Generic;

public class HelixManager : MonoBehaviour
{
    [Header("Meshes & Materials")]
    public Material material;             // Material with GPU instancing enabled
    public Mesh nucleotideMesh;           // Mesh for nucleotides
    public Mesh backboneMesh;             // Mesh for backbones
    public Mesh gcMesh;                   // Mesh for GridCircles

    // List of Helix instances (could be plain classes or MonoBehaviours)
    // public List<Helix> helixInstances = new List<Helix>();
    const int BATCH_SIZE = 1023;

    private Matrix4x4[] _matrixBuffer = new Matrix4x4[BATCH_SIZE];

    private MaterialPropertyBlock _mpb;
    private Vector4[] _colorBuffer;
    private Vector4[] _highlightBuffer;

    // Assume TransformHandle.Gizmos is the current gizmo GameObject.
    private Matrix4x4 _currentOffset;
    public Matrix4x4 CurrentOffset { get => _currentOffset; }

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _colorBuffer = new Vector4[BATCH_SIZE];
        _highlightBuffer = new Vector4[BATCH_SIZE];
    }

    void Update()
    {
        // Iterate over each helix and draw its instances with the current offset.
        foreach (Helix helix in GlobalVariables.s_helixDict.Values)
        {
            List<Color> nucleotideColorsA = helix.GetNucleotideColors(direction: 1);
            List<Color> nucleotideColorsB = helix.GetNucleotideColors(direction: 0);
            List<Color> backboneColorsA = helix.GetBackboneColors(direction: 1);
            List<Color> backboneColorsB = helix.GetBackboneColors(direction: 0);
            List<Color> nucleotideHighlightsA = helix.GetNucleotideHighlights(direction: 1);
            List<Color> nucleotideHighlightsB = helix.GetNucleotideHighlights(direction: 0);

            Matrix4x4 gizmosMatrix = Matrix4x4.TRS(
                                        TransformHandle.GizmosTransform.position,
                                        TransformHandle.GizmosTransform.rotation,
                                        Vector3.one);
            Matrix4x4 delta = gizmosMatrix * TransformHandle.InitialGizmoMatrix.inverse;

            _currentOffset = helix.OldTransformOffset;
            if (helix.IsTransforming) {
                _currentOffset = delta * _currentOffset;
                //Debug.Log("helix is transforming");
                helix.CurrTransformOffset = _currentOffset;

                // Update xover transform when helix is transforming
                helix.UpdateXovers();
            }
            
            // Apply the current gizmo transform as the offset.
            DrawInstances(nucleotideMesh, material, helix.NucleotideMatricesA, nucleotideColorsA, nucleotideHighlightsA, _currentOffset);
            DrawInstances(nucleotideMesh, material, helix.NucleotideMatricesB, nucleotideColorsB, nucleotideHighlightsB, _currentOffset);

            DrawInstances(backboneMesh, material, helix.BackboneMatricesA, backboneColorsA, backboneColorsA, _currentOffset); // No need to highlight backbones
            DrawInstances(backboneMesh, material, helix.BackboneMatricesB, backboneColorsB, backboneColorsB, _currentOffset); // No need to highlight backbones
        }
    }

    /// <summary>
    /// Draws instances using GPU instancing in batches of 1023, applying the given parent offset.
    /// </summary>
    /// <param name="mesh">Mesh to draw.</param>
    /// <param name="material">Material with instancing enabled.</param>
    /// <param name="matrices">Local instance matrices.</param>
    /// <param name="colors">Per-instance colors.</param>
    /// <param name="parentOffset">Parent transform offset to apply.</param>
    private void DrawInstances(Mesh mesh, Material material, List<Matrix4x4> matrices, List<Color> colors, List<Color> highlightColors, Matrix4x4 parentOffset)
    {
        int count = matrices.Count;
        if (count == 0)
            return;
        if (colors.Count < count)
            Debug.Log("Colors list shorter than matrix list " + colors.Count + " " + matrices.Count);
        
        if (highlightColors.Count < count)
            Debug.Log("Highlights list shorter than matrix list " + highlightColors.Count);

        for (int i = 0; i < count; i += BATCH_SIZE)
        {
            int batchCount = Mathf.Min(BATCH_SIZE, count - i);
            for (int j = 0; j < batchCount; j++)
            {
                // Multiply each local matrix by the parent offset.
                _matrixBuffer[j] = parentOffset * matrices[i + j];
                _colorBuffer[j] = i + j < colors.Count ? colors[i + j] : Color.white;
                _highlightBuffer[j] = i + j < highlightColors.Count ? highlightColors[i + j] : Color.white;
            }
            _mpb.SetVectorArray("_Color", _colorBuffer);
            _mpb.SetVectorArray("_HighlightColor", _highlightBuffer);
            Graphics.DrawMeshInstanced(mesh, 0, material, _matrixBuffer, batchCount, _mpb);
        }
    }
}