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

    // Boolean to keep track of how to split strand on crossover creation
    private bool _splitLeft;
    public bool SplitLeft { get { return _splitLeft; } set { _splitLeft = value; } }

    // Outline component
    private Outline _outline;

    private void Awake()
    {
        // On instantiation, this xover suggestion should not be visable.
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }

    /// <summary>
    /// Returns whether this crossover suggestion is valid, which is defined by
    /// whether on crossover suggestion could be currently drawn between this crossover suggestion's
    /// nucleotides (validity may change on some operations).
    /// </summary>
    /// <returns>True if valid. False otherwise.</returns>
    public bool IsValid()
    {
        return DrawCrossoverSuggestion.IsValid(_nucleotideComponent0, _nucleotideComponent1);
    }

    /// <summary>
    /// Returns whether the given nucleotides have a crossover suggestion between them.
    /// </summary>
    /// <param name="nucleotideComponent0">First nucleotide.</param>
    /// <param name="nucleotideComponent1">Second nucleotide.</param>
    /// <returns>True if there is a suggestion between the given nucleotides. False otherwise.</returns>
    public static bool NucleotidesShareSuggestion(NucleotideComponent nucleotideComponent0, NucleotideComponent nucleotideComponent1)
    {
        foreach (XoverSuggestionComponent xoverSuggestionComponent in nucleotideComponent0.XoverSuggestionComponents)
        {
            if (nucleotideComponent1.XoverSuggestionComponents.Contains(xoverSuggestionComponent))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Creates crossover in place of this crossover suggestion, destroying this crossover suggestion.
    /// </summary>
    public void CreateXover()
    {
        if (!_splitLeft)
        {
            DrawCrossover.DoCreateXover(_nucleotideComponent0.gameObject, _nucleotideComponent1.gameObject);
        }
        else
        {
            DrawCrossover.DoCreateXover(_nucleotideComponent1.gameObject, _nucleotideComponent0.gameObject);
        }

        _nucleotideComponent0.RemoveXoverSuggestions();
        _nucleotideComponent1.RemoveXoverSuggestions();

        s_xoverSuggestions.Remove(this);
        Destroy(gameObject);
    }
}
