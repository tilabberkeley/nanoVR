/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Strand object keeps track of an individual strand of nucleotides.
/// </summary>
public class Strand
{
    private const int PERIOD = 10;
    private const int START_OFFSET_3_5 = 3; // First crossover suggestion begins on the fourth nucleotide of helix going from 3->5.
    private const int START_OFFSET_5_3 = 8; // First crossover suggestion begins on the fourth nucleotide of helix going from 5->3.

    // List of nucleotide and backbone GameObjects included in this strand.
    private List<GameObject> _nucleotides;
    public List<GameObject> Nucleotides 
    { 
        get { return _nucleotides; } 
        set { _nucleotides = value; _head = value[0]; _tail = value.Last(); } 
    }

    // This strand's id.
    private int _strandId;

    // This strand's color.
    private Color _color;

    // GameObject at front of this strand (at index 0 of nucleotides list).
    private GameObject _head;

    // GameObject at end of this strand (at last index of nucleotides list).
    private GameObject _tail;

    // Direction of strand
    private int _direction; 

    // Indicates this strand's direction.
    private GameObject _cone;
    public GameObject Cone { get { return _cone; } }

    // List of this strand's crossovers (needs testing).
    private List<GameObject> _xovers;
    public List<GameObject> Xovers { get { return _xovers; } }

    // List of this strand's crossover suggestions.
    private List<GameObject> _xoverSuggestions;

    private GameObject _bezier;

    private String _sequence;

    public String Sequence { get { return _sequence; } set { _sequence = value; } }

    // List of helix ids this strand is on.
    //private List<int> _helixIds;

    // Strand constructor.
    public Strand(List<GameObject> nucleotides, List<GameObject> xovers, int strandId, Color color)
    {
        _nucleotides = new List<GameObject>(nucleotides);
        _xovers = xovers;
        _strandId = strandId;
        _color = color;
        _head = _nucleotides[0];
        _tail = _nucleotides.Last();
        _cone = DrawPoint.MakeCone();
        _bezier = null;
        _direction = nucleotides[0].GetComponent<NucleotideComponent>().Direction;
        //SetComponents();
        //s_strandDict.Add(strandId, this);
        CheckForXoverSuggestions();
        //s_numStrands += 1;
    }

    // Returns strand id.
    public int GetStrandId() { return _strandId; }

    // Returns nucleotide list.
    public List<GameObject> GetNucleotides() { return _nucleotides; }

    public List<GameObject> GetXovers() { return _xovers; }

    public List<GameObject> GetNucleotides(int startIndex, int endIndex) 
    {
        int count = Math.Abs(endIndex - startIndex) + 1;
        return _nucleotides.GetRange(startIndex, count); 
    }

    // Returns head GameObject.
    public GameObject GetHead() { return _head; }

    // Returns tail GameObject.
    public GameObject GetTail() { return _tail; }

    // Returns strand color.
    public Color GetColor() { return _color; }

    // Returns the next GameObject in nucleotide list.
    public GameObject GetPrevGO(GameObject go)
    {
        int index = GetIndex(go);
        return _nucleotides[index - 1];
    }

    // Returns index of GameObject in nucleotide list.
    public int GetIndex(GameObject go)
    {     
        return _nucleotides.IndexOf(go);
    }

    // Returns number of nucleotides in strand, not including backbones.
    public int Count { get { return _nucleotides.Count / 2 + 1; } }

    // Shows or hides cone based on input parameter.
    public void ShowHideCone(bool enabled)
    {
        _cone.GetComponent<Renderer>().enabled = enabled;
    }
    
    // Adds crossover to crossover list.
    public void AddXover(GameObject xover) 
    { 
        _xovers.Add(xover);
    }

    public void AddXovers(List<GameObject> xovers)
    {
        _xovers.AddRange(xovers);
    }

    public void RemoveXovers(List<GameObject> xovers)
    {
        _xovers.RemoveAll(xover => xovers.Contains(xover));
    }

    public bool HasXover()
    {
        return _xovers.Count > 0;
    }

    // Adds GameObject to front of nucleotide list.
    public void AddToHead(GameObject newNucl)
    {
        _nucleotides.Insert(0, newNucl);
        _head = _nucleotides[0];
        //_cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
    }

    // Adds list of GameObjects to front of nucleotide list.
    public void AddToHead(List<GameObject> newNucls) 
    {
        _nucleotides.InsertRange(0, newNucls);
        _head = _nucleotides[0];
        _cone.transform.position = _head.transform.position;
        SetCone();
        //CheckForXoverSuggestions();
    }

    // Adds GameObject to end of nucleotide list.
    public void AddToTail(GameObject newNucl)
    {
        _nucleotides.Add(newNucl);
        _tail = _nucleotides.Last();
    }

