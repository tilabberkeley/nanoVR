using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;

public static class Highlight
{
    private static Color highlightColor = Color.green;
    private static Color unhighlightColor = Color.black;

    public static void HighlightNucleotide(GameObject go)
    { 
        NucleotideComponent comp = go.GetComponent<NucleotideComponent>();
        comp.Highlight(highlightColor);
    }

    public static void UnhighlightNucleotide(GameObject go)
    {
        NucleotideComponent comp = go.GetComponent<NucleotideComponent>();
        comp.Highlight(unhighlightColor);
    }

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
}
