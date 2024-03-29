/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Component attached to each nucleotide gameobject. Handles direct Ray interactions and gameobject visuals.
/// </summary>
public class NucleotideComponent : DNAComponent
{
    // Length of insertion. If nucleotide is not insertion, length is 0.
    private int _insertion = 0;
    public int Insertion { get { return _insertion; } set { _insertion = value; } }
    public bool IsInsertion { get { return _insertion != 0; } }

    // Whether nucleotide is deletion or not.
    private bool _deletion;
    public bool IsDeletion { get { return _deletion; } set { _deletion = value; } }

    // DNA base of this nucleotide. Longer than one if nucleotide is insertion.
    // Default DNA is X (no base assigned yet)
    private string _sequence = "X";
    public string Sequence { get { return _sequence; } set { _sequence = value; } }

    public GameObject Complement 
    { 
        get 
        {
            Helix helix = s_helixDict[HelixId];
            return helix.GetNucleotide(Id, 1 - Direction);
        } 
    }

    // Gameobject of xover attached to this nucleotide. Null if there isn't a xover.
    private GameObject _xover = null;
    public GameObject Xover { get { return _xover;} set { _xover = value; } }
    public bool HasXover { get { return _xover != null; } }

    // Set of crossover suggestions connected to this nucleotide.
    private HashSet<XoverSuggestionComponent> _xoverSuggestionComponents;
    public HashSet<XoverSuggestionComponent> XoverSuggestionComponents { get { return _xoverSuggestionComponents; } }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _isBackbone = false;
        _xoverSuggestionComponents = new HashSet<XoverSuggestionComponent>();
    }

    /// <summary>
    /// Returns whether this nucleotide is the last nucleotide in its helix.
    /// </summary>
    /// <returns>True if the nucleotide is last. False otherwise</returns>
    public bool IsEndHelix()
    {
        s_helixDict.TryGetValue(_helixId, out Helix helix);
        // This nucleotide is the last nucleotide if its id is equal to the length of its helix - 1
        return Id == helix.Length - 1;
    }

    /// <summary>
    /// Returns whether this nucleotide is the head or tail of a strand.
    /// </summary>
    /// <returns>True if nucleotide is the head or tail of a strand. False otherwise.</returns>
    public bool IsEndStrand()
    {
        if (!_selected)
        {
            return false;
        }
        s_strandDict.TryGetValue(_strandId, out Strand strand);
        return ReferenceEquals(gameObject, strand.Tail) || ReferenceEquals(gameObject, strand.Head);
    }

    /// <summary>
    /// Returns the neighbor nucleotides that are in the oppositite direction and same index of this
    /// nucleotide.
    /// </summary>
    /// <returns>List of neighboring nucleotides.</returns>
    public List<NucleotideComponent> getNeighborNucleotides()
    {
        s_helixDict.TryGetValue(_helixId, out Helix thisHelix);
        List<NucleotideComponent> nucleotideComponents = new List<NucleotideComponent>();
        int oppositeDirection = (_direction + 1) % 2;
        foreach (Helix helix in thisHelix.getNeighborHelices())
        {
            if (oppositeDirection == 1)
            {
                nucleotideComponents.Add(helix.NucleotidesA[Id].GetComponent<NucleotideComponent>());
            }
            else
            {
                nucleotideComponents.Add(helix.NucleotidesB[Id].GetComponent<NucleotideComponent>());
            }
        }
        return nucleotideComponents;
    }
    
    /// <summary>
    /// Returns whether this nucleotide component has a crossover suggestion with the inputted nucleotide.
    /// </summary>
    /// <param name="nucleotideComponent">Nucleotide to check where there is a crossover suggestion.</param>
    /// <returns>True is there is a crossover suggestion between this nucleotide and the given one. False otherwise.</returns>
    public bool HasXoverSuggestion(NucleotideComponent nucleotideComponent)
    {
        foreach (XoverSuggestionComponent xoverSuggestionComponent in _xoverSuggestionComponents)
        {
            if (xoverSuggestionComponent.NucleotideComponent0 == nucleotideComponent || xoverSuggestionComponent.NucleotideComponent1 == nucleotideComponent)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Removes all crossover suggestions on this nucleotide.
    /// </summary>
    public void RemoveXoverSuggestions()
    {
        foreach (XoverSuggestionComponent xoverSuggestionComponent in _xoverSuggestionComponents)
        {
            s_xoverSuggestions.Remove(xoverSuggestionComponent);
            Destroy(xoverSuggestionComponent.gameObject);
        }
    }

    public override void ResetComponent()
    {
        base.ResetComponent();
        _insertion = 0;
        _deletion = false;
        _sequence = "X";
    }

    public int NumModsToLeft()
    {
        Helix helix = s_helixDict[_helixId];
        return helix.NumModsToLeft(_id, _direction);
    }
}
