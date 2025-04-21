using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static GlobalVariables;

public class GridRendererIndirect : MonoBehaviour
{
    [Header("Meshes & Materials")]
    public Mesh gridCircleMesh;
    public Material gridCircleMaterial;

    private ComputeBuffer _instanceBuffer;
    private ComputeBuffer _argsBuffer;

    private static readonly int InstanceBufferID = Shader.PropertyToID("instanceBuffer");

    private const int DEFAULT_BUFFER_SIZE = 10000;

    [StructLayout(LayoutKind.Sequential)]
    struct InstanceData
    {
        public Matrix4x4 modelMatrix;
        public Vector4 color;
    }

    void OnEnable()
    {
        // Create with initial capacity
        AllocateBuffers(DEFAULT_BUFFER_SIZE);
    }

    void OnDisable()
    {
        _instanceBuffer?.Release();
        _argsBuffer?.Release();
    }

    void Update()
    {
        List<Matrix4x4> allCircleMatrices = new List<Matrix4x4>();

        foreach (DNAGrid grid in s_gridDict.Values)
        {
            List<Matrix4x4> circleMatrices = grid.GetCircleMatrices();
            allCircleMatrices.AddRange(circleMatrices);
        }

        DrawIndirect(gridCircleMesh, gridCircleMaterial, allCircleMatrices);
    }

    private void AllocateBuffers(int size)
    {
        _instanceBuffer?.Release();
        _argsBuffer?.Release();

        int stride = Marshal.SizeOf(typeof(InstanceData));
        _instanceBuffer = new ComputeBuffer(size, stride, ComputeBufferType.Structured);
        _argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
    }

    private void DrawIndirect(Mesh mesh, Material material, List<Matrix4x4> matrices)
    {
        int count = matrices.Count;
        if (count == 0) return;

        // Expand buffer only if needed
        if (_instanceBuffer.count < count)
        {
            AllocateBuffers(count * 2); // grow conservatively (x2)
        }

        // Fill data
        InstanceData[] instanceData = new InstanceData[count];
        for (int i = 0; i < count; i++)
        {
            instanceData[i].modelMatrix = matrices[i];
            instanceData[i].color = new Vector4(1f, 1f, 1f, 1f);
        }

        _instanceBuffer.SetData(instanceData);
        material.SetBuffer(InstanceBufferID, _instanceBuffer);

        // Fill args buffer
        uint[] args = new uint[5] {
            mesh.GetIndexCount(0),
            (uint)count,
            mesh.GetIndexStart(0),
            mesh.GetBaseVertex(0),
            0
        };
        _argsBuffer.SetData(args);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, _argsBuffer);
    }
}
