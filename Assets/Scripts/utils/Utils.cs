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
public class Utils : MonoBehaviour
{
    // CONSTANTS
    public const float SCALE = 18f;
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
        return strand;
    }

    /*public static void CreateStrand(List<GameObject> nucleotides, int strandId, Color color) { CreateStrand(nucleotides, new List<GameObject>(), strandId, color); }
    public static void CreateStrand(List<GameObject> nucleotides, List<GameObject> xovers, int strandId) { CreateStrand(nucleotides, xovers, strandId, s_colors[s_numStrands % 6]); }
    public static void CreateStrand(List<GameObject> nucleotides, List<GameObject> xovers, int strandId, Color color)
    {
        Strand strand = new Strand(nucleotides, xovers, strandId, color);
        strand.SetComponents();
        s_strandDict.Add(strandId, strand);
        DrawNucleotideDynamic.CreateButton(strandId);
        s_numStrands += 1;
    }*/
}
