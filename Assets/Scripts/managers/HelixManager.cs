/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 * version: DrawMeshInstancedIndirect
 */
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using System.Linq;

public class HelixManager : MonoBehaviour
{
    /* ────────────────────────────────────────────────────────────── */
    /*  CONFIG / CONSTANTS                                            */
    /* ────────────────────────────────────────────────────────────── */

    [Header("Meshes & Materials")]
    public Material material;         // MUST use a shader that supports procedural instancing
    public Mesh nucleotideMesh;
    public Mesh backboneMesh;
    public Mesh gcMesh;

    // ───────── Strides ─────────────────────────────────────────────
    const int MATRIX_STRIDE     = sizeof(float) * 4 * 4;  // 4×4 matrix  = 64 bytes
    const int COLOR_STRIDE      = sizeof(float) * 4;      // Vector4     = 16 bytes
    const int HIGHLIGHT_STRIDE  = sizeof(float) * 4;
    const int ARGS_STRIDE_BYTES = 5 * sizeof(uint);

    /* ────────────────────────────────────────────────────────────── */
    /*  FIELDS (GPU BUFFERS)                                          */
    /* ────────────────────────────────────────────────────────────── */

    ComputeBuffer _nuclMatCB, _nuclColCB, _nuclHilCB, _nuclArgsCB;
    ComputeBuffer _backMatCB, _backColCB, _backHilCB, _backArgsCB;

    /* ────────────────────────────────────────────────────────────── */
    /*  FIELDS (CPU‑SIDE, Persistent NativeArrays)  – Change 1        */
    /* ────────────────────────────────────────────────────────────── */

    NativeArray<float4x4> _nucLocal;
    NativeArray<float4>   _nucCol;
    NativeArray<float4>   _nucHlt;

    NativeArray<float4x4> _backLocal;
    NativeArray<float4>   _backCol;

    // CPU staging arrays reused each frame


    // —— Current gizmo offset (read elsewhere) ——
    private Matrix4x4 _currentOffset;
    public Matrix4x4 CurrentOffset => _currentOffset;

    // MaterialPropertyBlock only needed for global (per‑drawcall) props now
    MaterialPropertyBlock _mpb;

