using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightGO : MonoBehaviour
{
    public void Highlight()
    {
        DrawCrossover.Highlight(gameObject, Color.yellow);
    }

    public void Unhighlight()
    {
        DrawCrossover.Unhighlight(gameObject);
    }
}
