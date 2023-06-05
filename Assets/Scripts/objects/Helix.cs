/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Helix object keeps track of nucleotides in the helix.
/// </summary>
public class Helix
{
    //constants
    private const float OFFSET = 0.034f;
    private const float RISE = 0.034f;
    private const int LENGTH = 64;

    // Helix id.
    private int _id;
    private Vector3 _startPoint;
    private Vector3 _endPoint;

    // Number of nucleotides in helix.
    private int _length;
    private string _orientation;

    // 3D position of grid circle.
    private Vector3 _gridPoint;

    // List containing all nucleotides in spiral going in direction 1.
    private List<GameObject> _nucleotidesA;

    // List containing all backbones in spiral going in direction 1.
    private List<GameObject> _backbonesA;

    // List containing all nucleotides in spiral going in direction 0.
    private List<GameObject> _nucleotidesB;

    // List containing all backbones in spiral going in direction 0.
    private List<GameObject> _backbonesB;

    // List of strand ids created on helix.
    private List<int> _strandIds;

    // Positions of last nucleotides in helix
    private Vector3 lastPositionA;
    private Vector3 lastPositionB;

    // Helix constructor.
    public Helix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, Vector3 gridPoint)
    {
        _id = id;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _length = 0;
        _orientation = orientation;
        _gridPoint = gridPoint;
        _nucleotidesA = new List<GameObject>();
        _backbonesA = new List<GameObject>();
        _nucleotidesB = new List<GameObject>();
        _backbonesB = new List<GameObject>();
        _strandIds = new List<int>();
        lastPositionA = _gridPoint - new Vector3(0, OFFSET, 0);
        lastPositionB = _gridPoint + new Vector3(0, OFFSET, 0);
        Extend();
        s_helixDict.Add(id, this);
    }

    /// <summary>
    /// Draws the nucleotides of the helix.
    /// </summary>
    public void Extend()
    {
        int prevLength = _length;
        _length += LENGTH;
        for (int i = prevLength; i < _length; i++)
        {
            GameObject sphereA = DrawPoint.MakeNucleotide(lastPositionA, i, _id, 1);
            sphereA.SetActive(false);
            _nucleotidesA.Add(sphereA);

            GameObject sphereB = DrawPoint.MakeNucleotide(lastPositionB, i, _id, 0);
            sphereB.SetActive(false);
            _nucleotidesB.Add(sphereB);

            float angleA = i * (2 * (float)(Math.PI) / 10); // rotation per bp in radians
            float angleB = (float)(((float)(i) + 5.5) * (2 * (float)(Math.PI) / 10));
            float axisOneChangeA = Mathf.Cos(angleA) * 0.02f;
            float axisTwoChangeA = Mathf.Sin(angleA) * 0.02f;
            float axisOneChangeB = Mathf.Cos(angleB) * 0.02f;
            float axisTwoChangeB = Mathf.Sin(angleB) * 0.02f;

            lastPositionA += new Vector3(axisOneChangeA, axisTwoChangeA, -RISE);
            lastPositionB += new Vector3(axisOneChangeB, axisTwoChangeB, -RISE);
        }

        if (prevLength == 0) 
        { 
            DrawBackbones(prevLength + 1); 
        }
        else
        {
            // Needs to add backbone to connect previous set of nucleotides
            DrawBackbones(prevLength);
        }
    }

    /// <summary>
    /// Draws the backbones between the nucleotides in the helix.
    /// </summary>
    /// <param name="start">Start index of nucleotide to begin drawing backbones.</param>
    private void DrawBackbones(int start)
    {
        // Backbones for A nucleotides
        for (int i = start; i < _nucleotidesA.Count; i++)
        {
            GameObject cylinder = DrawPoint.MakeBackbone(i - 1, _nucleotidesA[i].transform.position, _nucleotidesA[i - 1].transform.position);
            cylinder.SetActive(false);
            _backbonesA.Add(cylinder);
        }

        // Backbones for B nucleotides
        for (int i = start; i < _nucleotidesB.Count; i++)
        {
            GameObject cylinder = DrawPoint.MakeBackbone(i - 1, _nucleotidesB[i].transform.position, _nucleotidesB[i - 1].transform.position);
            cylinder.SetActive(false);
            _backbonesB.Add(cylinder);
        }
    }

    /// <summary>
    /// Returns sublist of nucleotides from helix spiral.
    /// </summary>
    /// <param name="sIndex">Start index of sublist.</param>
    /// <param name="eIndex">End index of sublist.</param>
    /// <param name="direction">Spiral direction (determines nucleotidesA or nucleotidesB).</param>
    /// <returns>Returns sublist of nucleotides from helix spiral.</returns>
    public List<GameObject> GetHelixSub(int sIndex, int eIndex, int direction)
    {
        List<GameObject> temp = new List<GameObject>();
        if (direction == 0)
        {
            for (int i = sIndex; i < eIndex; i++)
            {
                temp.Add(_nucleotidesB[i]);
                temp.Add(_backbonesB[i]);
            }
            temp.Add(_nucleotidesB[eIndex]);
            return temp;
        }
        else 
        {
            for (int i = sIndex; i < eIndex; i++)
            {
                temp.Add(_nucleotidesA[i]);
                temp.Add(_backbonesA[i]);
            }
            temp.Add(_nucleotidesA[eIndex]);
            temp.Reverse();
            return temp;
        }
    }

    /// <summary>
    /// Returns nucleotide in front of head nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of the helix, 0 or 1.</param>
    /// <returns>Returns nucleotide in front of head nucleotide.</returns>
    public GameObject GetHeadNeighbor(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = _nucleotidesB.IndexOf(go);
            return _nucleotidesB[index - 1];
        }
        else
        {
            int index = _nucleotidesA.IndexOf(go);
            return _nucleotidesA[index + 1];
        }
    }

    /// <summary>
    /// Returns nucleotide behind tail nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of helix, 0 or 1.</param>
    /// <returns>Returns nucleotide behind tail nucleotide.</returns>
    public GameObject GetTailNeighbor(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = _nucleotidesB.IndexOf(go);
            return _nucleotidesB[index + 1];
        }
        else
        {
            int index = _nucleotidesA.IndexOf(go);
            return _nucleotidesA[index - 1];
        }
    }

    /// <summary>
    /// Returns backbone in front of head nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of helix, 0 or 1.</param>
    /// <returns>Returns backbone in front of head nucleotide.</returns>
    public GameObject GetHeadBackbone(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = _nucleotidesB.IndexOf(go);
            return _backbonesB[index - 1];
        }
        else
        {
            int index = _nucleotidesA.IndexOf(go);
            return _backbonesA[index];
        }
    }

    /// <summary>
    /// Returns backbone in behind tail nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of helix, 0 or 1.</param>
    /// <returns>Returns backbone in behind tail nucleotide.</returns>
    public GameObject GetTailBackbone(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = _nucleotidesB.IndexOf(go);
            return _backbonesB[index];
        }
        else
        {
            int index = _nucleotidesA.IndexOf(go);
            return _backbonesA[index - 1];
        }
    }

    // Adds strandId to strand id list.
    public void AddStrandId(int strandId)
    {
        _strandIds.Add(strandId);
    }

    /// <summary>
    /// Returns the length of the helix.
    /// </summary>
    /// <returns>Lenght of the helix</returns>
    public int GetLength()
    {
        return _length;
    }

    // Removes strandId from strand id list.
    public void DeleteStrandId(int strandId)
    {
        _strandIds.Remove(strandId);
    }

    public void Highlight(Color color)
    {
        for (int i = 0; i < _nucleotidesA.Count; i++)
        {
            _nucleotidesA[i].GetComponent<NucleotideComponent>().Highlight(color);
            _nucleotidesB[i].GetComponent<NucleotideComponent>().Highlight(color);
        }
        for (int i = 0; i < _backbonesA.Count; i++)
        {
            _backbonesA[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
            _backbonesB[i].GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
        }
        for (int i = 0; i < _strandIds.Count; i++)
        {
            s_strandDict[i].HighlightCone(color);
        }
    }

    /// <summary>
    /// Shows and hides helix and its components (cones).
    /// </summary>
    /// <param name="enabled">Boolean for whether gameobjects are hidden or not.</param>
    public void ShowHideHelix(bool enabled)
    {
        for (int i = 0; i < _backbonesA.Count; i++)
        {
            _nucleotidesA[i].SetActive(enabled);
            _backbonesA[i].SetActive(enabled);
            _nucleotidesB[i].SetActive(enabled);
            _backbonesB[i].SetActive(enabled);
        }
        _nucleotidesA[_nucleotidesA.Count - 1].SetActive(enabled);
        _nucleotidesB[_nucleotidesB.Count - 1].SetActive(enabled);

        for (int i = 0; i < _strandIds.Count; i++)
        {
            Strand strand = s_strandDict[_strandIds[i]];
            strand.ShowHideCone(enabled);
        }
    }
}

