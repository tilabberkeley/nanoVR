/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using Unity.XR.CoreUtils;
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

    // adjust/add varaibles when add helix view.
    public static bool s_nucleotideView = true;
    public static bool s_hideStencils = false;
    public static bool s_strandView = false;
    public static bool s_helixView = false;

    public static List<Object> s_origamis;
    public static List<Pointer> s_pointerList = new List<Pointer>();
    public static List<Grid> s_gridList = new List<Grid>();
    public static Dictionary<int, Helix> s_helixDict = new Dictionary<int, Helix>();
    public static Dictionary<int, Strand> s_strandDict = new Dictionary<int, Strand>();
    public static Dictionary<int, Grid> s_gridDict = new Dictionary<int, Grid>();
    public static int s_numStrands = 0;
    public static int s_numHelices = 0;
    public static int s_numGrids = 0;
    public static Color[] s_colors = { Color.blue, Color.magenta, Color.green, Color.red, Color.cyan, Color.yellow };
}
