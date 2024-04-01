/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;
using static Utils;

/// <summary>
/// Helix object keeps track of nucleotides in the helix.
/// </summary>
public class Helix
{
    private const float ADJUSTMENT = 0.01f; // Accounts for Icosphere prefab's weird positioning 
    // Helix id.
    private int _id;
    public int Id { get { return _id; } set { _id = value; } }

    public string GridId { get { return _gridComponent.GridId; } }

    //private Vector3 _startPoint;
    public Vector3 StartPoint { get { return _gridComponent.Position ; } }

    private Vector3 _endPoint;
    public Vector3 EndPoint { get { return _endPoint; } set { _endPoint = value; } }

    private string _orientation;
    public string Orientation { get { return _orientation; } }

    // Number of nucleotides in helix.
    private int _length;
    public int Length { get { return _length; } }

    // Grid Component that helix is on.
    public GridComponent _gridComponent;

    // List containing all nucleotides in spiral going in direction 1.
    private List<GameObject> _nucleotidesA;
    public List<GameObject> NucleotidesA { get { return _nucleotidesA; } }


    // List containing all backbones in spiral going in direction 1.
    private List<GameObject> _backbonesA;
    public List<GameObject> BackbonesA { get { return _backbonesA; } }

    // List containing all nucleotides in spiral going in direction 0.
    private List<GameObject> _nucleotidesB;
    public List<GameObject> NucleotidesB { get { return _nucleotidesB; } }

    // List containing all backbones in spiral going in direction 0.
    private List<GameObject> _backbonesB;
    public List<GameObject> BackbonesB { get { return _backbonesB; } }

    // List of strand ids created on helix.
    private List<int> _strandIds;
    public List<int> StrandIds { get { return _strandIds; } }

    // Positions of last nucleotides in helix
    private Vector3 _lastPositionA;
    private Vector3 _lastPositionB;

    // Helix constructor.
    public Helix(int id, string orientation, int length, GridComponent gridComponent)
    {
        _id = id;
        _length = 0;
        _orientation = orientation;
        _gridComponent = gridComponent;
        _nucleotidesA = new List<GameObject>();
        _backbonesA = new List<GameObject>();
        _nucleotidesB = new List<GameObject>();
        _backbonesB = new List<GameObject>();
        _strandIds = new List<int>();
        _lastPositionA = Vector3.zero;
        _lastPositionB = Vector3.zero;
        Extend(length);
        //ChangeRendering();
    }

