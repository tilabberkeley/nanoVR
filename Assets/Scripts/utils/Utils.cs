/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GlobalVariables;
using static Geometry;

/// <summary>
/// Useful methods and constants for multiple files.
/// </summary>
public static class Utils
{
    // CONSTANTS
    public const float SCALE_FROM_NANOVR_TO_NM = 22f; // Multiply nanovr coordinate to get to nm scale.
    public const float SCALE_FROM_NM_TO_NANOVR = 1f / SCALE_FROM_NANOVR_TO_NM; // Divide nanovr coordinate to get to nm scale.
    public const float RADIUS = 1f / SCALE_FROM_NANOVR_TO_NM;
    public const float HELIX_GAP = 3f / SCALE_FROM_NANOVR_TO_NM;
    public const float RISE = RISE_PER_BASE_PAIR / SCALE_FROM_NANOVR_TO_NM;
    public const float NUM_BASE_PAIRS = BASES_PER_TURN;
    public const float MINOR_GROOVE_OFFSET = (float)(MINOR_GROOVE_ANGLE / (360f / NUM_BASE_PAIRS)); // Offset for minor groove angle in radians per base pair
    public const float CROSSOVER_LENGTH = 7 * RISE_PER_BASE_PAIR / SCALE_FROM_NANOVR_TO_NM; // Ideal xover length of 7 base pairs??
    public const float NUCL_RAD = 0.008f; // Radius of nucleotide sphere

    public const float XOVER_RAD = 0.2f;
    public const float LOOPOUT_RAD = 0.4f;

    public const float ATOM_SCALE = 10f;

    /// <summary>
    /// Returns nucleotide data given id, helixId, and direction.
    /// </summary>
    /// <param name="id">Id of nucleotide.</param>
    /// <param name="helixId">Id of helix that nucleotide is on.</param>
    /// <param name="direction">Direction of strand that nucleotide is on.</param>
    /// <returns></returns>
    public static NucleotideData FindNucleotideData(int nucleotideId, int helixId, int direction)
    {
        s_helixDict.TryGetValue(helixId, out Helix helix);
        if (helix == null)
        {
            Debug.LogError($"Helix with id {helixId} not found.");
            return null;
        }
        return helix.GetNucleotideData(nucleotideId, direction);
    }

    public static Strand CreateStrand(List<Domain> domains, int strandId, Color color)
    {
        return CreateStrand(domains, strandId, color, false, new Dictionary<int, int>(), isOxview: false); // TODO: Add sequence and isScaffold (if needed
    }

    public static Strand CreateStrand(List<NucleotideData> nucleotides, int strandId, Color color, bool isOxView)
    {
        // TODO: Implement this
        return null;
    }

    public static void CreateStrandWithoutXovers(List<Domain> domains)
    {
        CreateStrandWithoutXovers(domains, s_numStrands, Colors[s_numStrands % Colors.Length]);
    }

    /// <summary>
    /// Create strand without crossovers or loopouts.
    /// Used by strand splitting and deleting xovers since xovers/loopouts already exist.
    /// </summary>
    public static Strand CreateStrandWithoutXovers(List<Domain> domains, int strandId, Color color)
    {
        Strand strand = new Strand(domains, strandId, color, isScaffold: false, isOxview: false);
        strand.SetDomainsRevamp();

        s_strandDict.Add(strandId, strand);
        ObjectListManager.CreateStrandButton(strandId);
        s_numStrands += 1;
        return strand;
    }