    // Adds list of GameObjects to end of nucleotide list.
    public void AddToTail(List<GameObject> newNucls)
    {
        _nucleotides.AddRange(newNucls);
        _tail = _nucleotides.Last();
        //CheckForXoverSuggestions();
    }

    /// <summary>
    /// Removes list of GameObjects from front of nucleotide list. If all nucleotides are removed, delete the strand. Reset components of removed nucleotides (reset strandId, color, etc).
    /// </summary>
    /// <param name="nucleotides">List of GameObjects being removed from nucleotide list.</param>
    public void RemoveFromHead(List<GameObject> nucleotides)
    {
        _nucleotides.RemoveAll(nucleotide => nucleotides.Contains(nucleotide));

        if (_nucleotides.Count > 0)
        {
            _head = _nucleotides[0];
            SetCone();
        }
        ResetComponents(nucleotides);
    }

    /// <summary>
    /// Removes list of GameObjects from end of nucleotide list. If all nucleotides are removed, delete the strand. Reset components of removed nucleotides (reset strandId, color, etc).
    /// </summary>
    /// <param name="nucleotides">List of GameObjects being removed from nucleotide list.</param>
    public void RemoveFromTail(List<GameObject> nucleotides)
    {
        _nucleotides.RemoveAll(nucleotide => nucleotides.Contains(nucleotide));

        if (_nucleotides.Count > 0)
        {
            _tail = _nucleotides.Last();
        }
        ResetComponents(nucleotides);
    }

    /// <summary>
    /// Deletes strand and resets all GameObjects.
    /// </summary>
    public void RemoveStrand()
    {
        GameObject.Destroy(_cone);
        s_strandDict.Remove(_strandId);
    }

    public void DeleteStrand()
    {
        ResetComponents(_nucleotides);
        foreach (GameObject xover in _xovers.ToList()) 
        {
            DeleteXover(xover);
        }
        RemoveStrand();
    }

    /// <summary>
    /// Splits strand before given GameObject.
    /// </summary>
    /// <param name="go">GameObject that determines index of where strand is being split.</param>
    /// <returns>Returns list of nucleotides before split.</returns>
    public List<GameObject> SplitBefore(GameObject go, bool isXover)
    {
        List<GameObject> splitList = new List<GameObject>();
        int splitIndex = _nucleotides.IndexOf(go);

        if (isXover)
        {
            splitList.AddRange(_nucleotides.GetRange(0, splitIndex));
        }
        else
        {
            // Clears the backbone between the two split lists.
            _nucleotides[splitIndex - 1].GetComponent<DNAComponent>().Color = Color.white;
            splitList.AddRange(_nucleotides.GetRange(0, splitIndex - 1));
        }
        
        _nucleotides.RemoveRange(0, splitIndex);
        _head = _nucleotides[0];
        _cone.transform.position = _head.transform.position;
        ShowHideCone(true);
        SetCone();
        return splitList;
    }

    /// <summary>
    /// Splits strand after given GameObject.
    /// </summary>
    /// <param name="go">GameObject that determines index of where strand is being split.</param>
    /// <returns>Returns list of nucleotides after split.</returns>
    public List<GameObject> SplitAfter(GameObject go, bool isXover)
    {
        List<GameObject> splitList = new List<GameObject>();
        int splitIndex = _nucleotides.IndexOf(go);
        int count = _nucleotides.Count - splitIndex - 1;

        if (isXover)
        {
            splitList.AddRange(_nucleotides.GetRange(splitIndex + 1, count));
        }
        else
        {
            // Clear backbone between the two split lists.
            _nucleotides[splitIndex + 1].GetComponent<DNAComponent>().Color = Color.white;
            splitList.AddRange(_nucleotides.GetRange(splitIndex + 2, count - 1));
        }
        _nucleotides.RemoveRange(splitIndex + 1, count);
        _tail = _nucleotides.Last();
        ShowHideCone(true);
        SetCone();
        return splitList;
    }

    public List<GameObject> GetXoversBeforeIndex(int index)
    {
        List<GameObject> xovers = new List<GameObject>();
        foreach (GameObject xover in _xovers)
        {
            var xoverComp = xover.GetComponent<XoverComponent>();
            if (GetIndex(xoverComp.NextGO) < index)
            {
                xovers.Add(xover);
            }
        }
        return xovers;
    }

    public List<GameObject> GetXoversAfterIndex(int index)
    {
        List<GameObject> xovers = new List<GameObject>();
        foreach (GameObject xover in _xovers)
        {
            var xoverComp = xover.GetComponent<XoverComponent>();
            if (GetIndex(xoverComp.NextGO) > index)
            {
                xovers.Add(xover);
            }
        }
        return xovers;
    }

