/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Domain
{
    private int id;         // domain idx in strand's domain list
    private int strandId;
    private int helixId;
    private int direction; // direction 1 is forward (nucleotidesA), direction 0 is reverse (nucleotidesB)
    private int startId;
    private int endId;

    private Dictionary<int, int> insertions; // (nucleotideId, insertionLength)
    private List<int> deletions; // nucleotideIds

    private bool isExtension;
    private bool isHelixBound;

    private XoverComponent prevXover;
    private XoverComponent nextXover;

    private Color color;

    public int Id { get => id; }
    public int StrandId { get => strandId; set => strandId = value; }
    public int HelixId { get => helixId; }
    public int Direction { get => direction; }
    public int StartId { get => startId; set => startId = value; }
    public int EndId { get => endId; set => endId = value; } // INCLUSIVE
    public Dictionary<int, int> Insertions { get => insertions; }
    public List<int> Deletions { get => deletions; }
    public bool IsExtension { get => isExtension; set => isExtension = value; }
    public bool IsHelixBound { get => isHelixBound; set => isHelixBound = value; }
    public XoverComponent PrevXover { get => prevXover; set => prevXover = value; }
    public XoverComponent NextXover { get => nextXover; set => nextXover = value; }
    public Color Color { get => color; set => color = value; }

    public Domain(int helixId, int direction, int startId, int endId, Dictionary<int, int> insertions, List<int> deletions)
    {
        // this.id = id;
        // this.strandId = strandId;
        this.helixId = helixId;
        this.direction = direction;
        this.startId = startId;
        this.endId = endId;
        this.insertions = new Dictionary<int, int> (insertions);
        this.deletions = new List<int>(deletions);
    }

    /// <summary>
    /// Returns the mesh of the nucleotide at the given index.
    /// </summary>
    /// <param name="nucleotideId"></param>
    /// <returns></returns>
    public Matrix4x4 GetNucleotideMesh(int nucleotideId)
    {
        Helix helix = GetHelix();
        return helix.GetNucleotideMesh(nucleotideId, direction);
    }

    public Matrix4x4 GetBackboneMesh(int backboneId)
    {
        Helix helix = GetHelix();
        return helix.GetBackboneMesh(backboneId, direction);
    }

    /// <summary>
    /// Returns the data of the nucleotide at the given index.
    /// </summary>
    /// <param name="nucleotideId"></param>
    /// <returns></returns>
    public NucleotideData GetNucleotideData(int nucleotideId)
    {
        Helix helix = GetHelix();
        return helix.GetNucleotideData(nucleotideId, direction);
    }

    public List<Matrix4x4> GetDomainMeshes()
    {
        List<Matrix4x4> meshes = new List<Matrix4x4>();
        for (int i = startId; i <= endId; i++)
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
        List<NucleotideData> data = new List<NucleotideData>();
        for (int i = startId; i <= endId; i++)
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
        if (direction == 1)
        {
            return GetNucleotideData(endId);
        }
        return GetNucleotideData(startId);
    }

    public Domain SplitBefore(NucleotideData nd)
    {
        Dictionary<int, int> newDomainInsertions = new Dictionary<int, int>();
        List<int> newDomainDeletions = new List<int>();

        // Create new domain's insertions dictionary and remove them from this domain.
        foreach (KeyValuePair<int, int> entry in insertions.ToList())
        {
            if (entry.Key < nd.Id)
            {
                newDomainInsertions.Add(entry.Key, entry.Value);
                insertions.Remove(entry.Key);
            }
        }

        // Create new domain's deletions list and remove them from this domain.
        foreach (int deletion in deletions.ToList())
        {
            if (deletion < nd.Id)
            {
                newDomainDeletions.Add(deletion);
                deletions.Remove(deletion);
            }
        }

        Domain newDomain = new Domain(helixId, direction, startId, nd.Id - 1, newDomainInsertions, newDomainDeletions);

        // Update this domain's startId.
        startId = nd.Id;
        return newDomain;
    }

    public Domain SplitAfter(NucleotideData nd)
    {
        Dictionary<int, int> newDomainInsertions = new Dictionary<int, int>();
        List<int> newDomainDeletions = new List<int>();

        // Create new domain's insertions dictionary and remove them from this domain.
        foreach (KeyValuePair<int, int> entry in insertions.ToList())
        {
            if (entry.Key > nd.Id)
            {
                newDomainInsertions.Add(entry.Key, entry.Value);
                insertions.Remove(entry.Key);
            }
        }

        // Create new domain's deletions list and remove them from this domain.
        foreach (int deletion in deletions.ToList())
        {
            if (deletion > nd.Id)
            {
                newDomainDeletions.Add(deletion);
                deletions.Remove(deletion);
            }
        }

        Domain newDomain = new Domain(helixId, direction, nd.Id + 1, endId, newDomainInsertions, newDomainDeletions);

        // Update this domain's endId.
        endId = nd.Id;
        return newDomain;
    }

    public int GetLength()
    {
        int insertionsLength = insertions.Values.Sum();
        int deletionsLength = deletions.Count;
        return endId - startId + 1 + insertionsLength - deletionsLength;
    }

    public void SetDomain(int id, int strandId, Color color)
    {
        this.strandId = strandId;
        this.id = id;
        this.color = color;

        for (int i = startId; i <= endId; i++)
        {
            NucleotideData nucleotideData = GetNucleotideData(i);
            nucleotideData.DomainIdx = id;
            nucleotideData.StrandId = strandId;
            nucleotideData.Color = color;

            if (insertions.ContainsKey(i))
            {
                nucleotideData.Insertion = insertions[i];
            }

            if (deletions.Contains(i))
            {
                nucleotideData.IsDeletion = true;
            }
        }
    }

    public void SetSequence(string sequence)
    {
        int seqIdx = 0;
        for (int i = startId; i <= endId; i++)
        {
            NucleotideData nucleotideData = GetNucleotideData(i);
            if (nucleotideData.IsDeletion)
            {
                nucleotideData.Sequence = "X";
            }
            else
            {
                int nuclLength = nucleotideData.Insertion + 1;
                nucleotideData.Sequence = sequence.Substring(seqIdx, nuclLength);
                seqIdx += nuclLength;
            }
        }
    }

    public string GetGridId()
    {
        return GlobalVariables.s_helixDict[helixId].GridId;
    }

    public Helix GetHelix()
    {
        return GlobalVariables.s_helixDict[helixId];
    }
}
