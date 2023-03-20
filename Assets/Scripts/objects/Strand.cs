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
    private GameObject _cone;
    private GameObject _xover;
    private double _xoverLength;
    private static Color[] s_colors = { Color.blue, Color.magenta, Color.green, Color.red, Color.cyan, Color.yellow };

    public Strand(List<GameObject> nucleotides, int strandId, int direction)
    {
        _nucleotides = nucleotides;
        _strandId = strandId;
        _direction = direction;
        _color = s_colors[s_numStrands % 6];
        _head = nucleotides[0];
        _tail = nucleotides.Last();
        _xover = null;
        _xoverLength = 0;
        DrawPoint d = new DrawPoint();
        _cone = d.MakeCone(_head.transform.position, direction); 
    }

    public List<GameObject> GetNucleotides() { return _nucleotides; }
    public GameObject GetHead() { return _head; }
    public GameObject GetTail() { return _tail; }

    public void SetXover(GameObject xover) 
    { 
        if (_xover != null)
        {
            GameObject.Destroy(_xover);
        }
        _xover = xover;
        _xoverLength = xover.transform.position.y; 
    }


    public void AddToHead(GameObject newNucl)
    {
        _nucleotides.Insert(0, newNucl);
        _head = _nucleotides[0];
        _cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
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
            _cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
        }
        else
        {
            RemoveStrand();
        }
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
        else
        {
            RemoveStrand();
        }
        return nucleotides;
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
        splitList.AddRange(_nucleotides.GetRange(0, splitIndex - 1));
        _nucleotides[splitIndex - 1].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        _nucleotides.RemoveRange(0, splitIndex);
        _head = _nucleotides[0];
        _cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
        return splitList;
    }

    public List<GameObject> SplitAfter(GameObject go)
    {
        List<GameObject> splitList = new List<GameObject>();
        int splitIndex = _nucleotides.IndexOf(go);
        int count = _nucleotides.Count - splitIndex - 1;
        splitList.AddRange(_nucleotides.GetRange(splitIndex + 2, count - 1));
        _nucleotides[splitIndex + 1].GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        _nucleotides.RemoveRange(splitIndex + 1, count);
        _tail = _nucleotides.Last();
        return splitList;
    }


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
        _cone.GetComponent<Renderer>().material.SetColor("_Color", _color);
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
