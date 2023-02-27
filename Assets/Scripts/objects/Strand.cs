/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

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
    private static Color[] s_colors = { Color.blue, Color.magenta, Color.red, Color.green, Color.yellow, Color.cyan };
    private System.Random random = new System.Random();

    public Strand(List<GameObject> nucleotides, int strandId, int direction)
    {
        _nucleotides = nucleotides;
        _strandId = strandId;
        _direction = direction;
        _color = s_colors[random.Next(0, s_colors.Length)];
        _head = nucleotides[0];
        _tail = nucleotides.Last();
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

    public int RemoveFromHead(List<GameObject> nucleotides)
    {
        foreach (GameObject nucl in nucleotides)
        {
            _nucleotides.Remove(nucl);
        }
        _head = _nucleotides[0];
        return _nucleotides.Count;
    }

    public int RemoveFromTail(List<GameObject> nucleotides)
    {
        foreach (GameObject nucl in nucleotides)
        {
            _nucleotides.Remove(nucl);
        }
        _tail = _nucleotides.Last();
        return _nucleotides.Count;
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
            var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
            ntc.SetSelected(true);
            ntc.SetStrandId(_strandId);
            ntc.SetColor(_color);
        }
    }

    public void ResetComponents(List<GameObject> nucleotides)
    {
        for (int i = 0; i < _nucleotides.Count; i++)
        {
            var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
            ntc.SetSelected(false);
            ntc.SetStrandId(-1);
            ntc.ResetColor();
        }
    }
}
