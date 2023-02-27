/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVariables
{
    public static string s_sequence;
    public static bool s_lineTogOn = false;
    public static bool s_curveTogOn = false;
    public static bool s_loopTogOn = true;
    public static bool s_gridTogOn = true;
    public static bool s_honeycombTogOn = false;
    public static bool s_drawTogOn = true;
    public static bool s_eraseTogOn = false;
    public static List<Object> s_origamis;
    public static List<Pointer> s_pointerList = new List<Pointer>();
    public static List<Grid> s_gridList = new List<Grid>();
    public static Dictionary<int, Strand> s_strandDict = new Dictionary<int, Strand>();
    public static int s_numStrands = 0;
}