    /// <summary>
    /// Draws the nucleotides of the helix.
    /// </summary>
    public void Extend(int length)
    {
        int prevLength = _length;
        _length += length;
        for (int i = prevLength; i < _length; i++)
        {
            Vector3 position = StartPoint;
            Vector3 direction = Vector3.Normalize(_gridComponent.gameObject.transform.rotation.eulerAngles);
            float angleA = (float) (i * (2 * Math.PI / NUM_BASE_PAIRS)); // rotation per bp in radians
            float angleB = (float) ((i + 4.5f) * (2 * Math.PI / NUM_BASE_PAIRS)); //TODO: check this new offset
            float cosAngleA = (float) Math.Cos(angleA);
            float sinAngleA = (float) Math.Sin(angleA);
            float cosAngleB = (float) Math.Cos(angleB);
            float sinAngleB = (float) Math.Sin(angleB);

            float xPointA = position.x + RADIUS * (cosAngleA * direction.x + sinAngleA * direction.y);
            float yPointA = position.y + RADIUS * (-sinAngleA * direction.x + cosAngleA * direction.y);
            float zPointA = position.z + RADIUS * direction.z * i;

            float xPointB = position.x + RADIUS * (cosAngleB * direction.x + sinAngleB * direction.y);
            float yPointB = position.y + RADIUS * (-sinAngleB * direction.x + cosAngleB * direction.y);
            float zPointB = position.z + RADIUS * direction.z * i;

            _lastPositionA = new Vector3(xPointA, yPointA, zPointA);
            _lastPositionB = new Vector3(xPointB, yPointB, zPointB);


            /*float angleA = (float) (i * (2 * Math.PI / NUM_BASE_PAIRS)); // rotation per bp in radians
            float angleB = (float) ((i + 4.5f) * (2 * Math.PI/ NUM_BASE_PAIRS)); //TODO: check this new offset
            float axisOneChangeA = (float) (RADIUS * Mathf.Cos(angleA));
            float axisTwoChangeA = (float) (RADIUS * Mathf.Sin(angleA));
            float axisOneChangeB = (float) (RADIUS * Mathf.Cos(angleB));
            float axisTwoChangeB = (float) (RADIUS * Mathf.Sin(angleB));

            if (_orientation.Equals("XY"))
            {
                _lastPositionA = StartPoint + new Vector3(axisOneChangeA, axisTwoChangeA - ADJUSTMENT, -i * RISE);
                _lastPositionB = StartPoint + new Vector3(axisOneChangeB, axisTwoChangeB - ADJUSTMENT, -i * RISE);
            }
            else if (_orientation.Equals("XZ"))
            {
                _lastPositionA = StartPoint + new Vector3(axisOneChangeA, i * RISE, axisTwoChangeA);
                _lastPositionB = StartPoint + new Vector3(axisOneChangeB, i * RISE, axisTwoChangeB);
            }
            else
            {
                _lastPositionA = StartPoint + new Vector3(i * RISE, axisOneChangeA, axisTwoChangeA);
                _lastPositionB = StartPoint + new Vector3(i * RISE, axisOneChangeB, axisTwoChangeB);
            }*/

            GameObject sphereA = DrawPoint.MakeNucleotide(_lastPositionA, i, _id, 1);
            _nucleotidesA.Add(sphereA);
            GameObject sphereB = DrawPoint.MakeNucleotide(_lastPositionB, i, _id, 0);
            _nucleotidesB.Add(sphereB);
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
            GameObject cylinder = DrawPoint.MakeBackbone(i - 1, _id, 1, NucleotidesA[i].transform.position, NucleotidesA[i - 1].transform.position);
            //cylinder.SetActive(false);
            _backbonesA.Add(cylinder);
        }

        // Backbones for B nucleotides
        for (int i = start; i < _nucleotidesB.Count; i++)
        {
            GameObject cylinder = DrawPoint.MakeBackbone(i - 1, _id, 0, NucleotidesB[i].transform.position, NucleotidesB[i - 1].transform.position);
            //cylinder.SetActive(false);
            _backbonesB.Add(cylinder);
        }
    }

