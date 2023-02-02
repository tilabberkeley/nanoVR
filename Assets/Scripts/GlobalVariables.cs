using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVariables
{
    public static string s_sequence;
    public static bool s_lineTogOn = true;
    public static bool s_curveTogOn = false;
    public static bool s_loopTogOn = true;
    public static bool s_gridTogOn = false;
    public static bool s_honeycombTogOn = false;
    public static List<Object> s_origamis;
    public static List<Pointer> s_pointerList = new List<Pointer>();
}
