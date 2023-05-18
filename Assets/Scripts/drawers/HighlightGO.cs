using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightGO : MonoBehaviour
{
    Color highlightColor;
    public void Highlight()
    {
        highlightColor = gameObject.GetComponent<Renderer>().material.color;
        DrawCrossover.Highlight(gameObject, Color.yellow);
    }

    public void Unhighlight()
    {
        DrawCrossover.Highlight(gameObject, highlightColor);
    }
}
