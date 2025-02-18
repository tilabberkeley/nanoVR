using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Domain
{
    private int id;
    private int strandId;
    private int helixId;
    private int direction; // direction 1 is forward (nucleotidesA), direction 0 is reverse (nucleotidesB)
    private int startId;
    private int endId;

    private List<(int, int)> insertions; // (nucleotideId, insertionLength)
    private List<int> deletions; // nucleotideIds

    // public int Id { get => id; }
    public int StrandId { get => strandId; set => strandId = value; }
    public int HelixId { get => helixId; }
    public int Direction { get => direction; }
    public int StartId { get => startId; set => startId = value; }
    public int EndId { get => endId; set => endId = value; }

    public Domain(int strandId, int helixId, int direction, int startId, int endId, List<(int, int)> insertions, List<int> deletions)
    {
        // this.id = id;
        this.strandId = strandId;
        this.helixId = helixId;
        this.direction = direction;
        this.startId = startId;
        this.endId = endId;
        this.insertions = new List<(int, int)> (insertions);
        this.deletions = new List<int>(deletions);
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

        if (direction == 0)
        {
            meshes.Reverse();
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

        if (direction == 0)
        {
            data.Reverse();
        }

        return data;
    }

    public Matrix4x4 GetHeadMesh()
    {
        if (direction == 1)
        {
            return GetNucleotideMesh(startId);
        }
        return GetNucleotideMesh(endId);
    }

    public Matrix4x4 GetTailMesh()
    {
        if (direction == 1)
        {
            return GetNucleotideMesh(endId);
        }
        return GetNucleotideMesh(startId);
    }

    public NucleotideData GetHeadData()
    {
        if (direction == 1)
        {
            return GetNucleotideData(startId);
        }
        return GetNucleotideData(endId);
    }

    public NucleotideData GetTailData()
    {
        return GetNucleotideData(endId);
    }
}
