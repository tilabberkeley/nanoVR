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
    private int _id;
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private int _length;
    private string _orientation;

    private Vector3 _gridPoint;
    private List<GameObject> _nucleotidesA;
    private List<GameObject> _backbonesA;
    private List<GameObject> _nucleotidesB;
    private List<GameObject> _backbonesB;
    private List<int> _strandIds;

    public Helix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, Vector3 gridPoint)
    {
        _id = id;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _length = 64;
        _orientation = orientation;
        _gridPoint = gridPoint;
        _nucleotidesA = new List<GameObject>();
        _backbonesA = new List<GameObject>();
        _nucleotidesB = new List<GameObject>();
        _backbonesB = new List<GameObject>();
        _strandIds = new List<int>();
        HelixFormation();
        DrawBackbone(_nucleotidesA, _backbonesA);
        DrawBackbone(_nucleotidesB, _backbonesB);
        //SetNeighbors(_nucleotidesA, _nucleotidesB, _backbonesA);
        //SetNeighbors(_nucleotidesB, _nucleotidesA, _backbonesB);
    }

    public void HelixFormation()
    {
        float OFFSET = 0.034f; // helical radius
        float RISE = 0.034f; // vertical rise per bp

        // **CHECK THIS**
        Vector3 targetPositionA = _gridPoint - new Vector3(0, OFFSET, 0);
        Vector3 targetPositionB = _gridPoint + new Vector3(0, OFFSET, 0);
        DrawPoint d = new DrawPoint();
        for (int i = 0; i < _length; i++)
        {      
            GameObject sphereA = d.MakeNucleotide(targetPositionA, i, _id, 1);
            sphereA.GetComponent<Renderer>().enabled = false;
            _nucleotidesA.Add(sphereA);

            GameObject sphereB = d.MakeNucleotide(targetPositionB, i, _id, 0);
            sphereB.GetComponent<Renderer>().enabled = false;
            _nucleotidesB.Add(sphereB);

            float angleA = i * (2 * (float)(Math.PI) / 10); // rotation per bp in radians
            float angleB = (float)(((float)(i) + 5.5) * (2 * (float)(Math.PI) / 10));
            float axisOneChangeA = Mathf.Cos(angleA) * 0.02f;
            float axisTwoChangeA = Mathf.Sin(angleA) * 0.02f;
            float axisOneChangeB = Mathf.Cos(angleB) * 0.02f;
            float axisTwoChangeB = Mathf.Sin(angleB) * 0.02f;

            
            targetPositionA += new Vector3(axisOneChangeA, axisTwoChangeA, -RISE);
            targetPositionB += new Vector3(axisOneChangeB, axisTwoChangeB, -RISE);
            
        }
    }

    public void DrawBackbone(List<GameObject> nucleotides, List<GameObject> backbones)
    {
        DrawPoint d = new DrawPoint();
        for (int i = 1; i < nucleotides.Count; i++)
        {
            GameObject cylinder = d.MakeBackbone(i - 1, nucleotides[i].transform.position, nucleotides[i - 1].transform.position);
            cylinder.GetComponent<Renderer>().enabled = false;
            backbones.Add(cylinder);
        }
    }

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
    /// Return's neighbo in front of head nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of the helix, 0 or 1.</param>
    /// <returns>Return's neighbor in front of head nucleotide.</returns>
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
    /// Return's neighbor behind tail nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of helix, 0 or 1.</param>
    /// <returns>Return's neighbor behind tail nucleotide.</returns>
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

    public void AddStrandId(int strandId)
    {
        _strandIds.Add(strandId);
    }

    /*public void SetNeighbors(List<GameObject> nucleotides, List<GameObject> complements, List<GameObject> backbones)
    {
    for (int i = 0; i < nucleotides.Count; i++)
    {
        var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
        if (i < nucleotides.Count - 1)
        {
        ntc.SetNextGO(nucleotides[i + 1]);
        ntc.SetNextBB(backbones[i]);
        }
        if (i > 0)
        {
        ntc.SetPrevGO(nucleotides[i - 1]);
        ntc.SetPrevBB(backbones[i - 1]);
        }
        ntc.SetComplementGO(complements[i]);
    }
    }*/


    /// <summary>
    /// Shows and hides helix and its components (cones).
    /// </summary>
    /// <param name="enabled">Boolean for whether gameobjects are hidden or not.</param>
    public void ShowHideHelix(bool enabled)
    {
        for (int i = 0; i < _backbonesA.Count; i++)
        {
            _nucleotidesA[i].GetComponent<Renderer>().enabled = enabled;
            _backbonesA[i].GetComponent<Renderer>().enabled = enabled;
            _nucleotidesB[i].GetComponent<Renderer>().enabled = enabled;
            _backbonesB[i].GetComponent<Renderer>().enabled = enabled;
        }
        _nucleotidesA[_nucleotidesA.Count - 1].GetComponent<Renderer>().enabled = enabled;
        _nucleotidesB[_nucleotidesB.Count - 1].GetComponent<Renderer>().enabled = enabled;

        for (int i = 0; i < _strandIds.Count; i++)
        {
            Strand strand = s_strandDict[_strandIds[i]];
            strand.ShowHideCone(enabled);
        }
    }
}

