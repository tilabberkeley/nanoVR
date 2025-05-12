using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

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

    ComputeBuffer _helixOffsetCB, _helixIndexCB;

    private Matrix4x4 _currentOffset;
    public Matrix4x4 CurrentOffset => _currentOffset;

    MaterialPropertyBlock _mpb;
    private Bounds bounds;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        bounds = new Bounds(Vector3.zero, Vector3.one * 100000f);

        _nuclArgsCB = new ComputeBuffer(1, ARGS_STRIDE_BYTES, ComputeBufferType.IndirectArguments);
        _backArgsCB = new ComputeBuffer(1, ARGS_STRIDE_BYTES, ComputeBufferType.IndirectArguments);
        _helixOffsetCB = new ComputeBuffer(1, MATRIX_STRIDE);
        _helixIndexCB = new ComputeBuffer(1, sizeof(uint));
    }

    void Update()
    {
        int nucCount = 0, backCount = 0;
        foreach (Helix h in GlobalVariables.s_helixDict.Values)
        {
            nucCount += h.NucleotideMatricesA.Count + h.NucleotideMatricesB.Count;
            backCount += h.BackboneMatricesA.Count + h.BackboneMatricesB.Count;
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

        List<Matrix4x4> helixOffsets = new List<Matrix4x4>();
        List<uint> helixIndices = new List<uint>();
        Dictionary<Helix, int> helixToIndex = new Dictionary<Helix, int>();
        int helixOffsetIdx = 0;

        int nIdx = 0, bIdx = 0;
        foreach (Helix h in GlobalVariables.s_helixDict.Values)
        {
            UpdateCurrentOffset(h);

            if (!helixToIndex.ContainsKey(h))
            {
                helixToIndex[h] = helixOffsetIdx++;
                helixOffsets.Add(_currentOffset);
            }

            uint hIdx = (uint)helixToIndex[h];

            FillNativeArrays(h.NucleotideMatricesA, h.GetNucleotideColors(1),
                             h.GetNucleotideHighlights(1), localMats, colours, highlights, ref nIdx, helixIndices, hIdx);

            FillNativeArrays(h.NucleotideMatricesB, h.GetNucleotideColors(0),
                             h.GetNucleotideHighlights(0), localMats, colours, highlights, ref nIdx, helixIndices, hIdx);

            FillNativeArrays(h.BackboneMatricesA, h.GetBackboneColors(1), null,
                             bLocal, bColor, bHighlight, ref bIdx, helixIndices, hIdx);

            FillNativeArrays(h.BackboneMatricesB, h.GetBackboneColors(0), null,
                             bLocal, bColor, bHighlight, ref bIdx, helixIndices, hIdx);
        }

        _helixOffsetCB.Release();
        _helixOffsetCB = new ComputeBuffer(helixOffsets.Count, MATRIX_STRIDE);
        _helixOffsetCB.SetData(helixOffsets);

        _helixIndexCB.Release();
        _helixIndexCB = new ComputeBuffer(helixIndices.Count, sizeof(uint));
        _helixIndexCB.SetData(helixIndices);

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
        _helixOffsetCB?.Release(); _helixIndexCB?.Release();
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
        ref int idx, List<uint> helixIndices, uint helixIdx)
    {
        bool hasHlt = srcHls != null;
        for (int i = 0; i < srcMats.Count; ++i, ++idx)
        {
            dstLocal[idx] = (float4x4)srcMats[i];
            dstCol[idx] = new float4(srcCols[i].r, srcCols[i].g, srcCols[i].b, srcCols[i].a);
            dstHlt[idx] = hasHlt
                ? new float4(srcHls[i].r, srcHls[i].g, srcHls[i].b, srcHls[i].a)
                : new float4(srcCols[i].r, srcCols[i].g, srcCols[i].b, srcCols[i].a);

            helixIndices.Add(helixIdx);
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
        _mpb.SetBuffer("_HelixOffsets", _helixOffsetCB);
        _mpb.SetBuffer("_HelixIndices", _helixIndexCB);

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

    void UpdateCurrentOffset(Helix h)
    {
        Matrix4x4 g = Matrix4x4.TRS(
                          TransformHandle.GizmosTransform.position,
                          TransformHandle.GizmosTransform.rotation,
                          Vector3.one);
        Matrix4x4 delta = g * TransformHandle.InitialGizmoMatrix.inverse;

        _currentOffset = h.OldTransformOffset;   // base offset
        if (h.IsTransforming)
            _currentOffset = delta * _currentOffset; // live gizmo delta

        h.CurrTransformOffset = _currentOffset;         //  <<< ALWAYS write it
                                                        //      (moved or not)

        if (h.IsTransforming && _currentOffset != h.OldTransformOffset)
            h.UpdateXovers();                             // keep your old logic
    }
}
