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
    public enum NucleotideSize
    {
        LENGTH_1,
        LENGTH_2,
        LENGTH_4,
        LENGTH_8,
        LENGTH_16,
        LENGTH_32,
        LENGTH_64
    }

    public enum BackboneSize
    {
        LENGTH_1,
        LENGTH_3,
        LENGTH_7,
        LENGTH_15,
        LENGTH_31,
        LENGTH_63
    }

    // Toggle bools for Draw panel
    public static bool s_selectTogOn = false;
    public static bool s_drawTogOn = true;
    public static bool s_eraseTogOn = false;
    public static bool s_splitTogOn = false;
    public static bool s_mergeTogOn = false;
    public static bool s_insTogOn = false;
    public static bool s_delTogOn = false;
    public static bool s_loopoutOn = false;

    // adjust/add variables when add helix view.
    public static bool s_nucleotideView = true;
    public static bool s_hideStencils = false;
    public static bool s_visualMode = false;
    public static bool s_strandView = false;
    public static bool s_helixView = false;

    // Dictionaries and counts to keep track of Strand, Helix, and DNAGrid objects
    public static Dictionary<int, Helix> s_helixDict = new Dictionary<int, Helix>();
    public static Dictionary<int, Strand> s_strandDict = new Dictionary<int, Strand>();
    public static Dictionary<string, DNAGrid> s_gridDict = new Dictionary<string, DNAGrid>();
    public static Dictionary<int, SubGrid> s_subGridDict = new Dictionary<int, SubGrid>();
    public static HashSet<XoverSuggestionComponent> s_xoverSuggestions = new HashSet<XoverSuggestionComponent>();
    public static int s_numStrands = 1;
    public static int s_numHelices = 0;
    public static int s_numGrids = 1;
    public static List<GameObject> allGameObjects = new List<GameObject>();

    // OxView to keep track of all .oxview file imports
    public static OxView oxView = new OxView();

    // Tracks how many copies of each gridName have been made
    public static Dictionary<string, int> s_gridCopies = new Dictionary<string, int>();

    // Dictionaries and counts to to keep track of alternate Strand, Helix, and DNAGrid objects when switching to visual mode
    public static Dictionary<int, Helix> s_visHelixDict = new Dictionary<int, Helix>();
    public static Dictionary<int, Strand> s_visStrandDict = new Dictionary<int, Strand>();
    public static Dictionary<string, DNAGrid> s_visGridDict = new Dictionary<string, DNAGrid>();
    public static int s_numVisStrands = 1;
    public static int s_numVisHelices = 0;
    public static int s_numVisGrids = 1;
    public static List<GameObject> allVisGameObjects = new List<GameObject>();


    // M13 DNA sequences
    private static TextAsset _DNA7249 = Resources.Load("Sequences/dna7249") as TextAsset;
    private static TextAsset _DNA7560 = Resources.Load("Sequences/dna7560") as TextAsset;
    private static TextAsset _DNA8064 = Resources.Load("Sequences/dna8064") as TextAsset;
    private static TextAsset _DNA8634 = Resources.Load("Sequences/dna78634") as TextAsset; // Special sequence that can be ordered from tilibit
    public static string DNA7249 { get { return _DNA7249.text; } }
    public static string DNA7560 { get { return _DNA7560.text; } }
    public static string DNA8064 { get { return _DNA8064.text; } }
    public static string DNA8634 { get { return _DNA8634.text; } }

    // Premade scadnano shapes
    private static TextAsset _square = Resources.Load("ShapeFiles/square") as TextAsset;
    private static TextAsset _triangle = Resources.Load("ShapeFiles/triangle") as TextAsset;
    private static TextAsset _6hb = Resources.Load("ShapeFiles/6hb") as TextAsset;
    public static string SQUARE_SC { get { return _square.text; } }
    public static string TRIANGLE_SC { get { return _triangle.text; } }
    public static string SIXHB_SC { get { return _6hb.text; } }



    /* GameObjects to build structures */

    // Nucleotide prefabs with counts 1, 2, 4, 8, 16, 32, and 64
    private static GameObject _nucleotide64 = Resources.Load("Nucleotides/64nt") as GameObject;
    private static GameObject _nucleotide32 = Resources.Load("Nucleotides/32nt") as GameObject;
    private static GameObject _nucleotide16 = Resources.Load("Nucleotides/16nt") as GameObject;
    private static GameObject _nucleotide8 = Resources.Load("Nucleotides/8nt") as GameObject;
    private static GameObject _nucleotide4 = Resources.Load("Nucleotides/4nt") as GameObject;
    private static GameObject _nucleotide2 = Resources.Load("Nucleotides/2nt") as GameObject;
    private static GameObject _nucleotide = Resources.Load("Icosphere") as GameObject;

    public static GameObject Nucleotide64 { get { return _nucleotide64; } }
    public static GameObject Nucleotide32 { get { return _nucleotide32; } }
    public static GameObject Nucleotide16 { get { return _nucleotide16; } }
    public static GameObject Nucleotide8 { get { return _nucleotide8; } }
    public static GameObject Nucleotide4 { get { return _nucleotide4; } }
    public static GameObject Nucleotide2 { get { return _nucleotide2; } }
    public static GameObject Nucleotide { get { return _nucleotide; } }


    // Backbone prefabs with counts 1, 2, 4, 8, 16, 32, and 64
    private static GameObject _backbone63 = Resources.Load("Backbones/63bb") as GameObject;
    private static GameObject _backbone31 = Resources.Load("Backbones/31bb") as GameObject;
    private static GameObject _backbone15 = Resources.Load("Backbones/15bb") as GameObject;
    private static GameObject _backbone7 = Resources.Load("Backbones/7bb") as GameObject;
    private static GameObject _backbone3 = Resources.Load("Backbones/3bb") as GameObject;
    private static GameObject _backbone = Resources.Load("Cylinder") as GameObject;

    public static GameObject Backbone63 { get { return _backbone63; } }
    public static GameObject Backbone31 { get { return _backbone31; } }
    public static GameObject Backbone15 { get { return _backbone15; } }
    public static GameObject Backbone7 { get { return _backbone7; } }
    public static GameObject Backbone3 { get { return _backbone3; } }
    public static GameObject Backbone { get { return _backbone; } }



    private static GameObject _cone = Resources.Load("Cone") as GameObject;
    private static GameObject _xover = Resources.Load("Xover") as GameObject;
    private static GameObject _xoverSuggestion = Resources.Load("XoverSuggestion") as GameObject;
    private static GameObject _gridCircle = Resources.Load("GridCircle") as GameObject;
    private static GameObject _gizmos = Resources.Load("Gizmos") as GameObject;
    private static GameObject _loopout = Resources.Load("Loopout") as GameObject;

    private static GameObject _domainInteractable = Resources.Load("Domains/DomainInteractable") as GameObject;
    private static GameObject _domainBezier = Resources.Load("Domains/DomainBezier") as GameObject;
    private static GameObject _bezierEndpoint = Resources.Load("Domains/BezierEndpoint") as GameObject;

    public static GameObject Cone { get { return _cone; } }
    public static GameObject Xover { get { return _xover; } }
    public static GameObject XoverSuggestion { get { return _xoverSuggestion; } }
    public static GameObject GridCircle { get { return _gridCircle; } }
    public static GameObject Loopout { get { return _loopout; } }
    public static GameObject Gizmos { get { return _gizmos; } }

    public static GameObject DomainInteractable { get => _domainInteractable; }
    public static GameObject DomainBezier { get => _domainBezier; }
    public static GameObject BezierEndpoint { get => _bezierEndpoint; }


    // Strand colors
    private static Color orange = new Color(1.0f, 0.64f, 0.0f);
    private static Color purple = new Color(0.5f, 0.0f, 0.5f);
    private static Color[] _colors = new Color[] { Color.magenta, Color.green, Color.red, Color.yellow, Color.cyan, orange, purple};
    public static Color[] Colors { get { return _colors; } }
}
