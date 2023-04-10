using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightGO : MonoBehaviour
{
    public void Highlight()
    {
        DrawCrossover.Highlight(gameObject);
    }

    public void Unhighlight()
    {
        DrawCrossover.Unhighlight(gameObject);
    }
}