    public static Strand CreateStrand(List<Domain> domains, int strandId, Color color, bool isScaffold, Dictionary<int, int> loopouts, bool isOxview = false)
    {
        Strand strand = new Strand(domains, strandId, color, isScaffold, isOxview);
        strand.SetDomainsRevamp();

        s_strandDict.Add(strandId, strand);
        ObjectListManager.CreateStrandButton(strandId);
        s_numStrands += 1;

        // Draw and set xovers and loopouts
        for (int i = 1; i < domains.Count; i++)
        {
            if (loopouts.ContainsKey(i - 1))
            {
                DrawLoopout.CreateLoopoutHelper(domains[i - 1], domains[i], strandId, loopouts[i - 1]);
            }
            else
            {
                DrawCrossover.CreateXoverHelper(domains[i - 1], domains[i], strandId, color, savedColor: color);
            }
        }

        // TODO: CheckMismatch(strand);
        return strand;
    }
   
    public static void CheckMismatch(Strand strand)
    {
        foreach (Domain domain in strand.Domains)
        {
            foreach (NucleotideData nd in domain.GetDomainData())
            {
                CheckMismatch(nd);
            }
        }
    }

    public static void CheckMismatch(NucleotideData nd)
    {
        NucleotideData complementNucl = nd.GetComplement();

        // If complement nucleotide is not assigned a DNA sequence, there is no mismatch of DNA to check.
        if (complementNucl.Sequence.Equals("")) return;

        string complementSequence = ComplementSequence(nd.Sequence);
        if (!complementNucl.Sequence.Equals(complementSequence))
        {
            DrawMismatch(complementNucl);
        }
        else
        {
            RemoveMismatch(complementNucl);
            RemoveMismatch(nd);
        }
    }

    private static void DrawMismatch(NucleotideData complemenNucl)
    {
        Highlight.HighlightGO(complemenNucl, Color.magenta);
    }

    private static void RemoveMismatch(NucleotideData complemenNucl)
    {
        Highlight.UnhighlightGO(complemenNucl, false);
    }

    /// <summary>
    /// Returns complement sequence of input DNA.
    /// </summary>
    public static string ComplementSequence(string dna)
    {
        StringBuilder complementary = new StringBuilder();
        for (int i = dna.Length - 1; i >= 0; i--)
        {
            if (dna[i] == 'A')
            {
                complementary.Append("T");
            }
            else if (dna[i] == 'T')
            {
                complementary.Append("A");
            }
            else if (dna[i] == 'C')
            {
                complementary.Append("G");
            }
            else if (dna[i] == 'G')
            {
                complementary.Append("C");
            }
            else if (dna[i] == 'X')
            {
                complementary.Append("X");
            }
            else
            {
                complementary.Append("?");
            }
        }
        return complementary.ToString();
    }

    public static (float roll, float pitch, float yaw) MatrixToYawPitchRoll(Matrix4x4 m)
    {
        Quaternion q = m.rotation;
        Vector3 e = q.eulerAngles;

        return (e.y, e.x, e.z);
    }

    public static bool IsValidDomain(Helix helix, int startId, int endId, int direction)
    {
        if (helix == null) { return false; }
        for (int i = startId; i <= endId; i++)
        {
            NucleotideData nd = helix.GetNucleotideData(i, direction);
            if (nd.StrandId != -1)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Computes a transformation matrix for a backbone (cylinder) connecting two points.
    /// Assumes that the cylinder mesh is aligned along its Y-axis and centered.
    /// </summary>
    public static Matrix4x4 GetBackboneMatrix(Vector3 start, Vector3 end, bool fiveToThree)
    {
        Vector3 midpoint = (start + end) * 0.5f;
        Vector3 direction = fiveToThree ? end - start : start - end;
        float length = direction.magnitude;
        // Compute rotation so that the cylinder's Y axis aligns with the direction vector.
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
        Vector3 scale = new Vector3(0.2f, length, 0.2f);
        return Matrix4x4.TRS(midpoint, rotation, scale);
    }

    public static void ShowHideAllHelix(bool show)
    {
        foreach (Helix h in s_helixDict.Values)
        {
            h.GridComponent.gameObject.SetActive(show);
            foreach (XoverComponent xover in h.Xovers)
            {
                xover.gameObject.SetActive(show);
            }
        }
    }
}