    /// <summary>
    /// Returns sublist of nucleotides and backbones from helix spiral.
    /// </summary>
    /// <param name="sIndex">Start index of sublist.</param>
    /// <param name="eIndex">End index of sublist.</param>
    /// <param name="direction">Spiral direction (determines nucleotidesA or nucleotidesB).</param>
    /// <returns>Returns sublist of nucleotides from helix spiral.</returns>
    public List<GameObject> GetHelixSub(int sIndex, int eIndex, int direction)
    {
        if (sIndex < 0 || eIndex >= _nucleotidesA.Count)
        {
            Debug.Log("Nucleotides A length: " + _nucleotidesA.Count);
            return null;
        }
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

    public GameObject GetNucleotide(int id, int direction)
    {
        // Need to prevent indexOutOfBounds
        if (id < 0 || id >= NucleotidesA.Count) { return null; }
        if (direction == 0)
        {
            return _nucleotidesB[id];
        }
        else
        {
            return _nucleotidesA[id];
        }
    }

    public GameObject GetBackbone(int id, int direction)
    {
        if (direction == 0)
        {
            return _backbonesB[id];
        }
        else
        {
            return _backbonesA[id];
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
            if (index == 0)
            {
                return null;
            }
            return _nucleotidesB[index - 1];
        }
        else
        {
            int index = _nucleotidesA.IndexOf(go);
            if (index == _nucleotidesA.Count - 1)
            {
                return null;
            }
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
            if (index == _nucleotidesB.Count - 1)
            {
                return null;
            }
            return _nucleotidesB[index + 1];
        }
        else
        {
            int index = _nucleotidesA.IndexOf(go);
            if (index == 0)
            {
                return null;
            }
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

    public int NumModsToLeft(int id, int direction)
    {
        if (direction == 0)
        {
            int shiftCount = 0;
            for (int i = 0; i < id; i++)
            {
                var ntc = _nucleotidesB[i].GetComponent<NucleotideComponent>();
                shiftCount += ntc.Insertion;
                if (ntc.IsDeletion) { shiftCount -= 1; }
            }
            return shiftCount;
        }
        else
        {
            int shiftCount = 0;
            for (int i = 0; i < id; i++)
            {
                var ntc = _nucleotidesA[i].GetComponent<NucleotideComponent>();
                shiftCount += ntc.Insertion;
                if (ntc.IsDeletion) { shiftCount -= 1; }
            }
            return shiftCount;
        }
    }

    // Returns true if none of the helix's nucleotides are selected.
    // In other words, if there are no strands on the helix.
    public bool IsEmpty()
    {
        return IsEmpty(_nucleotidesA) && IsEmpty(_nucleotidesB) && IsEmpty(_backbonesA) && IsEmpty(_backbonesB);
    }

    // Helper method for IsEmpty().
    public bool IsEmpty(List<GameObject> lst)
    {
        foreach (GameObject nucleotide in lst)
        {
            DNAComponent dnaComponent = nucleotide.GetComponent<DNAComponent>();
            if (dnaComponent.Selected)
            {
                return false;
            }
        }
        return true;
    }

    public void ChangeStencilView()
    {
        ChangeStencilView(_nucleotidesA);
        ChangeStencilView(_nucleotidesB);
        ChangeStencilView(_backbonesA);
        ChangeStencilView(_backbonesB);
    }

    // Helper method to hide stencil.
    public void ChangeStencilView(List<GameObject> lst)
    {
        foreach (GameObject go in lst)
        {
            if (!go.GetComponent<DNAComponent>().Selected)
            {
                go.SetActive(s_hideStencils);
            }
        }
    }

    // Hides helix GameObjects in world.
    public void HideHelix()
    {
        HideObjects(_nucleotidesA);
        HideObjects(_nucleotidesB);
        HideObjects(_backbonesA);
        HideObjects(_backbonesB);
    }

    // Helper method that hides a list of GameObjects in world.
    public void HideObjects(List<GameObject> lst)
    {
        foreach (GameObject go in lst)
        {
            go.SetActive(false);
        }
    }

    /// <summary>
    /// Changes rendering of helix and its components (cones).
    /// </summary>
    public void ChangeRendering()
    {
        for (int i = 0; i < _backbonesA.Count; i++)
        {
            _nucleotidesA[i].SetActive(s_nucleotideView);
            _backbonesA[i].SetActive(s_nucleotideView);
            _nucleotidesB[i].SetActive(s_nucleotideView);
            _backbonesB[i].SetActive(s_nucleotideView);
        }
        NucleotidesA[NucleotidesA.Count - 1].SetActive(s_nucleotideView);
        NucleotidesB[NucleotidesB.Count - 1].SetActive(s_nucleotideView);
    }

    /// <summary>
    /// Returns the helices that neighbor this helix.
    /// </summary>
    /// <returns>List of neighboring helices.</returns>
    public List<Helix> getNeighborHelices()
    {
        List<Helix> helices = new List<Helix>();
        foreach (GridComponent gridComponent in _gridComponent.getNeighborGridComponents())
        {
            Helix helix = gridComponent.Helix;
            // helix != null if there is a helix on the grid component
            if (helix != null)
            {
                helices.Add(helix);
            }
        }
        return helices;
    }

    /// <summary>
    /// Moves all GameObjects in helix.
    /// </summary>
    /// <param name="diff">Vector3 specifying how much to move the helix.</param>
    public void MoveNucleotides(Vector3 diff)
    {
        foreach (GameObject nucleotide in NucleotidesA)
        {
            nucleotide.transform.position += diff;
            var ntc = nucleotide.GetComponent<NucleotideComponent>();
            Strand strand = null;
            if (ntc.Selected)
            {
                strand = s_strandDict[ntc.StrandId];
            }
            if (nucleotide.GetComponent<NucleotideComponent>().Xover != null)
            {
                MoveXover(nucleotide);
            }
            if (strand != null && strand.Head == nucleotide)
            {
                strand.SetCone();
            }
        }
        foreach (GameObject nucleotide in NucleotidesB)
        {
            nucleotide.transform.position += diff;
            var ntc = nucleotide.GetComponent<NucleotideComponent>();
            Strand strand = null;
            if (ntc.Selected)
            {
                strand = s_strandDict[ntc.StrandId];
            }
            if (nucleotide.GetComponent<NucleotideComponent>().Xover != null)
            {
                MoveXover(nucleotide);
            }
            if (strand != null && strand.Head == nucleotide)
            {
                strand.SetCone();
            }
        }
        foreach (GameObject backbone in BackbonesA)
        {
            backbone.transform.position += diff;
        }
        foreach (GameObject backbone in BackbonesB)
        {
            backbone.transform.position += diff;
        }
    }

    /// <summary>
    /// Helps redraw the xovers when a helix is moved to a new grid circle.
    /// </summary>
    /// <param name="nucleotide">Moved nucleotide GameObject which is attached to the xover being redrawn.</param>
    public void MoveXover(GameObject nucleotide)
    {
        var ntc = nucleotide.GetComponent<NucleotideComponent>();
        GameObject oldXover = ntc.Xover;
        var xoverComp = oldXover.GetComponent<XoverComponent>();
        DrawPoint.MakeXover(xoverComp.PrevGO, xoverComp.NextGO, ntc.StrandId);
        GameObject.Destroy(oldXover);
    }

    // Deletes helix and destroys all of its GameObjects.
    public void DeleteHelix()
    {
        _gridComponent.Helix = null;
        _gridComponent.Selected = false;
        s_helixDict.Remove(_id);
        foreach (GameObject nucleotide in NucleotidesA)
        {
            GameObject.Destroy(nucleotide);
        }
        foreach (GameObject nucleotide in NucleotidesB)
        {
            GameObject.Destroy(nucleotide);
        }
        foreach (GameObject nucleotide in BackbonesA)
        {
            GameObject.Destroy(nucleotide);
        }
        foreach (GameObject nucleotide in BackbonesB)
        {
            GameObject.Destroy(nucleotide);
        }
    }


    public void SetParent(GameObject go)
    {
        foreach (GameObject nucleotide in NucleotidesA)
        {
            nucleotide.transform.parent = go.transform;
        }
        foreach (GameObject nucleotide in NucleotidesB)
        {
            nucleotide.transform.parent = go.transform;
        }
        foreach (GameObject nucleotide in BackbonesA)
        {
            nucleotide.transform.parent = go.transform;
        }
        foreach (GameObject nucleotide in BackbonesB)
        {
            nucleotide.transform.parent = go.transform;
        }
    }

    public void ResetParent()
    {
        foreach (GameObject nucleotide in NucleotidesA)
        {
            nucleotide.transform.parent = null;
        }
        foreach (GameObject nucleotide in NucleotidesB)
        {
            nucleotide.transform.parent = null;
        }
        foreach (GameObject nucleotide in BackbonesA)
        {
            nucleotide.transform.parent = null;
        }
        foreach (GameObject nucleotide in BackbonesB)
        {
            nucleotide.transform.parent = null;
        }
    }
}