    private Bounds bounds;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        bounds = new Bounds(
            Vector3.zero,
            Vector3.one * 100000f
        );
        _nuclArgsCB = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        _backArgsCB = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
    }

    // ─────────────────────────────────────────────────────────────
    // BIG scratch lists reused every frame
    /*readonly List<Matrix4x4> _nucMatrices = new List<Matrix4x4>();
    readonly List<Vector4> _nucColors = new List<Vector4>();
    readonly List<Vector4> _nucHighlights = new List<Vector4>();

    readonly List<Matrix4x4> _backMatrices = new List<Matrix4x4>();
    readonly List<Vector4> _backColors = new List<Vector4>();*/

    // UPDATE //////////////////////////////////////////////////////
    void Update()
    {
        /* 1. Tally instances ------------------------------------------------*/
        int nucCount = 0, backCount = 0;
        foreach (Helix h in GlobalVariables.s_helixDict.Values)
        {
            nucCount += h.NucleotideMatricesA.Count + h.NucleotideMatricesB.Count;
            backCount += h.BackboneMatricesA.Count + h.BackboneMatricesB.Count;
        }

        if (nucCount == 0 && backCount == 0) return;

        /* 2. Resize / create buffers (SubUpdates mode required for BeginWrite) */
        EnsureBuffer(ref _nuclMatCB, nucCount, MATRIX_STRIDE);
        EnsureBuffer(ref _nuclColCB, nucCount, COLOR_STRIDE);
        EnsureBuffer(ref _nuclHilCB, nucCount, HIGHLIGHT_STRIDE);

        EnsureBuffer(ref _backMatCB, backCount, MATRIX_STRIDE);
        EnsureBuffer(ref _backColCB, backCount, COLOR_STRIDE);
        EnsureBuffer(ref _backHilCB, backCount, HIGHLIGHT_STRIDE);

        /* 3‑A. Stage nucleotide data into NativeArrays ---------------------- */
        var localMats = new NativeArray<float4x4>(nucCount, Allocator.TempJob);
        var colours = new NativeArray<float4>(nucCount, Allocator.TempJob);
        var highlights = new NativeArray<float4>(nucCount, Allocator.TempJob);
        var offsets = new NativeArray<float4x4>(nucCount, Allocator.TempJob);

        var bLocal = new NativeArray<float4x4>(backCount, Allocator.TempJob);
        var bColor = new NativeArray<float4>(backCount, Allocator.TempJob);
        var bHighlight = new NativeArray<float4>(backCount, Allocator.TempJob);
        var bOff = new NativeArray<float4x4>(backCount, Allocator.TempJob);

        int nIdx = 0, bIdx = 0;
        foreach (Helix h in GlobalVariables.s_helixDict.Values)
        {
            UpdateCurrentOffset(h);

            // ── nucleotides ──────────────────────────────
            FillNativeArrays(h.NucleotideMatricesA, h.GetNucleotideColors(1),
                             h.GetNucleotideHighlights(1), _currentOffset,
                             localMats, colours, highlights, offsets, ref nIdx);

            FillNativeArrays(h.NucleotideMatricesB, h.GetNucleotideColors(0),
                             h.GetNucleotideHighlights(0), _currentOffset,
                             localMats, colours, highlights, offsets, ref nIdx);

            // ── backbones ────────────────────────────────
            FillNativeArrays(h.BackboneMatricesA, h.GetBackboneColors(1), null,
                             _currentOffset, bLocal, bColor, bHighlight, bOff, ref bIdx);

            FillNativeArrays(h.BackboneMatricesB, h.GetBackboneColors(0), null,
                             _currentOffset, bLocal, bColor, bHighlight, bOff, ref bIdx);
        }

        /* 3‑B. Map GPU buffers & run Burst job ------------------------------ */
        var gpuMats = _nuclMatCB.BeginWrite<float4x4>(0, nucCount);
        var gpuCols = _nuclColCB.BeginWrite<float4>(0, nucCount);
        var gpuHls = _nuclHilCB.BeginWrite<float4>(0, nucCount);

        var nucJob = new CopyJob
        {
            local = localMats,
            off = offsets,
            col = colours,
            hlt = highlights,
            outM = gpuMats,
            outC = gpuCols,
            outH = gpuHls
        };
        nucJob.Schedule(nucCount, 64, default).Complete();

        _nuclMatCB.EndWrite<float4x4>(nucCount);
        _nuclColCB.EndWrite<float4>(nucCount);
        _nuclHilCB.EndWrite<float4>(nucCount);

        localMats.Dispose(); colours.Dispose(); highlights.Dispose(); offsets.Dispose();

        /* 4. Backbones (no highlights) ------------------------------------- */
        

        var gpuBackM = _backMatCB.BeginWrite<float4x4>(0, backCount);
        var gpuBackC = _backColCB.BeginWrite<float4>(0, backCount);
        var gpuBackH = _backHilCB.BeginWrite<float4>(0, backCount);

        var backJob = new CopyJob
        {
            local = bLocal,
            off = bOff,
            col = bColor,
            hlt = bHighlight,
            outM = gpuBackM,
            outC = gpuBackC,
            outH = gpuBackH
        };
        backJob.Schedule(backCount, 64, default).Complete();

        _backMatCB.EndWrite<float4x4>(backCount);
        _backColCB.EndWrite<float4>(backCount);
        _backHilCB.EndWrite<float4>(backCount);

        bLocal.Dispose(); bColor.Dispose(); bHighlight.Dispose(); bOff.Dispose();

        /* 5. Draw ----------------------------------------------------------- */
        DrawBatch(nucleotideMesh, nucCount,
                  _nuclMatCB, _nuclColCB, _nuclHilCB, _nuclArgsCB);

        DrawBatch(backboneMesh, backCount,
                  _backMatCB, _backColCB, _backColCB, _backArgsCB);
    }

    void OnDestroy()
    {
        _nuclMatCB?.Release(); _nuclColCB?.Release(); _nuclHilCB?.Release();
        _backMatCB?.Release(); _backColCB?.Release(); _backHilCB?.Release();
        _nuclArgsCB?.Release(); _backArgsCB?.Release();
    }

    /* ==================================================================== */
    /*  BURST JOBS (blittable float4x4 / float4)                            */
    /* ==================================================================== */

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    struct CopyJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float4x4> local, off;
        [ReadOnly] public NativeArray<float4> col, hlt;

        [WriteOnly] public NativeArray<float4x4> outM;
        [WriteOnly] public NativeArray<float4> outC, outH;

        public void Execute(int i)
        {
            outM[i] = math.mul(off[i], local[i]);
            outC[i] = col[i];
            outH[i] = hlt[i];
        }
    }

  /*  [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    struct CopyJobNoHighlight : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float4x4> local, off;
        [ReadOnly] public NativeArray<float4> col;

        [WriteOnly] public NativeArray<float4x4> outM;
        [WriteOnly] public NativeArray<float4> outC;

        public void Execute(int i)
        {
            outM[i] = math.mul(off[i], local[i]);
            outC[i] = col[i];
        }
    }*/

    /* ==================================================================== */
    /*  HELPERS                                                             */
    /* ==================================================================== */

    static void FillNativeArrays(
        List<Matrix4x4> srcMats, List<Color> srcCols, List<Color> srcHls,
        Matrix4x4 offset,
        NativeArray<float4x4> dstLocal, NativeArray<float4> dstCol,
        NativeArray<float4> dstHlt, NativeArray<float4x4> dstOff,
        ref int idx)
    {
        bool hasHlt = srcHls != null;
        for (int i = 0; i < srcMats.Count; ++i, ++idx)
        {
            dstLocal[idx] = (float4x4)srcMats[i];
            dstOff[idx] = (float4x4)offset;
            dstCol[idx] = new float4(srcCols[i].r, srcCols[i].g, srcCols[i].b, srcCols[i].a);
            if (hasHlt)
               dstHlt[idx] = new float4(srcHls[i].r, srcHls[i].g, srcHls[i].b, srcHls[i].a);
            else
               dstHlt[idx] = new float4(srcCols[i].r, srcCols[i].g, srcCols[i].b, srcCols[i].a);

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

    /* Create / resize compute buffer (Structured + SubUpdates) */
    ComputeBuffer EnsureBuffer(ref ComputeBuffer cb, int count, int stride)
    {
        const ComputeBufferType TYPE = ComputeBufferType.Structured;
        const ComputeBufferMode MODE = ComputeBufferMode.SubUpdates;     // ★
        if (cb == null || cb.count < count)
        {
            cb?.Release();
            cb = new ComputeBuffer(count, stride, TYPE, MODE);
        }
        return cb;
    }

    /* Re‑compute gizmo offset */
    void UpdateCurrentOffset(Helix h)
    {
        Matrix4x4 g =
            Matrix4x4.TRS(TransformHandle.GizmosTransform.position,
                          TransformHandle.GizmosTransform.rotation,
                          Vector3.one);

        Matrix4x4 delta = g * TransformHandle.InitialGizmoMatrix.inverse;
        _currentOffset = h.OldTransformOffset;

        if (h.IsTransforming)
        {
            _currentOffset = delta * _currentOffset;
            h.CurrTransformOffset = _currentOffset;
            if (_currentOffset != h.OldTransformOffset)
                h.UpdateXovers();
        }
    }
}
