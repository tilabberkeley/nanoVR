/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    //public static HashSet<XoverSuggestionComponent> s_xoverSuggestions = new HashSet<XoverSuggestionComponent>();
    public static int s_numStrands = 1;
    public static int s_numHelices = 0;
    public static int s_numGrids = 1;
    public static List<GameObject> allGameObjects = new List<GameObject>();

    // Dictionary to keep track of all .oxview file imports
    public static Dictionary<int, OxView> s_oxViewDict = new Dictionary<int, OxView>();
    public static int s_numOxViews = 0;

    // Simulating toggle - whether we are in simulation mode.
    public static bool s_simulating = false;
    // Simulation running toggle - whether we are simulating (receiving messages from oxview)
    public static bool s_simulationRunning = false;

    // Tracks all protein (PDB) imports
    public static List<GameObject> proteins = new List<GameObject>();

    // Tracks how many copies of each gridName have been made
    public static Dictionary<string, int> s_gridCopies = new Dictionary<string, int>();

    /*// Dictionaries and counts to to keep track of alternate Strand, Helix, and DNAGrid objects when switching to visual mode
    public static Dictionary<int, Helix> s_visHelixDict = new Dictionary<int, Helix>();
    public static Dictionary<int, Strand> s_visStrandDict = new Dictionary<int, Strand>();
    public static Dictionary<string, DNAGrid> s_visGridDict = new Dictionary<string, DNAGrid>();
    public static int s_numVisStrands = 1;
    public static int s_numVisHelices = 0;
    public static int s_numVisGrids = 1;
    public static List<GameObject> allVisGameObjects = new List<GameObject>();*/


    // M13 DNA sequences
    private static TextAsset _DNA7249 = Resources.Load("Sequences/dna7249") as TextAsset;
    private static TextAsset _DNA7560 = Resources.Load("Sequences/dna7560") as TextAsset;
    private static TextAsset _DNA8064 = Resources.Load("Sequences/dna8064") as TextAsset;
    private static TextAsset _DNA8634 = Resources.Load("Sequences/dna78634") as TextAsset; // Special sequence that can be ordered from tilibit

    private static string DNA7249_string = Regex.Replace(_DNA7249.text, @"\s+", "");
    private static string DNA7560_string = Regex.Replace(_DNA7560.text, @"\s+", "");
    private static string DNA8064_string = Regex.Replace(_DNA8064.text, @"\s+", "");
    private static string DNA8634_string = Regex.Replace(_DNA8634.text, @"\s+", "");

    public static string DNA7249 { get { return DNA7249_string; } }
    public static string DNA7560 { get { return DNA7560_string; } }
    public static string DNA8064 { get { return DNA8064_string; } }
    public static string DNA8634 { get { return DNA8634_string; } }

    // Premade scadnano shapes
    private static TextAsset _square = Resources.Load("ShapeFiles/square") as TextAsset;
    private static TextAsset _triangle = Resources.Load("ShapeFiles/triangle") as TextAsset;
    private static TextAsset _6hb = Resources.Load("ShapeFiles/6hb") as TextAsset;
    public static string SQUARE_SC { get { return _square.text; } }
    public static string TRIANGLE_SC { get { return _triangle.text; } }
    public static string SIXHB_SC { get { return _6hb.text; } }



    /* GameObjects to build structures */
    private static GameObject _cone = Resources.Load("HexCone") as GameObject;
    private static GameObject _xover = Resources.Load("Xover") as GameObject;
    private static GameObject _xoverSuggestion = Resources.Load("XoverSuggestion") as GameObject;
    private static GameObject _gridCircle = Resources.Load("GridCircle") as GameObject;
    private static GameObject _gizmos = Resources.Load("Gizmo") as GameObject;
    private static GameObject _loopout = Resources.Load("Loopout") as GameObject;
    private static GameObject _helixCollider = Resources.Load("HelixCollider") as GameObject;
    private static GameObject _helixCylinder = Resources.Load("HelixCylinder") as GameObject;

    private static GameObject _domainInteractable = Resources.Load("Domains/DomainInteractable") as GameObject;
    private static GameObject _domainBezier = Resources.Load("Domains/DomainBezier") as GameObject;
    private static GameObject _bezierEndpoint = Resources.Load("Domains/BezierEndpoint") as GameObject;

    public static GameObject Xover { get { return _xover; } }
    public static GameObject XoverSuggestion { get { return _xoverSuggestion; } }
    public static GameObject GridCircle { get { return _gridCircle; } }
    public static GameObject Loopout { get { return _loopout; } }
    public static GameObject Gizmos { get { return _gizmos; } }
    public static GameObject HelixCylinder { get { return _helixCylinder; } }

    /*public static GameObject DomainInteractable { get => _domainInteractable; }
    public static GameObject DomainBezier { get => _domainBezier; }
    public static GameObject BezierEndpoint { get => _bezierEndpoint; }
    public static GameObject HelixCollider { get => _helixCollider; }*/

    // Protein Atom
    private static GameObject _atom = Resources.Load("Atom") as GameObject;
    public static GameObject Atom { get => _atom; }


    // Strand colors
    private static Color orange = new Color(1.0f, 0.64f, 0.0f);
    private static Color purple = new Color(0.5f, 0.0f, 0.5f);
    private static Color[] _colors = new Color[] { Color.magenta, Color.green, Color.red, Color.yellow, Color.cyan, orange, purple};
    public static Color[] Colors { get { return _colors; } }
}
