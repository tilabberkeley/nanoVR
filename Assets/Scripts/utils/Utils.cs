/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GlobalVariables;

/// <summary>
/// Useful methods for multiple files.
/// </summary>
public static class Utils
{
    // CONSTANTS
    public const float SCALE = 19f;
    public const float RADIUS = 1f / SCALE;
    public const float HELIX_GAP = 3f / SCALE;
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
    public static Strand CreateStrand(List<GameObject> nucleotides, int strandId, bool isOxview = false) { return CreateStrand(nucleotides, strandId, Colors[s_numStrands % Colors.Length], new List<(GameObject, int)>(), new List<GameObject>(), "", false, isOxview); }
    public static Strand CreateStrand(List<GameObject> nucleotides, int strandId, Color color, bool isOxView = false) { return CreateStrand(nucleotides, strandId, color, new List<(GameObject, int)>(), new List<GameObject>(), "", false, isOxView); }
    public static Strand CreateStrand(List<GameObject> nucleotides, int strandId, Color color, List<(GameObject, int)> insertions,
                                      List<GameObject> deletions, string sequence, bool isScaffold, bool isOxview = false)
    {
        Strand strand = new Strand(nucleotides, strandId, color, isOxview);
        strand.SetComponents();

        foreach ((GameObject, int) nucl in insertions)
        {
            DrawInsertion.Insertion(nucl.Item1, nucl.Item2);
        }

        foreach (GameObject nucl in deletions)
        {
            DrawDeletion.Deletion(nucl);
        }

        
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

    /// <summary>
    /// Returns Strand object of any GameObject (nucleotide, backbone, xover, or loopout).
    /// </summary>
    public static Strand GetStrand(GameObject nucl)
    {
        DNAComponent dnaComp = nucl.GetComponent<DNAComponent>();
        XoverComponent xoverComp = nucl.GetComponent<XoverComponent>();
        Strand strand = null;
        if (dnaComp != null && dnaComp.Selected)
        {
            s_strandDict.TryGetValue(dnaComp.StrandId, out strand);
        }
        if (xoverComp != null)
        {
            s_strandDict.TryGetValue(xoverComp.StrandId, out strand);
        }
        return strand;
    }

    public static void SetUnknownSequence(List<GameObject> nucls)
    {
        foreach (GameObject nucl in nucls)
        {
            NucleotideComponent ntc = nucl.GetComponent<NucleotideComponent>();
            if (ntc != null)
            {
                ntc.Sequence = "?";
            }
        }
    }
}
