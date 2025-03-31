/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 *//*
using System.Collections.Generic;
using UnityEngine;
using Dummiesman;
using static GlobalVariables;
using static Utils;
using System.Linq;

public class OBJImport : MonoBehaviour
{
    public static void Parse(string filePath)
    {
        // Load the .obj file into a mesh
        GameObject obj = new OBJLoader().Load(filePath);
        if (obj == null)
        {
            Debug.LogError("Failed to load OBJ file.");
            return;
        }

        // Extract the edges of the mesh
        List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)>();
        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>();
        Debug.Log($"Num mesh filter children: {filters.Length}");
        foreach (MeshFilter filter in filters)
        {
            edges.AddRange(ExtractMeshEdges(filter.mesh));
            Debug.Log($"Edges count: {edges.Count}");
        }
        CreateWireframe(edges, obj);
    }

    private static async void CreateWireframe(List<(Vector3, Vector3)> edges, GameObject obj)
    {
        Wireframe wireframe = new Wireframe(s_numWireframes);
        Debug.Log($"Creating wireframe edges");
        foreach (var edge in edges)
        {
            Helix helix = new Helix(s_numHelices, null); // Setting gridComponent null may be bad
           

            Vector3 start = edge.Item1;
            Vector3 end = edge.Item2;
            float edgeLength = Vector3.Distance(start, end);
            int helixLength = Mathf.CeilToInt(edgeLength / RISE);
            await helix.ExtendAsync(helixLength, hideNucleotides: true, start, end);

            s_helixDict.Add(s_numHelices, helix);
            s_numHelices += 1;
        }
        Debug.Log($"Done looping through edges");
        s_wireframeDict.Add(s_numWireframes, wireframe);
        s_numWireframes += 1;
        GameObject.Destroy(obj);

    }

    private static List<(Vector3, Vector3)> ExtractMeshEdges(Mesh mesh)
    {
        Debug.Log("Begin extracting");

        var edges = new List<(Vector3, Vector3)>();
        var vertices = mesh.vertices;
        Debug.Log("Mesh vertices");

        var triangles = mesh.triangles;
        Debug.Log($"Mesh triangles count {triangles.Length}");

        var edgeSet = new HashSet<(int, int)>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v0 = triangles[i];
            int v1 = triangles[i + 1];
            int v2 = triangles[i + 2];

            AddEdge(edgeSet, v0, v1);
            AddEdge(edgeSet, v1, v2);
            AddEdge(edgeSet, v2, v0);
        }

        Debug.Log($"Edge set count: {edgeSet.Count}");

        foreach (var edge in edgeSet)
        {
            edges.Add((vertices[edge.Item1], vertices[edge.Item2]));
        }

        return edges;
    }

    private static void AddEdge(HashSet<(int, int)> edgeSet, int v1, int v2)
    {
        if (v1 > v2)
            (v1, v2) = (v2, v1);

        edgeSet.Add((v1, v2));
    }

    *//*private void DrawDNAHelixAlongEdge(Vector3 start, Vector3 end)
    {
        float edgeLength = Vector3.Distance(start, end);
        int helixSegments = Mathf.CeilToInt(edgeLength / helixPitch);

        for (int i = 0; i <= helixSegments; i++)
        {
            float t = (float)i / helixSegments;
            Vector3 position = Vector3.Lerp(start, end, t);

            float angle = 2 * Mathf.PI * t * (edgeLength / helixPitch);
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * helixRadius,
                Mathf.Sin(angle) * helixRadius,
                0
            );

            Quaternion rotation = Quaternion.LookRotation(end - start);
            Vector3 helixPosition = position + rotation * offset;

            Instantiate(helixSegmentPrefab, helixPosition, rotation, transform);
        }
    }*//*

}
*/