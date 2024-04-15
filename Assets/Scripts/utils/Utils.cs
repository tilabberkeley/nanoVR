/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Useful methods for multiple files.
/// </summary>
public static class Utils
{
    // CONSTANTS
    public const float SCALE = 19f;
    public const float RADIUS = 1f / SCALE;
    public const float RISE = .34f / SCALE;
    public const float NUM_BASE_PAIRS = 10.5f;
    public const float CROSSOVER_LENGTH = 7 * .34f / SCALE; // Ideal xover length of 7 base pairs??

    /// <summary>
    /// Returns nucleotide Gameobject given id, helixId, and direction.
    /// </summary>
    /// <param name="id">Id of nucleotide.</param>
    /// <param name="helixId">Id of helix that nucleotide is on.</param>
    /// <param name="direction">Direction of strand that nucleotide is on.</param>
    /// <returns></returns>
    public static GameObject FindNucleotide(int id, int helixId, int direction)
    {
        s_helixDict.TryGetValue(helixId, out Helix helix);
        return helix.GetNucleotide(id, direction);
    }


    // Create strand overloading methods.
    public static Strand CreateStrand(List<GameObject> nucleotides, int strandId) { return CreateStrand(nucleotides, strandId, Colors[s_numStrands % Colors.Length], new List<(GameObject, int)>(), new List<GameObject>(), "", false); }
    public static Strand CreateStrand(List<GameObject> nucleotides, int strandId, Color color) { return CreateStrand(nucleotides, strandId, color, new List<(GameObject, int)>(), new List<GameObject>(), "", false); }
    public static Strand CreateStrand(List<GameObject> nucleotides, int strandId, Color color, List<(GameObject, int)> insertions,
                                      List<GameObject> deletions, string sequence, bool isScaffold)
    {
        Strand strand = new Strand(nucleotides, strandId, color);
        strand.SetComponents();

        foreach ((GameObject, int) nucl in insertions)
        {
            DrawInsertion.Insertion(nucl.Item1, nucl.Item2);
        }

        foreach (GameObject nucl in deletions)
        {
            DrawDeletion.Deletion(nucl);
        }

        strand.Sequence = sequence;
        if (isScaffold)
        {
            strand.IsScaffold = isScaffold;
        }
        if (s_visualMode)
        {
            s_visStrandDict.Add(strandId, strand);
            s_numVisStrands += 1;
        }
        else
        {
            s_strandDict.Add(strandId, strand);
            ObjectListManager.CreateStrandButton(strandId);
            s_numStrands += 1;
        }
        CheckMismatch(strand);
        return strand;
    }

    public static void CheckMismatch(Strand strand)
    {
        foreach (GameObject nucl in strand.Nucleotides)
        {
            NucleotideComponent ntc = nucl.GetComponent<NucleotideComponent>();
            if (ntc != null) CheckMismatch(ntc);
        }
    }

    public static void CheckMismatch(NucleotideComponent ntc)
    {
        NucleotideComponent complementNtc = ntc.Complement.GetComponent<NucleotideComponent>();

        // If complement nucleotide is not assigned a DNA sequence, there is no mismatch of DNA to check.
        if (complementNtc.Sequence.Equals("")) return;

        string complementSequence = ComplementSequence(ntc.Sequence);
        if (!complementNtc.Sequence.Equals(complementSequence))
        {
            DrawMismatch(complementNtc);
        }
        else
        {
            RemoveMismatch(complementNtc);
            RemoveMismatch(ntc);
        }
    }

    private static void DrawMismatch(NucleotideComponent complementNtc)
    {
        Highlight.HighlightGO(complementNtc.gameObject, Color.magenta);
    }

    private static void RemoveMismatch(NucleotideComponent complementNtc)
    {
        Highlight.UnhighlightGO(complementNtc.gameObject, false);
    }

    /// <summary>
    /// Returns complement sequence of input DNA
    /// </summary>
    public static string ComplementSequence(string dna)
    {
        string complementary = "";
        for (int i = dna.Length - 1; i >= 0; i--)
        {
            if (dna[i] == 'A')
            {
                complementary += "T";
            }
            else if (dna[i] == 'T')
            {
                complementary += "A";
            }
            else if (dna[i] == 'C')
            {
                complementary += "G";
            }
            else if (dna[i] == 'G')
            {
                complementary += "C";
            }
            else
            {
                complementary += "?";
            }
        }
        return complementary;
    }

    /// <summary>
    /// Helper method to get Strand object given nucleotide gameobject
    /// </summary>
    public static Strand GetStrand(GameObject nucl)
    {
        NucleotideComponent ntc = nucl.GetComponent<NucleotideComponent>();
        if (ntc.Selected)
        {
            return s_strandDict[ntc.StrandId];
        }
        return null;
    }

    public static Strand GetComplementStrand(Strand strand)
    {
        var ntc = strand.Head.GetComponent<NucleotideComponent>();
        GameObject complement = ntc.Complement;
        return GetStrand(complement);
    }
}
