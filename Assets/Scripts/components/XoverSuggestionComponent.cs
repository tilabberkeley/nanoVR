using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Highlight;

/// <summary>
/// Component for crossover suggestion game object.
/// </summary>
public class XoverSuggestionComponent : MonoBehaviour
{
    // Nucleotide 0 that crossover suggestion is attached to.
    private NucleotideComponent _nucleotideComponent0;
    public NucleotideComponent NucleotideComponent0 { get { return _nucleotideComponent0; } set { _nucleotideComponent0 = value; } }

    // Nucleotide 1 that crossover suggestion is attached to.
    private NucleotideComponent _nucleotideComponent1;
    public NucleotideComponent NucleotideComponent1 { get { return _nucleotideComponent1; } set { _nucleotideComponent1 = value; } }

    // Outline component
    private Outline _outline;

    private void Start()
    {
        // On instantiation, this xover suggestion should not be visable.
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
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
