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
    private int _direction;
    private Color _color;
    private GameObject _head;
    private GameObject _tail;
    private static Color[] s_colors = { Color.blue, Color.magenta, Color.green, Color.red, Color.cyan, Color.yellow };

    public Strand(List<GameObject> nucleotides, int strandId, int direction)
    {
        _nucleotides = nucleotides;
        _strandId = strandId;
        _direction = direction;
        _color = s_colors[s_numStrands % 6];
        _head = nucleotides[0];
        _tail = nucleotides.Last();
    }

    public Strand(GameObject nucleotide, int strandId, int direction)
    {
        _nucleotides.Add(nucleotide);
        _strandId = strandId;
        _direction = direction;
        _color = s_colors[s_numStrands % 6];
        _head = nucleotide;
        _tail = nucleotide;
    }

    public List<GameObject> GetNucleotides() { return _nucleotides; }
    public GameObject GetHead() { return _head; }
    public GameObject GetTail() { return _tail; }
    public int GetDirection() { return _direction; }

    public void AddToHead(List<GameObject> newNucls) 
    {
        _nucleotides.InsertRange(0, newNucls);
        _head = _nucleotides[0];
    }

    public void AddToTail(List<GameObject> newNucls)
    {
        _nucleotides.AddRange(newNucls);
        _tail = _nucleotides.Last();
    }

    /*
    public void AddToRightHead(List<GameObject> newNucls)
    {
        _nucleotides.AddRange(newNucls);
        _head = _nucleotides.Last();
    }

    public void AddToLeftTail(List<GameObject> newNucls)
    {
        _nucleotides.InsertRange(0, newNucls);
        _tail = _nucleotides[0];
    }
    */

    public List<GameObject> RemoveFromHead(List<GameObject> nucleotides)
    {
        foreach (GameObject nucl in nucleotides)
        {
            _nucleotides.Remove(nucl);
        }

        if (_nucleotides.Count > 0)
        {
            nucleotides.Add(_nucleotides[0]);
            _nucleotides.RemoveAt(0);
            _head = _nucleotides[0];
        }
        RemoveStrand();
        return nucleotides;
    }

    public List<GameObject> RemoveFromTail(List<GameObject> nucleotides)
    {
        foreach (GameObject nucl in nucleotides)
        {
            _nucleotides.Remove(nucl);
        }
        
        if (_nucleotides.Count > 0)
        {
            nucleotides.Add(_nucleotides.Last());
            _nucleotides.Remove(_nucleotides.Last());
            _tail = _nucleotides.Last();
        }
        RemoveStrand();
        return nucleotides;
    }

    public void RemoveStrand()
    {
        if (_nucleotides.Count == 0)
        {
            s_strandDict.Remove(_strandId);
        }
    }

    public List<GameObject> SplitAt(GameObject go)
    {
        List<GameObject> splitList = new List<GameObject>();
        int splitIndex = _nucleotides.IndexOf(go);
        int count = _nucleotides.Count - splitIndex - 1;
        splitList.AddRange(_nucleotides.GetRange(splitIndex + 2, count - 1));
        ResetComponents(_nucleotides.GetRange(splitIndex - 1, count));
        _nucleotides.RemoveRange(splitIndex - 1, count);
        _tail = _nucleotides.Last();
        return splitList;
    }

    /*
    public int RemoveFromRightHead(List<GameObject> nucleotides)
    {
        foreach (GameObject nucl in nucleotides)
        {
            _nucleotides.Remove(nucl);
        }
        _head = _nucleotides.Last();
        return _nucleotides.Count;
    }

    public int RemoveFromLeftTail(List<GameObject> nucleotides)
    {
        foreach (GameObject nucl in nucleotides)
        {
            _nucleotides.Remove(nucl);
        }
        _tail = _nucleotides[0];
        return _nucleotides.Count;
    } */

    public void SetComponents()
    {
        for (int i = 0; i < _nucleotides.Count; i++)
        {
            if (i % 2 == 0)
            {
                var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
                ntc.SetSelected(true);
                ntc.SetStrandId(_strandId);
                ntc.SetColor(_color);
            }
            else
            {
                _nucleotides[i].GetComponent<Renderer>().material.SetColor("_Color", _color);
            }
        }
    }

    public void ResetComponents(List<GameObject> nucleotides)
    {
        for (int i = 0; i < nucleotides.Count; i++)
        {
            if (i % 2 == 0)
            {
                var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
                ntc.SetSelected(false);
                ntc.SetStrandId(-1);
                ntc.ResetColor();
            }
            else
            {
                nucleotides[i].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
            }
        }
    }
}
