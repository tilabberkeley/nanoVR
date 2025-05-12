/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Useful methods and constants for multiple files.
/// </summary>
public static class Utils
{
    // CONSTANTS
    public const float SCALE_FROM_NANOVR_TO_NM = 22f; // Multiply nanovr coordinate to get to nm scale.
    public const float RADIUS = 1f / SCALE_FROM_NANOVR_TO_NM;
    public const float HELIX_GAP = 3f / SCALE_FROM_NANOVR_TO_NM;
    public const float RISE = .34f / SCALE_FROM_NANOVR_TO_NM;
    public const float NUM_BASE_PAIRS = 10.5f;
    public const float CROSSOVER_LENGTH = 7 * .34f / SCALE_FROM_NANOVR_TO_NM; // Ideal xover length of 7 base pairs??
    public const float NUCL_RAD = 0.008f; // Radius of nucleotide sphere

    public const float XOVER_RAD = 0.2f;
    public const float LOOPOUT_RAD = 0.4f;


    public const float ATOM_SCALE = 10f;

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

    public static Strand CreateStrand(List<Domain> domains)
    {
        return CreateStrand(domains, s_numStrands, Colors[s_numStrands % Colors.Length], false, new Dictionary<int, int>(), isOxview: false); // TODO: Add sequence and isScaffold (if needed
    }

    public static Strand CreateStrand(List<Domain> domains, int strandId, Color color, bool isScaffold, Dictionary<int, int> loopouts, bool isOxview = false)
    {
        Strand strand = new Strand(domains, strandId, color, isScaffold, isOxview);
        //Debug.Log("Created strand " + strandId);
        // Set strand domains
        strand.SetDomainsRevamp();
        //Debug.Log("Set domains");

        // Set cone
        //strand.SetConeRevamp();

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

        // Set sequence
        //strand.SetSequenceRevamp(sequence);
        //Debug.Log("Set sequence");

        // Add to dict and strand list
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

        // TODO: CheckMismatch(strand);
        return strand;
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
        foreach (Domain domain in strand.Domains)
        {
            foreach (NucleotideData nd in domain.GetDomainData())
            {
                CheckMismatch(nd);
            }
        }
    }

    /*public static void CheckMismatch(NucleotideComponent ntc)
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
    }*/

    public static void CheckMismatch(NucleotideData ntc)
    {
        NucleotideData complementNtc = ntc.GetComplement();

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

    private static void DrawMismatch(NucleotideData complementNtc)
    {
        Highlight.HighlightGO(complementNtc, Color.magenta);
    }

    private static void RemoveMismatch(NucleotideData complementNtc)
    {
        Highlight.UnhighlightGO(complementNtc, false);
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
        var dnaComp = nucl.GetComponent<NucleotideColliderComponent>();
        var nd = dnaComp.Data;
        var xoverComp = nucl.GetComponent<XoverComponent>();
        Strand strand = null;
        if (dnaComp != null && nd.IsSelected())
        {
            nd.GetStrand();
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

    public static Quaternion ToQuaternion(float roll, float pitch, float yaw) // roll (x), pitch (y), yaw (z), angles are in degrees, must convert to radians
    {
        // Abbreviations for the various angular functions
        float roll_r = Mathf.Deg2Rad * roll;
        float pitch_r = Mathf.Deg2Rad * pitch;
        float yaw_r = Mathf.Deg2Rad * yaw;

        float cr = Mathf.Cos(roll_r * 0.5f);
        float sr = Mathf.Sin(roll_r * 0.5f);
        float cp = Mathf.Cos(pitch_r * 0.5f);
        float sp = Mathf.Sin(pitch_r * 0.5f);
        float cy = Mathf.Cos(yaw_r * 0.5f);
        float sy = Mathf.Sin(yaw_r * 0.5f);

        Quaternion q;
        q.w = cr * cp * cy + sr * sp * sy;
        q.x = sr * cp * cy - cr * sp * sy;
        q.y = cr * sp * cy + sr * cp * sy;
        q.z = cr * cp * sy - sr * sp * cy;

        return q;
    }

    public static float ToRoll(Quaternion q)
    {
        // roll (x-axis rotation)
        /*float sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
        float cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
        float roll = Mathf.Rad2Deg * Mathf.Atan2(sinr_cosp, cosr_cosp);*/
        float roll = Mathf.Rad2Deg * Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w);
        return roll;
    }

    public static float ToPitch(Quaternion q)
    {
        // pitch (y-axis rotation)
        /*float sinp = Mathf.Sqrt(1 + 2 * (q.w * q.y - q.x * q.z));
        float cosp = Mathf.Sqrt(1 - 2 * (q.w * q.y - q.x * q.z));
        float pitch = Mathf.Rad2Deg * 2 * Mathf.Atan2(sinp, cosp) - Mathf.PI / 2;*/
        float pitch = Mathf.Rad2Deg * Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z);
        return pitch;
    }

    public static float ToYaw(Quaternion q)
    {
        // yaw (z-axis rotation)
        /*float siny_cosp = 2 * (q.w * q.z + q.x * q.y);
        float cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
        float yaw = Mathf.Rad2Deg * Mathf.Atan2(siny_cosp, cosy_cosp);*/
        float yaw = Mathf.Rad2Deg * Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z);
        return yaw;
    }

    public static bool IsValidNucleotides(List<GameObject> nucleotides)
    {
        if (nucleotides == null) { Debug.Log("Is valid nucls are null"); return false; }

        foreach (GameObject nucleotide in nucleotides)
        {
            if (nucleotide == null)
            {
                return false;
            }

            var ntc = nucleotide.GetComponent<DNAComponent>();
            if (ntc.Selected)
            {
                return false;
            }
        }
        return true;
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
}
