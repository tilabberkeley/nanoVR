using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Domain
{
    private int id;
    private int strandId;
    private int helixId;
    private int direction;
    private int startId;
    private int endId;

    // public int Id { get => id; }
    public int StrandId { get => strandId; set => strandId = value; }
    public int HelixId { get => helixId; }
    public int Direction { get => direction; }
    public int StartId { get => startId; set => startId = value; }
    public int EndId { get => endId; set => endId = value; }

    public Domain(int strandId, int helixId, int direction, int startId, int endId)
    {
        // this.id = id;
        this.strandId = strandId;
        this.helixId = helixId;
        this.direction = direction;
        this.startId = startId;
        this.endId = endId;
    }

    /// <summary>
    /// Returns the mesh of the nucleotide at the given index.
    /// </summary>
    /// <param name="nucleotideId"></param>
    /// <returns></returns>
    public Matrix4x4 GetNucleotideMesh(int nucleotideId)
    {
        Helix helix = GlobalVariables.s_helixDict[helixId];
        return helix.GetNucleotideMesh(nucleotideId, direction);
    }

    public Matrix4x4 GetBackboneMesh(int backboneId)
    {
        Helix helix = GlobalVariables.s_helixDict[helixId];
        return helix.GetBackboneMesh(backboneId, direction);
    }

    /// <summary>
    /// Returns the data of the nucleotide at the given index.
    /// </summary>
    /// <param name="nucleotideId"></param>
    /// <returns></returns>
    public NucleotideData GetNucleotideData(int nucleotideId)
    {
        Helix helix = GlobalVariables.s_helixDict[helixId];
        return helix.GetNucleotideData(nucleotideId, direction);
    }

    public List<Matrix4x4> GetDomainMeshes()
    {
        Helix helix = GlobalVariables.s_helixDict[helixId];

        List<Matrix4x4> meshes = new List<Matrix4x4>();
        for (int i = startId; i < endId; i++)
        {
            meshes.Add(GetNucleotideMesh(i));

            if (i > startId)
            {
                meshes.Add(GetBackboneMesh(i - 1));
            }
        }

        return meshes;
    }

    public List<NucleotideData> GetDomainData()
    {
        Helix helix = GlobalVariables.s_helixDict[helixId];

        List<NucleotideData> data = new List<NucleotideData>();
        for (int i = startId; i < endId; i++)
        {
            data.Add(GetNucleotideData(i));
        }

        return data;
    }

    public Matrix4x4 GetHeadMesh()
    {
        return GetNucleotideMesh(startId);
    }

    public Matrix4x4 GetTailMesh()
    {
        return GetNucleotideMesh(endId);
    }

    public NucleotideData GetHeadData()
    {
        return GetNucleotideData(startId);
    }

    public NucleotideData GetTailData()
    {
        return GetNucleotideData(endId);
    }
}
