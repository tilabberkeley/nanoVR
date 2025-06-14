/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 *//*
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;
using static Utils;

/// <summary>
/// Component attached to each nucleotide gameobject. Handles direct Ray interactions and gameobject visuals.
/// </summary>
public class NucleotideComponent// : DNAComponent
{
    // Length of insertion. If nucleotide is not insertion, length is 0.
    private int _insertion = 0;
    public int Insertion { get { return _insertion; } set { _insertion = value; } }
    public bool IsInsertion { get { return _insertion != 0; } }

    // Whether nucleotide is deletion or not.
    private bool _deletion;
    public bool IsDeletion { get { return _deletion; } set { _deletion = value; } }

    // DNA base of this nucleotide. Longer than one if nucleotide is insertion.
    // Default DNA is empty string (no base assigned yet)
    private string _sequence = "";
    public string Sequence { get => _sequence; set => _sequence = value; }

    *//*public GameObject Complement 
    { 
        get 
        {
            Helix helix = s_helixDict[HelixId];
            return helix.GetNucleotide(Id, 1 - Direction);
        } 
    }*//*

    // Gameobject of xover or loopout attached to this nucleotide. Null if there isn't a xover or loopout.
    private GameObject _xover = null;
    public GameObject Xover { get { return _xover;} set { _xover = value; } }
    public bool HasXover { get { return _xover != null; } }

    private GameObject _cone = null;
    public GameObject Cone { get { return _cone; } set { _cone = value; } }

    // Set of crossover suggestions connected to this nucleotide.
    private HashSet<XoverSuggestionComponent> _xoverSuggestionComponents;
    public HashSet<XoverSuggestionComponent> XoverSuggestionComponents { get { return _xoverSuggestionComponents; } }

    /// <summary>
    /// Editing location - location of this nucleotide before simulation.
    /// </summary>
    private Vector3 _position = Vector3.zero;
    public Vector3 Position { get { return _position; } set { _position = value; } }
}
*/