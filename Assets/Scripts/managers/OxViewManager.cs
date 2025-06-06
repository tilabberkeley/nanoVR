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

/// <summary>
/// Manages rendering of OxView structures in the scene.
/// Similar to HelixManager.cs
/// </summary>
public class OxViewManager : MonoBehaviour
{
    [Header("Meshes & Materials")]
    public Material material;
    public Mesh nucleotideMesh;
    public Mesh backboneMesh;

    const int MATRIX_STRIDE = sizeof(float) * 4 * 4;
    const int COLOR_STRIDE = sizeof(float) * 4;
    const int HIGHLIGHT_STRIDE = sizeof(float) * 4;
    const int ARGS_STRIDE_BYTES = 5 * sizeof(uint);

    ComputeBuffer _nuclMatCB, _nuclColCB, _nuclHilCB, _nuclArgsCB;
    ComputeBuffer _backMatCB, _backColCB, _backHilCB, _backArgsCB;
    MaterialPropertyBlock _mpb;
    private Bounds bounds;

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        bounds = new Bounds(Vector3.zero, Vector3.one * 100000f);

        _nuclArgsCB = new ComputeBuffer(1, ARGS_STRIDE_BYTES, ComputeBufferType.IndirectArguments);
        _backArgsCB = new ComputeBuffer(1, ARGS_STRIDE_BYTES, ComputeBufferType.IndirectArguments);
    }

    void Update()
    {
        // Do not render oxview structures when not simulating
        // HelixMaanger will render DNAGrid helices instead.
        /*if (!GlobalVariables.s_simulating)
        {
            return;
        }*/

        int nucCount = 0, backCount = 0;
        foreach (OxView oxView in GlobalVariables.s_oxViewDict.Values)
        {
            nucCount += oxView.Nucleotides.Count;
            backCount += oxView.Backbones.Count;
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

        foreach (OxView oxView in GlobalVariables.s_oxViewDict.Values)
        {
            Matrix4x4 offset = Matrix4x4.identity;

            FillNativeArrays(oxView.Nucleotides, oxView.NucleotideColors,
                             null, localMats, colours, highlights, ref nIdx, offset);

            FillNativeArrays(oxView.Backbones, oxView.BackboneColors, null,
                             bLocal, bColor, bHighlight, ref bIdx, offset);
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
}
