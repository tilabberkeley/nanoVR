/*
 * nanoVR, a VR application for building DNA nanostructures.
 * authors: David Yang <davidmyang@berkeley.edu and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

public class SelectGrid : MonoBehaviour
{
    public static DNAGrid s_grid = null;
    public static void HighlightGrid(int gridId)
    {
        s_gridDict.TryGetValue(gridId, out DNAGrid grid);
        s_grid = grid;
        foreach (GridComponent gc in grid.Grid2D)
        {
            Highlight.HighlightHelix(gc.Helix);
        }
    }
}
