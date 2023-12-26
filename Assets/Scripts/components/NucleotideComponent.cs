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
    /* public static Color s_defaultColor = Color.white;

     // Components
     private Renderer _ntRenderer;
     private Outline _outline;

     // Color of this nucleotide.
     private Color _color = s_defaultColor;
     public Color Color 
     { 
         get 
         { 
             return _color; 
         } 
         set
         {
             _color = value;
             _ntRenderer.material.SetColor("_Color", value);
         }
     }

     // Whether this nucleotide is apart of a strand.
     private bool _selected = false;
     public bool Selected { get { return _selected; } set { _selected = value; } }

     // Id of the nucleotide.
     private int _id;
     public int Id { get { return _id; } set { _id = value; } }

     // Helix id of the helix this nucleotide is apart of.
     private int _helixId;
     public int HelixId { get { return _helixId; } set { _helixId = value; } }

     // Strand id of the strand this nucleotide is apart of. Default as -1, meaning not apart of strand.
     private int _strandId = -1;
     public int StrandId { get { return _strandId; } set { _strandId = value; } }

     // Direction of this nucleotides. 0 = 5' to 3' left<-right, 1 = left->right
     private int _direction;
     public int Direction { get { return _direction; } set { _direction = value; } }*/

    // Length of insertion. If nucleotide is not insertion, length is 0.
    private int _insertion;
    public int Insertion { get { return _insertion; } set { _insertion = value; } }
    public bool IsInsertion { get { return _insertion != 0; } }

    // Whether nucleotide is deletion or not.
    private bool _deletion;
    public bool IsDeletion { get { return _deletion; } set { _deletion = value; } }

    // DNA base of this nucleotide. Longer than one if nucleotide is insertion.
    private string _sequence;
    public string Sequence { get { return _sequence; } set { _sequence = value; } }

    // Gameobject of xover attached to this nucleotide. Null if there isn't a xover.
    private GameObject _xover;
    public GameObject Xover { get { return _xover;} set { _xover = value; } }

    // List of crossover suggestions connect to this nucleotide.
    private List<XoverSuggestionComponent> _xoverSuggestionComponents;
    public List<XoverSuggestionComponent> XoverSuggestionComponents { get { return _xoverSuggestionComponents; } }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _isBackbone = false;
        _xoverSuggestionComponents = new List<XoverSuggestionComponent>();
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
        return ReferenceEquals(gameObject, strand.GetTail()) || ReferenceEquals(gameObject, strand.GetHead());
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
    /// Removes all crossover suggestions on this nucleotide.
    /// </summary>
    public void RemoveXoverSuggestions()
    {
        foreach (XoverSuggestionComponent xoverSuggestionComponent in _xoverSuggestionComponents)
        {
            Destroy(xoverSuggestionComponent.gameObject);
        }
        _xoverSuggestionComponents.Clear();
    }
}
