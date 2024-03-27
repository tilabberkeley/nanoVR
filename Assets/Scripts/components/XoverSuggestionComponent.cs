using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Highlight;
using static GlobalVariables;

/// <summary>
/// Component for crossover suggestion game object.
/// </summary>
public class XoverSuggestionComponent : MonoBehaviour
{
    // Nucleotide 0 that crossover suggestion is attached to.
    private NucleotideComponent _nucleotideComponent0;
    public NucleotideComponent NucleotideComponent0 
    { 
        get 
        { 
            return _nucleotideComponent0; 
        } 
        set 
        { 
            _nucleotideComponent0 = value;
            value.XoverSuggestionComponents.Add(this);
        } 
    }

    // Nucleotide 1 that crossover suggestion is attached to.
    private NucleotideComponent _nucleotideComponent1;
    public NucleotideComponent NucleotideComponent1 
    {
        get
        {
            return _nucleotideComponent1;
        }
        set
        {
            _nucleotideComponent1 = value;
            value.XoverSuggestionComponents.Add(this);
        }
    }

    // Outline component
    private Outline _outline;

    private void Awake()
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
        DrawCrossover.CreateXover(_nucleotideComponent1.gameObject, _nucleotideComponent0.gameObject);
        // Remove all the crossover suggestions on both nucleotides.
        _nucleotideComponent0.RemoveXoverSuggestions();
        _nucleotideComponent1.RemoveXoverSuggestions();
    }
}
