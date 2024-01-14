/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains all variables shared within program.
/// </summary>
public static class GlobalVariables
{
    public static string s_sequence;

    public static bool s_lineTogOn = false;
    public static bool s_curveTogOn = false;
    public static bool s_loopTogOn = true;
    public static bool s_gridTogOn = true;
    public static bool s_honeycombTogOn = false;
    public static bool s_cameraTogOn = false;

    public static bool s_drawTogOn = true;
    public static bool s_eraseTogOn = false;
    public static bool s_splitTogOn = false;
    public static bool s_mergeTogOn = false;
    public static bool s_insTogOn = false;
    public static bool s_delTogOn = false;

    // adjust/add varaibles when add helix view.
    public static bool s_nucleotideView = true;
    public static bool s_hideStencils = false;
    public static bool s_strandView = false;
    public static bool s_helixView = false;

    public static List<Pointer> s_pointerList = new List<Pointer>();
    public static List<DNAGrid> s_gridList = new List<DNAGrid>();
    public static Dictionary<int, Helix> s_helixDict = new Dictionary<int, Helix>();
    public static Dictionary<int, Strand> s_strandDict = new Dictionary<int, Strand>();
    public static Dictionary<int, DNAGrid> s_gridDict = new Dictionary<int, DNAGrid>();
    public static int s_numStrands = 0;
    public static int s_numHelices = 0;
    public static int s_numGrids = 0;
    public static Color[] s_colors = { Color.blue, Color.magenta, Color.green, Color.red, Color.cyan, Color.yellow };
}
