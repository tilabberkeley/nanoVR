/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Strand object keeps track of an individual strand of nucleotides.
/// </summary>
public class Strand
{
    private List<GameObject> _nucleotides;
    private int _strandId;
    private Color _color;
    private GameObject _head;
    private GameObject _tail;
    private GameObject _cone;
    private List<GameObject> _xovers;
    private static Color[] s_colors = { Color.blue, Color.magenta, Color.green, Color.red, Color.cyan, Color.yellow };

    public Strand(List<GameObject> nucleotides, int strandId)
    {
        _nucleotides = nucleotides;
        _strandId = strandId;
        _color = s_colors[s_numStrands % 6];
        _head = nucleotides[0];
        _tail = nucleotides.Last();
        _xovers = new List<GameObject>();
        DrawPoint d = new DrawPoint();
        _cone = d.MakeCone(_head.transform.position); 
    }

    public Strand(List<GameObject> nucleotides, int strandId, Color color)
    {
        _nucleotides = nucleotides;
        _strandId = strandId;
        _color = color;
        _head = nucleotides[0];
        _tail = nucleotides.Last();
        _xovers = new List<GameObject>();
        DrawPoint d = new DrawPoint();
        _cone = d.MakeCone(_head.transform.position);
    }

    public List<GameObject> GetNucleotides() { return _nucleotides; }
    public GameObject GetHead() { return _head; }
    public GameObject GetTail() { return _tail; }
    public Color GetColor() { return _color; }

    public GameObject GetNextGO(GameObject go)
    {
        int index = _nucleotides.IndexOf(go);
        return _nucleotides[index + 1];
    }

    public int GetStrandId() { return _strandId;}

    public void ShowCone()
    {
        _cone.GetComponent<Renderer>().enabled = true;
    }

    public void HideCone()
    {
        _cone.GetComponent<Renderer>().enabled = false;
    }

    public void AddXover(GameObject xover) 
    { 
        _xovers.Add(xover);
    }


    public void AddToHead(GameObject newNucl)
    {
        _nucleotides.Insert(0, newNucl);
        _head = _nucleotides[0];
        //_cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
    }

    public void AddToHead(List<GameObject> newNucls) 
    {
        _nucleotides.InsertRange(0, newNucls);
        _head = _nucleotides[0];
        _cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
    }

    public void AddToTail(GameObject newNucl)
    {
        _nucleotides.Add(newNucl);
        _tail = _nucleotides.Last();
    }

    public void AddToTail(List<GameObject> newNucls)
    {
        _nucleotides.AddRange(newNucls);
        _tail = _nucleotides.Last();
    }


    public void RemoveFromHead(List<GameObject> nucleotides)
    {
        _nucleotides.RemoveAll(nucleotide => _nucleotides.Contains(nucleotide));

        if (_nucleotides.Count > 0)
        {
            _head = _nucleotides[0];
            _cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
        }
        else
        {
            RemoveStrand();
        }
        ResetComponents(nucleotides);
    }

    public void RemoveFromTail(List<GameObject> nucleotides)
    {
        _nucleotides.RemoveAll(nucleotide => _nucleotides.Contains(nucleotide));

        if (_nucleotides.Count > 0)
        {
            _tail = _nucleotides.Last();
        }
        else
        {
            RemoveStrand();
        }
        ResetComponents(nucleotides);
    }

    public void RemoveStrand()
    {
        GameObject.Destroy(_cone);
        s_strandDict.Remove(_strandId);
    }

    public List<GameObject> SplitBefore(GameObject go)
    {
        List<GameObject> splitList = new List<GameObject>();
        int splitIndex = _nucleotides.IndexOf(go);
        RemoveXover(splitIndex - 1);
        splitList.AddRange(_nucleotides.GetRange(0, splitIndex - 1));
        _nucleotides[splitIndex - 1].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        _nucleotides.RemoveRange(0, splitIndex);
        _head = _nucleotides[0];
        _cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
        ShowCone();
        return splitList;
    }

    public List<GameObject> SplitAfter(GameObject go)
    {
        List<GameObject> splitList = new List<GameObject>();
        int splitIndex = _nucleotides.IndexOf(go);
        RemoveXover(splitIndex + 1);
        int count = _nucleotides.Count - splitIndex - 1;
        splitList.AddRange(_nucleotides.GetRange(splitIndex + 2, count - 1));
        _nucleotides[splitIndex + 1].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        _nucleotides.RemoveRange(splitIndex + 1, count);
        _tail = _nucleotides.Last();
        ShowCone();
        return splitList;
    }

    public void RemoveXover(int index)
    {
        GameObject backbone = _nucleotides[index];
        if (backbone.GetComponent<XoverComponent>() != null)
        {
            RemoveXover(backbone);
        }
    }


    public void SetComponents()
    {
        for (int i = 0; i < _nucleotides.Count; i++)
        {
            // Set nucleotide
            if (_nucleotides[i].GetComponent<NucleotideComponent>() != null)
            {
                var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
                ntc.SetSelected(true);
                ntc.SetStrandId(_strandId);
                ntc.SetColor(_color);
            }
            // Set Crossover
            else if (_nucleotides[i].GetComponent<XoverComponent>() != null)
            {
                SetXoverColor(_nucleotides[i]);
            }
            // Set backbone
            else
            {
                _nucleotides[i].GetComponent<Renderer>().material.SetColor("_Color", _color);
            }
        }
        SetCone();
    }

    public void SetCone()
    {
        _cone.GetComponent<Renderer>().material.SetColor("_Color", _color);
        if (_head.GetComponent<NucleotideComponent>().GetDirection() == 0)
        {
            _cone.transform.eulerAngles = new Vector3(90, 0, 0);
        }
        else
        {
            _cone.transform.eulerAngles = new Vector3(-90, 0, 0);
        }
    }

    public void SetXoverColor(GameObject xover)
    {
        double length = xover.transform.localScale.y;
        if (length <= 0.02)
        {
            xover.GetComponent<Renderer>().material.SetColor("_Color", _color);
        }
        else if (length <= 0.03)
        {
            xover.GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
        }
        else
        {
            xover.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
        }           
    }

    public void RemoveXover(GameObject xover)
    {
        _xovers.Remove(xover);
        GameObject.Destroy(xover);
    }

    public void ResetComponents(List<GameObject> nucleotides)
    {
        if (nucleotides == null) { return; }
        for (int i = 0; i < nucleotides.Count; i++)
        {
            // Reset nucleotide
            if (nucleotides[i].GetComponent<NucleotideComponent>() != null)
            {
                var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
                ntc.SetSelected(false);
                ntc.SetStrandId(-1);
                ntc.ResetColor();
            }
            // Reset Crossover (remove it)
            else if (nucleotides[i].GetComponent<XoverComponent>() != null)
            {
                RemoveXover(nucleotides[i]);
            }
            // Reset backbone
            else
            {
                nucleotides[i].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            }
        }
    }
}
