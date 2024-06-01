using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;
using static GlobalVariables;

/// <summary>
/// Static helper class to highlight gameobjects.
/// </summary>
public static class Highlight
{
    /* METHODS RELY ON ALL PASSED IN GAMEOBJECTS HAVING OUTLINE COMPONENT. */

    // Colors for highlighting.
    private static Color drawNucleotideHighlightColor = Color.green;
    private static Color eraseNucleotideHighlightColor = Color.red;
    private static Color strandHighlightColor = Color.blue;
    private static Color helixHighlightColor = Color.yellow;
    private static Color xoverSuggestionColor = Color.cyan;

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

    /// <summary>
    /// Unhighlights given gameobject.
    /// </summary>
    /// <param name="go">GameObject to unhighlight.</param>
    /// <param name="unhighlightInsAndDel">If strand is being deleted, insertions/deletions should be unhighlighted.</param>
    public static void UnhighlightGO(GameObject go, bool unhighlightInsAndDel)
    {
        if (go == null) { return; }
        
        NucleotideComponent ntc = go.GetComponent<NucleotideComponent>(); 
        Outline outline = go.GetComponent<Outline>();

        if (!unhighlightInsAndDel && ntc != null && (ntc.IsInsertion || ntc.IsDeletion))
        {
            if (ntc.IsInsertion)
            {
                outline.OutlineColor = drawNucleotideHighlightColor;
            } 
            else if (ntc.IsDeletion)
            {
                outline.OutlineColor = eraseNucleotideHighlightColor;
            }
            return;
        }
        outline.enabled = false;
    }

    /// <summary>
    /// Helper method to highlight an insertion nucleotide.
    /// </summary>
    /// <param name="go">GameObject nucleotide being highlighted.</param>
    public static void HighlightInsertion(GameObject go)
    {
        if (go.GetComponent<NucleotideComponent>() != null)
        {
            HighlightGO(go, drawNucleotideHighlightColor);
        }
    }

    /// <summary>
    /// Helper method to highlight a deletion nucleotide.
    /// </summary>
    /// <param name="go">GameObject nucleotide being highlighted.</param>
    public static void HighlightDeletion(GameObject go)
    {
        if (go.GetComponent<NucleotideComponent>() != null)
        {
            HighlightGO(go, eraseNucleotideHighlightColor);
        }
    }

    /// <summary>
    /// Helper method to unhighlight an insertion nucleotide.
    /// </summary>
    /// <param name="go">GameObject nucleotide being unhighlighted.</param>
    public static void UnhighlightInsertion(GameObject go)
    {
        if (go.GetComponent<NucleotideComponent>() != null)
        {
            UnhighlightGO(go, true);
        }
    }

    /// <summary>
    /// Helper method to unhighlight a deletion nucleotide.
    /// </summary>
    /// <param name="go">GameObject nucleotide being unhighlighted.</param>
    public static void UnhighlightDeletion(GameObject go)
    {
        if (go.GetComponent<NucleotideComponent>() != null)
        {
            UnhighlightGO(go, true);
        }
    }

    /// <summary>
    /// Highlights given list of nucleotides and backbones. Highlights red if erase is on. Green otherwise.
    /// </summary>
    /// <param name="list">GameObject list of nucleotides and backbones.</param>
    public static void HighlightNucleotideSelection(List<GameObject> list, bool draw)
    {
        Color color = drawNucleotideHighlightColor;
        if (!draw)
        {
            color = eraseNucleotideHighlightColor;
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

    /// <summary>
    /// Unhighlights given list of nucleotides and backbones.
    /// </summary>
    /// <param name="list">GameObject list of nucleotides and backbones.</param>
    public static void UnhighlightNucleotideSelection(List<GameObject> list, bool isDelete)
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
        List<GameObject> nucleotides = strand.Nucleotides;
        GameObject cone = strand.Cone;
        for (int i = 0; i < nucleotides.Count; i++)
        {
            HighlightGO(nucleotides[i], strandHighlightColor);
            var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            if (ntc != null && ntc.HasXover)
            {
                HighlightGO(ntc.Xover, strandHighlightColor);
            }
        }
       
        HighlightGO(cone, strandHighlightColor);
    }

    /// <summary>
    /// Unhighlights given strand.
    /// </summary>
    /// <param name="strand">Strand to unhighlight.</param>
    public static void UnhighlightStrand(Strand strand, bool isDelete)
    {
        List<GameObject> nucleotides = strand.Nucleotides;
        GameObject cone = strand.Cone;
        for (int i = 0; i < nucleotides.Count; i++)
        {
            UnhighlightGO(nucleotides[i], isDelete);
            var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            if (ntc != null && ntc.HasXover)
            {
                UnhighlightGO(ntc.Xover, isDelete);
            }
        }

        UnhighlightGO(cone, isDelete);
    }
    
    /// <summary>
    /// Highlights given helix.
    /// </summary>
    /// <param name="helix">Helix to highlight.</param>
    public static void HighlightHelix(Helix helix)
    {
        List<GameObject> nucleotidesA = helix.NucleotidesA;
        List<GameObject> nucleotidesB = helix.NucleotidesB;
        List<GameObject> backbonesA = helix.BackbonesA;
        List<GameObject> backbonesB = helix.BackbonesB;
        for (int i = 0; i < nucleotidesA.Count; i++)
        {
            HighlightGO(nucleotidesA[i], helixHighlightColor);
            HighlightGO(nucleotidesB[i], helixHighlightColor);
        }
        for (int i = 0; i < backbonesA.Count; i++)
        {
            HighlightGO(backbonesA[i], helixHighlightColor);
            HighlightGO(backbonesB[i], helixHighlightColor);
        }
    }

    /// <summary>
    /// Unhighlights given helix.
    /// </summary>
    /// <param name="helix">Helix to unhighlight.</param>
    public static void UnhighlightHelix(Helix helix)
    {
        List<GameObject> nucleotidesA = helix.NucleotidesA;
        List<GameObject> nucleotidesB = helix.NucleotidesB;
        List<GameObject> backbonesA = helix.BackbonesA;
        List<GameObject> backbonesB = helix.BackbonesB;
        for (int i = 0; i < nucleotidesA.Count; i++)
        {
            UnhighlightGO(nucleotidesA[i], false);
            UnhighlightGO(nucleotidesB[i], false);
        }
        for (int i = 0; i < backbonesA.Count; i++)
        {
            UnhighlightGO(backbonesA[i], false);
            UnhighlightGO(backbonesB[i], false);
        }
    }

    public static void HighlightGridCircle(GridComponent gc)
    {
        HighlightGO(gc.gameObject, helixHighlightColor);
    }

    public static void UnhighlightGridCircle(GridComponent gc)
    {
        UnhighlightGO(gc.gameObject, false);
    }

    /// <summary>
    /// Highlights crossover suggestion.
    /// </summary>
    /// <param name="xoverSuggestionComponent">Crossover suggestion to highlight.</param>
    public static void HighlightXoverSuggestion(XoverSuggestionComponent xoverSuggestionComponent)
    {
        HighlightGO(xoverSuggestionComponent.gameObject, xoverSuggestionColor);
    }

    /// <summary>
    /// Unhighlights crossover suggestion.
    /// </summary>
    /// <param name="xoverSuggestionComponent">Crossover suggestion to unhighlight.</param>
    public static void UnhighlightXoverSuggestion(XoverSuggestionComponent xoverSuggestionComponent)
    {
        UnhighlightGO(xoverSuggestionComponent.gameObject, false);
    }
}
