using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;

public static class Highlight
{
    private static Color highlightColor = Color.green;
    private static Color unhighlightColor = Color.black;

    /// <summary>
    /// Highlights given nucleotide.
    /// </summary>
    /// <param name="go">Nucleotide GameObject</param>
    public static void HighlightNucleotide(GameObject go)
    { 
        NucleotideComponent comp = go.GetComponent<NucleotideComponent>();
        comp.Highlight(highlightColor);
    }

    /// <summary>
    /// Unhighlights given nucleotide.
    /// </summary>
    /// <param name="go">Nucleotide GameObject</param>
    public static void UnhighlightNucleotide(GameObject go)
    {
        NucleotideComponent comp = go.GetComponent<NucleotideComponent>();
        comp.Highlight(unhighlightColor);
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
            // Highlight nucleotide
            if (list[i].GetComponent<NucleotideComponent>() != null)
            {
                HighlightNucleotide(list[i]);
            }
            // Highlight backbone
            else
            {
                list[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", highlightColor);
            }
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
            // Highlight nucleotide
            if (list[i].GetComponent<NucleotideComponent>() != null)
            {
                UnhighlightNucleotide(list[i]);
            }
            // Highlight backbone
            else
            {
                list[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", unhighlightColor);
            }
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
