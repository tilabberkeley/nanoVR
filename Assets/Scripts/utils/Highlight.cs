using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;

/// <summary>
/// Static helper class to highlight gameobjects.
/// </summary>
public static class Highlight
{
    /* METHODS RELY ON ALL GAMEOBJECTS PASSED IN HAVE OUTLINE COMPONENT */

    private static Color nucleotideHighlightColor = Color.green;

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
    /// Unhighlights given nucleotide.
    /// </summary>
    /// <param name="go">GameObject to unhighlight.</param>
    private static void UnhighlightGO(GameObject go)
    {
        Outline outline = go.GetComponent<Outline>();
        outline.enabled = false;
    }

    /// <summary>
    /// Highlights given list of nucleotides and backbones.
    /// </summary>
    /// <param name="list">GameObject list of nucleotides and backbones.</param>
    public static void HighlightNucleotideSelection(List<GameObject> list)
    {
        if (list == null)
        {
            return;
        }
        for (int i = 0; i < list.Count; i++)
        {
            HighlightGO(list[i], nucleotideHighlightColor);
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
    /// Highlights or unhighlights the given crossover suggestion.
    /// </summary>
    /// <param name="xoverSuggestionComponent">Crossover suggestion to highlight.</param>
    /// <param name="highlight">Highlights if true. Unhighlights if false.</param>
    public static void HighlightXoverSuggestion(XoverSuggestionComponent xoverSuggestionComponent, bool highlight)
    {
        xoverSuggestionComponent.GetComponent<Renderer>().enabled = highlight;
    }
}
