/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains all variables shared within program.
/// </summary>
public static class GlobalVariables
{
    // Toggle bools for Draw panel
    public static bool s_selectTogOn = false;
    public static bool s_drawTogOn = true;
    public static bool s_eraseTogOn = false;
    public static bool s_splitTogOn = false;
    public static bool s_mergeTogOn = false;
    public static bool s_insTogOn = false;
    public static bool s_delTogOn = false;

    // adjust/add varibles when add helix view.
    public static bool s_nucleotideView = true;
    public static bool s_hideStencils = false;
    public static bool s_visualMode = false;
    public static bool s_strandView = false;
    public static bool s_helixView = false;

    // Dictionaries and counts to keep track of Strand, Helix, and DNAGrid objects
    public static Dictionary<int, Helix> s_helixDict = new Dictionary<int, Helix>();
    public static Dictionary<int, Strand> s_strandDict = new Dictionary<int, Strand>();
    public static Dictionary<string, DNAGrid> s_gridDict = new Dictionary<string, DNAGrid>();
    public static int s_numStrands = 0;
    public static int s_numHelices = 0;
    public static int s_numGrids = 0;

    // Dictionaries and counts to to keep track of alternate Strand, Helix, and DNAGrid objects when switching to visual mode
    public static Dictionary<int, Helix> s_visHelixDict = new Dictionary<int, Helix>();
    public static Dictionary<int, Strand> s_visStrandDict = new Dictionary<int, Strand>();
    public static Dictionary<string, DNAGrid> s_visGridDict = new Dictionary<string, DNAGrid>();
    public static int s_numVisStrands = 0;
    public static int s_numVisHelices = 0;
    public static int s_numVisGrids = 0;

    // M13 DNA sequences
    private static TextAsset _DNA7249 = Resources.Load("Sequences/dna7249") as TextAsset;
    private static TextAsset _DNA7560 = Resources.Load("Sequences/dna7560") as TextAsset;
    private static TextAsset _DNA8064 = Resources.Load("Sequences/dna8064") as TextAsset;
    private static TextAsset _DNA8634 = Resources.Load("Sequences/dna78634") as TextAsset; // Special sequence that can be ordered from tilibit
    public static string DNA7249 { get { return _DNA7249.text; } }
    public static string DNA7560 { get { return _DNA7560.text; } }
    public static string DNA8064 { get { return _DNA8064.text; } }
    public static string DNA8634 { get { return _DNA8634.text; } }


    // Strand colors
    private static Color orange = new Color(1.0f, 0.64f, 0.0f);
    private static Color purple = new Color(0.5f, 0.0f, 0.5f);
    private static Color[] _colors = new Color[] { Color.magenta, Color.green, Color.red, Color.yellow, Color.cyan, orange, purple};
    public static Color[] Colors { get { return _colors; } }
}
