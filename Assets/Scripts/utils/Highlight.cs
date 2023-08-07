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
    private static void HighlightGO(GameObject go, Color color)
    { 
        Outline outline = go.GetComponent<Outline>();
        outline.enabled = true;
        outline.OutlineColor = color;
    }

    /// <summary>
    /// Unhighlights given gameobject.
    /// </summary>
    /// <param name="go">GameObject to unhighlight.</param>
    private static void UnhighlightGO(GameObject go)
    {
        Outline outline = go.GetComponent<Outline>();
        outline.enabled = false;
    }

    /// <summary>
    /// Highlights given list of nucleotides and backbones. Highlights red if erase is on. Green otherwise.
    /// </summary>
    /// <param name="list">GameObject list of nucleotides and backbones.</param>
    public static void HighlightNucleotideSelection(List<GameObject> list)
    {
        Color color = drawNucleotideHighlightColor;
        if (s_eraseTogOn)
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
    public static void UnhighlightNucleotideSelection(List<GameObject> list)
    {
        if (list == null)
        {
            return;
        }
        for (int i = 0; i < list.Count; i++)
        {
            UnhighlightGO(list[i]);
        }
    }

    /// <summary>
    /// Highlights given strand.
    /// </summary>
    /// <param name="strand">Strand to highlight.</param>
    public static void HighlightStrand(Strand strand)
    {
        List<GameObject> nucleotides = strand.Nucleotides;
        List<GameObject> xovers = strand.Xovers;
        GameObject cone = strand.Cone;
        for (int i = 0; i < nucleotides.Count; i++)
        {
            HighlightGO(nucleotides[i], strandHighlightColor);
        }
        for (int i = 0; i < xovers.Count; i++)
        {
            HighlightGO(xovers[i], strandHighlightColor);
        }
        HighlightGO(cone, strandHighlightColor);
    }

    /// <summary>
    /// Unhighlights given strand.
    /// </summary>
    /// <param name="strand">Strand to unhighlight.</param>
    public static void UnhighlightStrand(Strand strand)
    {
        List<GameObject> nucleotides = strand.Nucleotides;
        List<GameObject> xovers = strand.Xovers;
        GameObject cone = strand.Cone;
        for (int i = 0; i < nucleotides.Count; i++)
        {
            UnhighlightGO(nucleotides[i]);
        }
        for (int i = 0; i < xovers.Count; i++)
        {
            UnhighlightGO(xovers[i]);
        }
        UnhighlightGO(cone);
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
        List<int> strandIds = helix.StrandIds;
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
        List<int> strandIds = helix.StrandIds;
        for (int i = 0; i < nucleotidesA.Count; i++)
        {
            UnhighlightGO(nucleotidesA[i]);
            UnhighlightGO(nucleotidesB[i]);
        }
        for (int i = 0; i < backbonesA.Count; i++)
        {
            UnhighlightGO(backbonesA[i]);
            UnhighlightGO(backbonesB[i]);
        }
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
        UnhighlightGO(xoverSuggestionComponent.gameObject);
    }
}
