/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using System.Linq;

/// <summary>
/// Manages rendering of DNAGrid helices in the nucleotide view.
/// </summary>
public class HelixManager : MonoBehaviour
{
    [Header("Meshes & Materials")]
    public Material material;
    public Mesh nucleotideMesh;
    public Mesh backboneMesh;
    public Mesh gcMesh;

    const int MATRIX_STRIDE = sizeof(float) * 4 * 4;
    const int COLOR_STRIDE = sizeof(float) * 4;
    const int HIGHLIGHT_STRIDE = sizeof(float) * 4;
    const int ARGS_STRIDE_BYTES = 5 * sizeof(uint);

    ComputeBuffer _nuclMatCB, _nuclColCB, _nuclHilCB, _nuclArgsCB;
    ComputeBuffer _backMatCB, _backColCB, _backHilCB, _backArgsCB;
    MaterialPropertyBlock _mpb;
    private Bounds bounds;

    // Tracks whether or not helices need to update their xovers when transforming
    Dictionary<DNAGrid, bool> updateXoverMap;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        bounds = new Bounds(Vector3.zero, Vector3.one * 100000f);

        _nuclArgsCB = new ComputeBuffer(1, ARGS_STRIDE_BYTES, ComputeBufferType.IndirectArguments);
        _backArgsCB = new ComputeBuffer(1, ARGS_STRIDE_BYTES, ComputeBufferType.IndirectArguments);

