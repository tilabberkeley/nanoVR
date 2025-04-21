using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static GlobalVariables;

public class HelixRendererIndirect : MonoBehaviour
{
    [Header("Meshes & Materials")]
    public Material material;
    public Mesh nucleotideMesh;
    public Mesh backboneMesh;

    private ComputeBuffer _nucleotideBuffer;
    private ComputeBuffer _backboneBuffer;
    private ComputeBuffer _nucleotideArgsBuffer;
    private ComputeBuffer _backboneArgsBuffer;

    private InstanceData[] _nucleotideDataArray;
    private InstanceData[] _backboneDataArray;

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
        AllocateBuffers(DEFAULT_BUFFER_SIZE, ref _nucleotideBuffer, ref _nucleotideDataArray);
        AllocateBuffers(DEFAULT_BUFFER_SIZE, ref _backboneBuffer, ref _backboneDataArray);

        _nucleotideArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        _backboneArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

    }

    void OnDisable()
    {
        _nucleotideArgsBuffer?.Release();
        _backboneArgsBuffer?.Release();
    }

    void Update()
    {
        List<InstanceData> allNucleotides = new List<InstanceData>();
        List<InstanceData> allBackbones = new List<InstanceData>();

        foreach (Helix helix in s_helixDict.Values)
        {
            // Gather all nucleotide instances
            AddInstances(allNucleotides, helix.NucleotideMatricesA, helix.GetNucleotideColors(1));
            AddInstances(allNucleotides, helix.NucleotideMatricesB, helix.GetNucleotideColors(0));

            // Gather all backbone instances
            AddInstances(allBackbones, helix.BackboneMatricesA, helix.GetBackboneColors(1));
            AddInstances(allBackbones, helix.BackboneMatricesB, helix.GetBackboneColors(0));
        }

        DrawAllInstances(backboneMesh, allBackbones, ref _backboneBuffer, ref _backboneDataArray, _backboneArgsBuffer);
        DrawAllInstances(nucleotideMesh, allNucleotides, ref _nucleotideBuffer, ref _nucleotideDataArray, _nucleotideArgsBuffer);
    }

    void AddInstances(List<InstanceData> targetList, List<Matrix4x4> matrices, List<Color> colors)
    {
        int count = matrices.Count;
        for (int i = 0; i < count; i++)
        {
            targetList.Add(new InstanceData
            {
                modelMatrix = matrices[i],
                color = colors[i]
            });
        }
    }

    void AllocateBuffers(int size, ref ComputeBuffer buffer, ref InstanceData[] dataArray)
    {
        int stride = Marshal.SizeOf(typeof(InstanceData));
        buffer = new ComputeBuffer(size, stride, ComputeBufferType.Structured);
        dataArray = new InstanceData[size];
    }

    void EnsureCapacity(int count, ref ComputeBuffer buffer, ref InstanceData[] dataArray)
    {
        if (buffer.count < count)
        {
            buffer?.Release();
            AllocateBuffers(count * 2, ref buffer, ref dataArray);
        }
    }

    void DrawAllInstances(Mesh mesh, List<InstanceData> dataList, ref ComputeBuffer buffer, ref InstanceData[] dataArray, ComputeBuffer argsBuffer)
    {
        int count = dataList.Count;
        if (count == 0) return;

        EnsureCapacity(count, ref buffer, ref dataArray);
        dataList.CopyTo(dataArray);

        buffer.SetData(dataArray, 0, 0, count);
        material.SetBuffer(InstanceBufferID, buffer);

        uint[] args = new uint[5] {
            mesh.GetIndexCount(0),
            (uint)count,
            mesh.GetIndexStart(0),
            mesh.GetBaseVertex(0),
            0
        };
        argsBuffer.SetData(args);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }
}
