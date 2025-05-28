/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Utils;

public class Extension : Domain
{
    private List<NucleotideData> nucleotides;
    private List<Vector3> nucleotidePositions;
    private List<Matrix4x4> nuclMatrices;
    private List<Matrix4x4> backboneMatrices;
    private int length;
    private const float ANGLE_OFFSET_DEG = 36f;

    public List<Matrix4x4> Nucleotides { get => nuclMatrices; }
    public List<Matrix4x4> Backbones { get => backboneMatrices; }


    public Extension(int helixId, int direction, int length, Vector3 lastPos, Vector3 secondToLastPos) : base(helixId, direction, startId: -1, endId: -1, new Dictionary<int, int>(), new List<int>())
    {
        IsExtension = true;
        IsHelixBound = false;
        nucleotides = new List<NucleotideData>();
        nucleotidePositions = new List<Vector3>();
        nuclMatrices = new List<Matrix4x4>();
        backboneMatrices = new List<Matrix4x4>();
        this.length = length;
        DrawExtension(length, lastPos, secondToLastPos);
    }

    private void DrawExtension(int length, Vector3 lastPos, Vector3 secondToLastPos)
    {
        // Step 1: Get direction of the last domain segment
        Vector3 lastSegmentDir = (lastPos - secondToLastPos).normalized;

        // Step 2: Rotate it by +36° around Y (helix axis)
        Quaternion twist = Quaternion.AngleAxis(ANGLE_OFFSET_DEG, Vector3.up);
        Vector3 extensionDir = twist * lastSegmentDir;

        // Step 3: Generate positions in a straight line
        for (int i = 0; i < length; i++)
        {
            Vector3 offset = extensionDir * RADIUS / 2 * (i + 1);
            Vector3 pos = lastPos + offset;
            nucleotidePositions.Add(pos);
            Matrix4x4 nuclMatrix = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(NUCL_RAD, NUCL_RAD, NUCL_RAD));
            nuclMatrices.Add(nuclMatrix);
            NucleotideData nd = new NucleotideData(i, HelixId, Direction, inExtension: true);
            nucleotides.Add(nd);

            if (i > 0)
            {
                Vector3 prevPos = nucleotidePositions[i - 1];
                Matrix4x4 backboneMatrixA = GetBackboneMatrix(prevPos, pos, fiveToThree: Direction == 1);
                backboneMatrices.Add(backboneMatrixA);
            }
        }
    }

    // TODO: Add color getter methods
    public Color GetNucleotideColor(int idx)
    {
        NucleotideData nd = GetNucleotideData(idx);
        return nd.Color;
    }

    public Color GetNucleotideHighlight(int idx)
    {
        NucleotideData nd = GetNucleotideData(idx);
        return nd.Highlight;
    }

    public Color GetBackboneColor(int idx)
    {
        if (idx < 0 || idx >= nucleotides.Count - 1)
        {
            throw new IndexOutOfRangeException($" for backbone index {idx}. Valid range is 0 to {nucleotides.Count - 2}.");
        }
        NucleotideData nd = GetNucleotideData(idx);
        return nd.Color;
    }

    public List<Color> GetNucleotideColors()
    {
        List<Color> colors = new List<Color>();
        for (int i = 0; i < length; i++)
        {
            colors.Add(GetNucleotideColor(i));
        }
        return colors;
    }

    public List<Color> GetNucleotideHighlights()
    {
        List<Color> highlights = new List<Color>();
        for (int i = 0; i < length; i++)
        {
            highlights.Add(GetNucleotideHighlight(i));
        }
        return highlights;
    }

    public List<Color> GetBackboneColors()
    {
        List<Color> colors = new List<Color>();
        for (int i = 0; i < length - 1; i++)
        {
            colors.Add(GetBackboneColor(i));
        }
        return colors;
    }

    public override Matrix4x4 GetNucleotideMesh(int idx)
    {
        if (idx < 0 || idx >= nuclMatrices.Count)
        {
            throw new IndexOutOfRangeException($" for nucleotide index {idx}. Valid range is 0 to {nuclMatrices.Count - 1}.");
        }
        return nuclMatrices[idx];
    }

    public override Matrix4x4 GetBackboneMesh(int idx)
    {
        if (idx < 0 || idx >= backboneMatrices.Count)
        {
            throw new IndexOutOfRangeException($" for backbone index {idx}. Valid range is 0 to {backboneMatrices.Count - 1}.");
        }
        return backboneMatrices[idx];
    }

    public override NucleotideData GetNucleotideData(int idx)
    {
        if (idx < 0 || idx >= nucleotides.Count)
        {
            throw new IndexOutOfRangeException($" for nucleotide index {idx}. Valid range is 0 to {nucleotides.Count - 1}.");
        }
        return nucleotides[idx];
    }

    public override List<Matrix4x4> GetDomainMeshes()
    {
        List<Matrix4x4> meshes = new List<Matrix4x4>();
        for (int i = 0; i < nucleotides.Count; i++)
        {
            meshes.Add(GetNucleotideMesh(i));
            if (i > 0)
            {
                meshes.Add(GetBackboneMesh(i - 1));
            }
        }
        return meshes;
    }

    public override List<NucleotideData> GetDomainData()
    {
        return new List<NucleotideData>(nucleotides);
    }

    public override Matrix4x4 GetHeadMesh()
    {       
        return GetNucleotideMesh(0);
    }

    public override Matrix4x4 GetTailMesh()
    {
        return GetNucleotideMesh(nucleotides.Count - 1);
    }

    public override NucleotideData GetHeadData()
    {
        return GetNucleotideData(0);
    }

    public override NucleotideData GetTailData()
    {
        return GetNucleotideData(nucleotides.Count - 1);
    }

    public override Domain SplitBefore(NucleotideData nd)
    {
        throw new NotImplementedException("SplitBefore is not implemented for Extension class.");
    }

    public override Domain SplitAfter(NucleotideData nd)
    {
        throw new NotImplementedException("SplitAfter is not implemented for Extension class.");
    }

    public override void Merge(Domain domain)
    {
        throw new NotImplementedException("Merge is not implemented for Extension class.");
    }

    public override int GetLength()
    {
        return length;
    }

    public override void SetDomain(int id, int strandId, Color color)
    {
        StrandId = strandId;
        Id = id;
        Color = color;

        if (NextXover != null)
        {
            NextXover.Color = color;
        }

        for (int i = 0; i < nucleotides.Count; i++)
        {
            NucleotideData nucleotideData = GetNucleotideData(i);
            nucleotideData.DomainIdx = id;
            nucleotideData.StrandId = strandId;
            nucleotideData.Color = color;
        }
    }

    public override string GetSequence()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < nucleotides.Count; i++)
        {
            NucleotideData nucleotideData = GetNucleotideData(i);
            sb.Append(nucleotideData.Sequence);
        }
        return sb.ToString();
    }

    public override void SetSequence(string sequence)
    {
        if (sequence.Length != nucleotides.Count)
        {
            throw new ArgumentException($"Sequence length {sequence.Length} does not match the number of nucleotides {nucleotides.Count}.");
        }

        for (int i = 0; i < sequence.Length; i++)
        {
            NucleotideData nucleotideData = GetNucleotideData(i);
            nucleotideData.Sequence = sequence[i].ToString();
        }
    }
}