    // Removes crossover.
    public void DeleteXover(GameObject xover)
    {
        _xovers.Remove(xover);
        xover.GetComponent<XoverComponent>().PrevGO.GetComponent<NucleotideComponent>().Xover = null;
        xover.GetComponent<XoverComponent>().NextGO.GetComponent<NucleotideComponent>().Xover = null;
        GameObject.Destroy(xover);
    }

    // Sets variables of each GameObject's component (strandId, color, etc).
    public void SetComponents()
    {
        for (int i = 0; i < _nucleotides.Count; i++)
        {
            var ntc = _nucleotides[i].GetComponent<DNAComponent>();
            ntc.Selected = true;
            ntc.StrandId = _strandId;
            ntc.Color = _color;
        }
        for (int i = 0; i < _xovers.Count; i++)
        {
            SetXoverColor(_xovers[i]);
        }
        SetCone();
    }

    // Sets cone direction (pointing left or right).
    public void SetCone()
    { 
        _cone.GetComponent<ConeComponent>().Color = _color; // FIX: Abstract into Cone component
        int helixId = _head.GetComponent<NucleotideComponent>().HelixId;
        GameObject neighbor = s_helixDict[helixId].GetHeadNeighbor(_head, _head.GetComponent<NucleotideComponent>().Direction);
        Vector3 toDirection;

        // If strand head is index 0 of nucleotide, recalculate direction of cone with next nucleotide.
        if (neighbor == null) {
            toDirection = _head.transform.position - _nucleotides[2].transform.position;
        }
        else
        {
            toDirection = neighbor.transform.position - _head.transform.position;
        }
        _cone.transform.SetPositionAndRotation(_head.transform.position, Quaternion.FromToRotation(Vector3.up, toDirection));
    }

    // Sets crossover color based on length (color of strand if appropriate length, gray if questionable, black if undoable).
    public void SetXoverColor(GameObject xover)
    {
        double length = xover.transform.localScale.y;
        var xoverComp = xover.GetComponent<XoverComponent>();
        if (length <= 0.025)
        {
            xoverComp.Color = _color;
        }
        else if (length <= 0.035)
        {
            xoverComp.Color = Color.gray;
        }
        else
        {
            xoverComp.Color = Color.black;
        }           
    }

    // Resets all GameObject components in the nucleotides list.
    public void ResetComponents(List<GameObject> nucleotides)
    {
        if (nucleotides == null) { return; }
        for (int i = 0; i < nucleotides.Count; i++)
        {
            var ntc = nucleotides[i].GetComponent<DNAComponent>();
            ntc.Selected = false;
            ntc.StrandId = -1;
            ntc.Color = NucleotideComponent.s_defaultColor;
        }
    }

    /// <summary>
    /// Gets all the nucleotides of the strand.
    /// </summary>
    /// <returns>List of nucleotides (their components, not as gameObjects).</returns>
    private List<NucleotideComponent> GetNucleotidesOnly()
    {
        List<NucleotideComponent> result = new List<NucleotideComponent>();

        foreach (GameObject gameObject in _nucleotides)
        {
            DNAComponent dnaComponent = gameObject.GetComponent<DNAComponent>();
            if (!dnaComponent.IsBackbone)
            {
                result.Add((NucleotideComponent) dnaComponent);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates cross over suggestion game objects if there is a close enough strand in the opposite direction.
    /// Closeness is defined by the period of when the nucleotides are close to each other. 
    /// </summary>
    private void CheckForXoverSuggestions()
    {
        List<NucleotideComponent> nucleotides = GetNucleotidesOnly();

        foreach (NucleotideComponent nucleotide in nucleotides)
        {
            int periodIndex = _direction == 0 ? (nucleotide.Id - START_OFFSET_5_3) % PERIOD : (nucleotide.Id - START_OFFSET_3_5) % PERIOD;
            bool isOnPeriod = periodIndex % PERIOD == 0;
            if (isOnPeriod)
            {
                List<NucleotideComponent> neighborNucleotideComponents = nucleotide.getNeighborNucleotides();
                foreach (NucleotideComponent neighborNucleotideComponent in neighborNucleotideComponents)
                {
                    if (neighborNucleotideComponent.IsInStrand())
                    {
                        DrawPoint.MakeXoverSuggestion(nucleotide.gameObject, neighborNucleotideComponent.gameObject);
                    }
                }
            }
        }
    }

    public void DrawBezier()
    {
        if (_bezier == null)
        {
            _bezier = DrawPoint.MakeStrandCylinder(_nucleotides, _color);
        }
    }

    public void DeleteBezier()
    {
        if (_bezier != null)
        {
            GameObject.Destroy(_bezier);
            _bezier = null;
        }
    }
}
