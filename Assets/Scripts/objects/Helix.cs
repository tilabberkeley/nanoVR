/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;

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
    private List<Vector3> _ntAPos;
    private List<GameObject> _nucleotidesA;
    private List<GameObject> _backbonesA;
    private List<Vector3> _ntBPos;
    private List<GameObject> _nucleotidesB;
    private List<GameObject> _backbonesB;

    public Helix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, Vector3 gridPoint)
    {
        _id = id;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _length = 32;
        _orientation = orientation;
        _gridPoint = gridPoint;
        _ntAPos = new List<Vector3>();
        _nucleotidesA = new List<GameObject>();
        _backbonesA = new List<GameObject>();
        _ntBPos = new List<Vector3>();
        _nucleotidesB = new List<GameObject>();
        _backbonesB = new List<GameObject>();
        HelixFormation();
        DrawBackbone(_ntAPos, _backbonesA);
        DrawBackbone(_ntBPos, _backbonesB);
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
            _ntAPos.Add(targetPositionA);
            _nucleotidesA.Add(sphereA);

            GameObject sphereB = d.MakeNucleotide(targetPositionB, i, _id, 0);
            sphereB.GetComponent<Renderer>().enabled = false;
            _ntBPos.Add(targetPositionB);
            _nucleotidesB.Add(sphereB);

            float angleA = i * (2 * (float)(Math.PI) / 10); // rotation per bp in radians
            float angleB = (float)(((float)(i) + 5.5) * (2 * (float)(Math.PI) / 10));
            float axisOneChangeA = Mathf.Cos(angleA) * 0.02f;
            float axisTwoChangeA = Mathf.Sin(angleA) * 0.02f;
            float axisOneChangeB = Mathf.Cos(angleB) * 0.02f;
            float axisTwoChangeB = Mathf.Sin(angleB) * 0.02f;

            if (_orientation.Equals("XY"))
            {
                targetPositionA = new Vector3(targetPositionA.x + axisOneChangeA, targetPositionA.y + axisTwoChangeA, targetPositionA.z - RISE);
                targetPositionB = new Vector3(targetPositionB.x + axisOneChangeB, targetPositionB.y + axisTwoChangeB, targetPositionB.z - RISE);
            }
        }
    }

    public void DrawBackbone(List<Vector3> ntPos, List<GameObject> backbones)
    {
        for (int i = 1; i < ntPos.Count; i++)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.layer = LayerMask.NameToLayer("Ignore Raycast");
            Vector3 cylinderDefaultOrientation = new Vector3(0, 1, 0);

            // Position
            cylinder.transform.position = (ntPos[i] + ntPos[i - 1]) / 2.0F;

            // Rotation
            Vector3 dirV = Vector3.Normalize(ntPos[i] - ntPos[i - 1]);
            Vector3 rotAxisV = dirV + cylinderDefaultOrientation;
            rotAxisV = Vector3.Normalize(rotAxisV);
            cylinder.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);

            // Scale        
            float dist = Vector3.Distance(ntPos[i], ntPos[i - 1]);
            cylinder.transform.localScale = new Vector3(0.005f, dist / 2, 0.005f);
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

    public void ShowHelix()
    {
        for (int i = 0; i < _backbonesA.Count; i++)
        {
            _nucleotidesA[i].GetComponent<Renderer>().enabled = true;
            _backbonesA[i].GetComponent<Renderer>().enabled = true;
            _nucleotidesB[i].GetComponent<Renderer>().enabled = true;
            _backbonesB[i].GetComponent<Renderer>().enabled = true;
        }
        _nucleotidesA[_nucleotidesA.Count - 1].GetComponent<Renderer>().enabled = true;
        _nucleotidesB[_nucleotidesB.Count - 1].GetComponent<Renderer>().enabled = true;
    }

    public void HideHelix()
    {
        for (int i = 0; i < _backbonesA.Count; i++)
        {
            _nucleotidesA[i].GetComponent<Renderer>().enabled = false;
            _backbonesA[i].GetComponent<Renderer>().enabled = false;
            _nucleotidesB[i].GetComponent<Renderer>().enabled = false;
            _backbonesB[i].GetComponent<Renderer>().enabled = false;
        }
        _nucleotidesA[_nucleotidesA.Count - 1].GetComponent<Renderer>().enabled = false;
        _nucleotidesB[_nucleotidesB.Count - 1].GetComponent<Renderer>().enabled = false;
    }
}