        updateXoverMap = new Dictionary<DNAGrid, bool>();
    }

    void Update()
    {
        /*if (!GlobalVariables.s_nucleotideView)
        {
            return;
        }*/

        // Do not render helices when running simulation
        // OxViewManager will render oxView structures instead.
        if (GlobalVariables.s_simulating)
        {
            return;
        }

        int nucCount = 0, backCount = 0;
        foreach (Helix h in GlobalVariables.s_helixDict.Values)
        {
            foreach (Extension ext in h.Extensions)
            {
                nucCount += ext.Nucleotides.Count;
                backCount += ext.Backbones.Count;
            }

            // Don't render helices in helix view; however, we still need to render non-helix-bound extensions above.
            if (!h.IsHelixView)
            {
                nucCount += h.NucleotideMatricesA.Count + h.NucleotideMatricesB.Count;
                backCount += h.BackboneMatricesA.Count + h.BackboneMatricesB.Count;
            } 
        }

        if (nucCount == 0 && backCount == 0) return;

        EnsureBuffer(ref _nuclMatCB, nucCount, MATRIX_STRIDE);
        EnsureBuffer(ref _nuclColCB, nucCount, COLOR_STRIDE);
        EnsureBuffer(ref _nuclHilCB, nucCount, HIGHLIGHT_STRIDE);

        EnsureBuffer(ref _backMatCB, backCount, MATRIX_STRIDE);
        EnsureBuffer(ref _backColCB, backCount, COLOR_STRIDE);
        EnsureBuffer(ref _backHilCB, backCount, HIGHLIGHT_STRIDE);

        var localMats = new NativeArray<float4x4>(nucCount, Allocator.TempJob);
        var colours = new NativeArray<float4>(nucCount, Allocator.TempJob);
        var highlights = new NativeArray<float4>(nucCount, Allocator.TempJob);

        var bLocal = new NativeArray<float4x4>(backCount, Allocator.TempJob);
        var bColor = new NativeArray<float4>(backCount, Allocator.TempJob);
        var bHighlight = new NativeArray<float4>(backCount, Allocator.TempJob);

        int nIdx = 0, bIdx = 0;

        foreach (DNAGrid g in GlobalVariables.s_gridDict.Values)
        {
            bool updateXovers = UpdateCurrentOffset(g);
            updateXoverMap[g] = updateXovers;
        }

        foreach (Helix h in GlobalVariables.s_helixDict.Values)
        {
            Matrix4x4 offset = h.GetCurrentOffset();

            List<Matrix4x4> extensionNucMats = new List<Matrix4x4>();
            List<Color> extensionNucColors = new List<Color>();
            List<Color> extensionNucHighlights = new List<Color>();
            List<Matrix4x4> extensionBackboneMats = new List<Matrix4x4>();
            List<Color> extensionBackboneColors = new List<Color>();

            foreach (Extension ext in h.Extensions)
            {
                extensionNucMats.AddRange(ext.Nucleotides);
                extensionNucColors.AddRange(ext.GetNucleotideColors());
                extensionNucHighlights.AddRange(ext.GetNucleotideHighlights());
                extensionBackboneMats.AddRange(ext.Backbones);
                extensionBackboneColors.AddRange(ext.GetBackboneColors());
            }

            FillNativeArrays(extensionNucMats,
                 extensionNucColors,
                 extensionNucHighlights,
                 localMats, colours, highlights, ref nIdx, offset);

            FillNativeArrays(extensionBackboneMats,
                             extensionBackboneColors,
                             null, // if you don't track highlight for backbone
                             bLocal, bColor, bHighlight, ref bIdx, offset);

            if (updateXoverMap[h.GetGrid()])
            {
                h.UpdateXovers();
            }

            if (!h.IsHelixView)
            {
                FillNativeArrays(h.NucleotideMatricesA, h.GetNucleotideColors(1),
                             h.GetNucleotideHighlights(1), localMats, colours, highlights, ref nIdx, offset);

                FillNativeArrays(h.NucleotideMatricesB, h.GetNucleotideColors(0),
                                 h.GetNucleotideHighlights(0), localMats, colours, highlights, ref nIdx, offset);

                FillNativeArrays(h.BackboneMatricesA, h.GetBackboneColors(1), null,
                                 bLocal, bColor, bHighlight, ref bIdx, offset);

                FillNativeArrays(h.BackboneMatricesB, h.GetBackboneColors(0), null,
                                 bLocal, bColor, bHighlight, ref bIdx, offset);
            }
        }

        var gpuMats = _nuclMatCB.BeginWrite<float4x4>(0, nucCount);
        var gpuCols = _nuclColCB.BeginWrite<float4>(0, nucCount);
        var gpuHls = _nuclHilCB.BeginWrite<float4>(0, nucCount);

        var nucJob = new CopyJob
        {
            local = localMats,
            col = colours,
            hlt = highlights,
            outM = gpuMats,
            outC = gpuCols,
            outH = gpuHls
        };
        nucJob.Schedule(nucCount, 64).Complete();

        _nuclMatCB.EndWrite<float4x4>(nucCount);
        _nuclColCB.EndWrite<float4>(nucCount);
        _nuclHilCB.EndWrite<float4>(nucCount);

        localMats.Dispose(); colours.Dispose(); highlights.Dispose();

        var gpuBackM = _backMatCB.BeginWrite<float4x4>(0, backCount);
        var gpuBackC = _backColCB.BeginWrite<float4>(0, backCount);
        var gpuBackH = _backHilCB.BeginWrite<float4>(0, backCount);

        var backJob = new CopyJob
        {
            local = bLocal,
            col = bColor,
            hlt = bHighlight,
            outM = gpuBackM,
            outC = gpuBackC,
            outH = gpuBackH
        };
        backJob.Schedule(backCount, 64).Complete();

        _backMatCB.EndWrite<float4x4>(backCount);
        _backColCB.EndWrite<float4>(backCount);
        _backHilCB.EndWrite<float4>(backCount);

        bLocal.Dispose(); bColor.Dispose(); bHighlight.Dispose();

        DrawBatch(nucleotideMesh, nucCount,
                  _nuclMatCB, _nuclColCB, _nuclHilCB, _nuclArgsCB);

        DrawBatch(backboneMesh, backCount,
                  _backMatCB, _backColCB, _backHilCB, _backArgsCB);
    }

    void OnDestroy()
    {
        _nuclMatCB?.Release(); _nuclColCB?.Release(); _nuclHilCB?.Release();
        _backMatCB?.Release(); _backColCB?.Release(); _backHilCB?.Release();
        _nuclArgsCB?.Release(); _backArgsCB?.Release();
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    struct CopyJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float4x4> local;
        [ReadOnly] public NativeArray<float4> col, hlt;
        [WriteOnly] public NativeArray<float4x4> outM;
        [WriteOnly] public NativeArray<float4> outC, outH;

        public void Execute(int i)
        {
            outM[i] = local[i];
            outC[i] = col[i];
            outH[i] = hlt[i];
        }
    }

    static void FillNativeArrays(
        List<Matrix4x4> srcMats, List<Color> srcCols, List<Color> srcHls,
        NativeArray<float4x4> dstLocal, NativeArray<float4> dstCol, NativeArray<float4> dstHlt,
        ref int idx, Matrix4x4 offset)
    {
        bool hasHlt = srcHls != null;
        for (int i = 0; i < srcMats.Count; ++i, ++idx)
        {    
            dstLocal[idx] = math.mul(offset, srcMats[i]);
            dstCol[idx] = new float4(srcCols[i].r, srcCols[i].g, srcCols[i].b, srcCols[i].a);
            dstHlt[idx] = hasHlt
                ? new float4(srcHls[i].r, srcHls[i].g, srcHls[i].b, srcHls[i].a)
                : new float4(srcCols[i].r, srcCols[i].g, srcCols[i].b, srcCols[i].a);
        }
    }

    void DrawBatch(Mesh mesh, int instanceCount,
                   ComputeBuffer matCB, ComputeBuffer colCB,
                   ComputeBuffer hltCB, ComputeBuffer argsCB)
    {
        if (instanceCount == 0) return;

        uint[] args = {
            (uint)mesh.GetIndexCount(0),
            (uint)instanceCount,
            (uint)mesh.GetIndexStart(0),
            (uint)mesh.GetBaseVertex(0),
            0
        };
        argsCB.SetData(args);

        _mpb.Clear();
        _mpb.SetBuffer("_Matrices", matCB);
        _mpb.SetBuffer("_Colors", colCB);
        _mpb.SetBuffer("_Highlights", hltCB);

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsCB, 0, _mpb);
    }

    ComputeBuffer EnsureBuffer(ref ComputeBuffer cb, int count, int stride)
    {
        const ComputeBufferType TYPE = ComputeBufferType.Structured;
        const ComputeBufferMode MODE = ComputeBufferMode.SubUpdates;
        if (cb == null || cb.count < count)
        {
            cb?.Release();
            cb = new ComputeBuffer(count, stride, TYPE, MODE);
        }
        return cb;
    }

    private bool UpdateCurrentOffset(DNAGrid grid)
    {
        Matrix4x4 gizmo = Matrix4x4.TRS(TransformHandle.GizmosTransform.position,
                                    TransformHandle.GizmosTransform.rotation,
                                    Vector3.one);
        Matrix4x4 delta = gizmo * TransformHandle.InitialGizmoMatrix.inverse;
        Matrix4x4 offset = grid.OldTransformOffset;

        if (grid.IsTransforming)
        {
            offset = delta * offset;
            grid.CurrTransformOffset = offset;

            foreach (GridComponent gc in grid.GridComponents)
            {
                // Update helix bounding box
                Helix helix = gc.Helix;
                if (helix != null)
                {
                    helix.BoundingBox.Reset();

                    helix.BoundingBox.Extend(helix.NucleotideDataA[0].GetPosition());
                    helix.BoundingBox.Extend(helix.NucleotideDataB[0].GetPosition());
                    helix.BoundingBox.Extend(helix.NucleotideDataA.Last().GetPosition());
                    helix.BoundingBox.Extend(helix.NucleotideDataB.Last().GetPosition());
                }
            }

            if (offset != grid.OldTransformOffset)
                return true;
        }       
        return false;
    }
}
