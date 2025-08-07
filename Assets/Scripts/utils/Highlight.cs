/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static helper class to highlight gameobjects.
/// </summary>
public static class Highlight
{
    // Colors for highlighting.
    private static Color DRAW_NUCL_COLOR = Color.green;
    private static Color ERASE_NUCL_COLOR = Color.red;
    //private static Color STRAND_COLOR = Color.blue;
    private static Color HELIX_COLOR = Color.blue;
    //private static Color xoverSuggestionColor = Color.cyan;

    /// <summary>
    /// Highlights given gameobject.
    /// </summary>
    /// <param name="go">GameObject to highlight.</param>
    /// <param name="color">Color to highlight with.</param>
    public static void HighlightGO(GameObject go, Color color)
    { 
        Outline outline = go.GetComponent<Outline>();
        outline.enabled = true;
        outline.OutlineColor = color;
    }

    public static void HighlightGO(NucleotideData nd, Color color)
    {
        nd.Highlight = color;
        nd.IsHighlighted = true;
    }

    /// <summary>
    /// Unhighlights given gameobject.
    /// </summary>
    public static void UnhighlightGO(GameObject go, bool unhighlightInsAndDel)
    {
        if (go == null) { return; }
        Outline outline = go.GetComponent<Outline>();
        outline.enabled = false;
    }

    public static void UnhighlightGO(NucleotideData nd, bool unhighlightInsAndDel)
    {
        if (!unhighlightInsAndDel && (nd.IsInsertion || nd.IsDeletion))
        {
            if (nd.IsInsertion)
            {
                nd.Highlight = DRAW_NUCL_COLOR;
            }
            else if (nd.IsDeletion)
            {
                nd.Highlight = ERASE_NUCL_COLOR;
            }
            return;
        }
        nd.IsHighlighted = false;
    }

    public static void HighlightInsertion(NucleotideData nd)
    {      
        HighlightGO(nd, DRAW_NUCL_COLOR);
    }

    public static void HighlightDeletion(NucleotideData nd)
    {
        HighlightGO(nd, ERASE_NUCL_COLOR);
    }

    public static void UnhighlightInsertion(NucleotideData nd)
    {
        UnhighlightGO(nd, unhighlightInsAndDel: true);
    }

    public static void UnhighlightDeletion(NucleotideData nd)
    {    
        UnhighlightGO(nd, unhighlightInsAndDel: true);
    }

    /// <summary>
    /// Highlights given list of nucleotides and backbones. Highlights red if erase is on. Green otherwise.
    /// </summary>
    /// <param name="list">GameObject list of nucleotides and backbones.</param>
    public static void HighlightNucleotideSelection(List<NucleotideData> list, bool draw)
    {
        Debug.Log("Highlighting nucl seelection");
        Color color = DRAW_NUCL_COLOR;
        if (!draw)
        {
            color = ERASE_NUCL_COLOR;
        }
        if (list == null)
        {
            return;
        }
        for (int i = 0; i < list.Count; i++)
        {
            HighlightGO(list[i], color);
        }
    }

    public static void UnhighlightNucleotideSelection(List<NucleotideData> list, bool isDelete)
    {
        if (list == null)
        {
            return;
        }
        for (int i = 0; i < list.Count; i++)
        {
            UnhighlightGO(list[i], isDelete);
        }
    }

    /// <summary>
    /// Highlights given strand.
    /// </summary>
    /// <param name="strand">Strand to highlight.</param>
    public static void HighlightStrand(Strand strand)
    {
        Debug.Log("Highlighting strand");

        for (int i = 0; i < strand.Domains.Count; i++)
        {
            HighlightDomain(strand.Domains[i], draw: true);
        }
    }

    /// <summary>
    /// Unhighlights given strand.
    /// </summary>
    /// <param name="strand">Strand to unhighlight.</param>
    public static void UnhighlightStrand(Strand strand, bool isDelete)
    {
        for (int i = 0; i < strand.Domains.Count; i++)
        {
            UnhighlightDomain(strand.Domains[i], isDelete);
        }
    }

    public static void HighlightDomain(Domain domain, bool draw)
    {
        HighlightNucleotideSelection(domain.GetDomainData(), draw);
    }

    public static void UnhighlightDomain(Domain domain, bool isDelete)
    {
        UnhighlightNucleotideSelection(domain.GetDomainData(), isDelete);
    }

    public static void HighlightGridCircle(GridComponent gc)
    {
        HighlightGO(gc.gameObject, HELIX_COLOR);
    }

    public static void UnhighlightGridCircle(GridComponent gc)
    {
        UnhighlightGO(gc.gameObject, false);
    }
}
