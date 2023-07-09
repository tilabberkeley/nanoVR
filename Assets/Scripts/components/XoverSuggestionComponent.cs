using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Highlight;

public class XoverSuggestionComponent : MonoBehaviour
{
    private NucleotideComponent _nucleotideComponent0;
    public NucleotideComponent NucleotideComponent0 { get { return _nucleotideComponent0; } set { _nucleotideComponent0 = value; } }
    private NucleotideComponent _nucleotideComponent1;
    public NucleotideComponent NucleotideComponent1 { get { return _nucleotideComponent1; } set { _nucleotideComponent1 = value; } }

    private void Start()
    {
        // On instantiation, this xover suggestion should be unhighlighted
        HighlightXoverSuggestion(this, false);
    }

    /// <summary>
    /// Creates crossover in place of this crossover suggestion, destroying this crossover suggestion.
    /// </summary>
    public void CreateXover()
    {
        DrawCrossover.DoCreateXover(_nucleotideComponent0.gameObject, _nucleotideComponent1.gameObject);
        Destroy(gameObject);
    }
}
