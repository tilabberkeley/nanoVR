/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Component attached to each nucleotide gameobject. Handles direct Ray interactions and gameobject visuals.
/// </summary>
public class NucleotideComponent : MonoBehaviour
{
    private GameObject _xover;
 
    private Color _color = Color.white;
    private static Color s_yellow = new Color(1, 0.92f, 0.016f, 0.5f);

    private Renderer _ntRenderer;
    private Outline _outline;

    public bool Selected { get; set; } = false;
    public int Id { get; set; }
    public int HelixId { get; set; }
    public int StrandId { get; set; } = -1;
    public int Direction { get; set; } // 0 = 5' to 3' right->left, 1 = left->right

    public bool HasXover() { return _xover != null; }
    public GameObject GetXover() { return _xover; }
    public void SetXover(GameObject x) { _xover = x; }

    public Color GetColor() { return _color; }
    public void SetColor(Color c) 
    { 
        _color = c; 
        _ntRenderer.material.SetColor("_Color", c); 
    }

    public void ResetColor() { 
        _color = Color.white;
        _ntRenderer.material.SetColor("_Color", _color);
    }
    
    public void Highlight(Color color)
    {
        GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
    }

    // Start is called before the first frame update
    void Start()
    {
        _ntRenderer = GetComponent<Renderer>();
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }

    /// <summary>
    /// Returns whether this nucleotide is the last nucleotide in its helix.
    /// </summary>
    /// <returns>True if the nucleotide is last. False otherwise</returns>
    public bool IsEndHelix()
    {
        s_helixDict.TryGetValue(HelixId, out Helix helix);
        // This nucleotide is the last nucleotide if its id is equal to the length of its helix - 1
        return Id == helix.Length - 1;
    }

    /// <summary>
    /// Returns whether this nucleotide is the head or tail of a strand.
    /// </summary>
    /// <returns>True if nucleotide is the head or tail of a strand. False otherwise.</returns>
    public bool IsEndStrand()
    {
        if (!Selected)
        {
            return false;
        }
        s_strandDict.TryGetValue(StrandId, out Strand strand);
        return ReferenceEquals(gameObject, strand.GetTail()) || ReferenceEquals(gameObject, strand.GetHead());
    }

    /// <summary>
    /// Returns the neighbor nucleotides that are in the oppositite direction and same index of this
    /// nucleotide.
    /// </summary>
    /// <returns>List of neighboring nucleotides.</returns>
    public List<NucleotideComponent> getNeighborNucleotides()
    {
        s_helixDict.TryGetValue(HelixId, out Helix thisHelix);
        List<NucleotideComponent> nucleotideComponents = new List<NucleotideComponent>();
        int oppositeDirection = (Direction + 1) % 2;
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
}
