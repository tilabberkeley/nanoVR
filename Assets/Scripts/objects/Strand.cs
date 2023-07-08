/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
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
    // List of nucleotide GameObjects included in strand.
    private List<GameObject> _nucleotides;

    // Strand id.
    private int _strandId;

    // Strand color.
    private Color _color;

    // GameObject at front of strand (at index 0 of nucleotides list).
    private GameObject _head;

    // GameObject at end of strand (at last index of nucleotides list).
    private GameObject _tail;

    // Indicates strand's direction.
    private GameObject _cone;

    // List of strand's crossovers (needs testing).
    private List<GameObject> _xovers;

    // Array of colors that strands cycle through.
    private static Color[] s_colors = { Color.blue, Color.magenta, Color.green, Color.red, Color.cyan, Color.yellow };

    // Strand constructor.
    public Strand(List<GameObject> nucleotides, int strandId)
    {
        _nucleotides = new List<GameObject>(nucleotides);
        _strandId = strandId;
        _color = s_colors[s_numStrands % 6];
        _head = _nucleotides[0];
        _tail = _nucleotides.Last();
        _xovers = new List<GameObject>();
        _cone = DrawPoint.MakeCone(_head.transform.position); 
    }

    // Strand constructor with Color parameter.
    public Strand(List<GameObject> nucleotides, int strandId, Color color)
    {
        _nucleotides = new List<GameObject>(nucleotides);
        _strandId = strandId;
        _color = color;
        _head = _nucleotides[0];
        _tail = _nucleotides.Last();
        _xovers = new List<GameObject>();
        _cone = DrawPoint.MakeCone(_head.transform.position);
    }

    public Strand(List<GameObject> nucleotides, List<GameObject> xovers, int strandId, Color color)
    {
        _nucleotides = new List<GameObject>(nucleotides);
        _xovers = xovers;
        _strandId = strandId;
        _color = color;
        _head = _nucleotides[0];
        _tail = _nucleotides.Last();
        _cone = DrawPoint.MakeCone(_head.transform.position);
    }

    public Strand(List<GameObject> nucleotides, List<GameObject> xovers, int strandId)
    {
        _nucleotides = new List<GameObject>(nucleotides);
        _xovers = xovers;
        _strandId = strandId;
        _color = s_colors[s_numStrands % 6];
        _head = _nucleotides[0];
        _tail = _nucleotides.Last();
        _cone = DrawPoint.MakeCone(_head.transform.position);
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
        _cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
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
            _cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
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
        // ResetComponents(_nucleotides);
        GameObject.Destroy(_cone);
        s_strandDict.Remove(_strandId);
        // s_numStrands -= 1;
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
    public List<GameObject> SplitBefore(GameObject go)
    {
        List<GameObject> splitList = new List<GameObject>();
        int splitIndex = _nucleotides.IndexOf(go);
        //DeleteXover(splitIndex - 1);
        if (!_nucleotides[splitIndex - 1].GetComponent<NucleotideComponent>())
        {
            splitList.AddRange(_nucleotides.GetRange(0, splitIndex - 1));
            _nucleotides[splitIndex - 1].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            _nucleotides.RemoveRange(0, splitIndex);

        }
        else 
        {
            splitList.AddRange(_nucleotides.GetRange(0, splitIndex));
            _nucleotides.RemoveRange(0, splitIndex);
        }
        _head = _nucleotides[0];
        _cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
        ShowHideCone(true);
        SetCone();
        return splitList;
    }

    /// <summary>
    /// Splits strand after given GameObject.
    /// </summary>
    /// <param name="go">GameObject that determines index of where strand is being split.</param>
    /// <returns>Returns list of nucleotides after split.</returns>
    public List<GameObject> SplitAfter(GameObject go)
    {
        List<GameObject> splitList = new List<GameObject>();
        int splitIndex = _nucleotides.IndexOf(go);
        //DeleteXover(splitIndex + 1);
        int count = _nucleotides.Count - splitIndex - 1;
        if (!_nucleotides[splitIndex + 1].GetComponent<NucleotideComponent>())
        {
            splitList.AddRange(_nucleotides.GetRange(splitIndex + 2, count - 1));
            _nucleotides[splitIndex + 1].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            _nucleotides.RemoveRange(splitIndex + 1, count);

        }
        else
        {
            splitList.AddRange(_nucleotides.GetRange(splitIndex + 1, count));
            _nucleotides.RemoveRange(splitIndex + 1, count);

        }
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
            if (GetIndex(xoverComp.GetNextGO()) < index)
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
            if (GetIndex(xoverComp.GetNextGO()) > index)
            {
                xovers.Add(xover);
            }
        }
        return xovers;
    }

    // Removes crossover.
    public void DeleteXover(int index)
    {
        GameObject backbone = _nucleotides[index];
        if (backbone.GetComponent<XoverComponent>())
        {
            DeleteXover(backbone);
        }
    }

    // Removes crossover.
    public void DeleteXover(GameObject xover)
    {
        _xovers.Remove(xover);
        xover.GetComponent<XoverComponent>().GetPrevGO().GetComponent<NucleotideComponent>().SetXover(null);
        xover.GetComponent<XoverComponent>().GetNextGO().GetComponent<NucleotideComponent>().SetXover(null);
        GameObject.Destroy(xover);
    }

    // Highlights strand.
    public void Highlight(Color color)
    {
        for (int i = 0; i < _nucleotides.Count; i++)
        {
            // Set nucleotide
            if (_nucleotides[i].GetComponent<NucleotideComponent>() != null)
            {
                var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
                ntc.Highlight(color);
            }
            // Set backbone
            else
            {
                _nucleotides[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
            }
        }
        for (int i = 0; i < _xovers.Count; i++)
        {
            var xoverComp = _xovers[i].GetComponent<XoverComponent>();
            xoverComp.Highlight(color);
        }
        HighlightCone(color);
    }

    // Highlights cone.
    public void HighlightCone(Color color)
    {
        _cone.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
    }

    // Sets variables of each GameObject's component (strandId, color, etc).
    public void SetComponents()
    {
        Debug.Log("Strand id of nucleotides: " + _strandId);
        for (int i = 0; i < _nucleotides.Count; i++)
        {
            // Set nucleotide
            if (_nucleotides[i].GetComponent<NucleotideComponent>() != null)
            {
                var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
                ntc.Selected = true;
                ntc.StrandId = _strandId;
                ntc.SetColor(_color);
            }
            // Set backbone
            else
            {
                _nucleotides[i].GetComponent<Renderer>().material.SetColor("_Color", _color);
            }
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
        _cone.GetComponent<Renderer>().material.SetColor("_Color", _color);
        if (_head.GetComponent<NucleotideComponent>().Direction == 0)
        {
            _cone.transform.eulerAngles = new Vector3(90, 0, 0);
        }
        else
        {
            _cone.transform.eulerAngles = new Vector3(-90, 0, 0);
        }
    }

    // Sets crossover color based on length (color of strand if appropriate length, gray if questionable, black if undoable).
    public void SetXoverColor(GameObject xover)
    {
        double length = xover.transform.localScale.y;
        if (length <= 0.025)
        {
            xover.GetComponent<Renderer>().material.SetColor("_Color", _color);
        }
        else if (length <= 0.035)
        {
            xover.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
        }
        else
        {
            xover.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
        }           
    }

    // Resets all GameObject components in the nucleotides list.
    public void ResetComponents(List<GameObject> nucleotides)
    {
        if (nucleotides == null) { return; }
        for (int i = 0; i < nucleotides.Count; i++)
        {
            // Reset nucleotide
            if (nucleotides[i].GetComponent<NucleotideComponent>() != null)
            {
                var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
                ntc.Selected = false;
                ntc.StrandId = -1;
                ntc.ResetColor();
                ntc.Highlight(Color.black);
            }
            // Reset backbone
            else
            {
                nucleotides[i].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
                nucleotides[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}
