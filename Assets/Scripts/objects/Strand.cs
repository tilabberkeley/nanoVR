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
    public int Id { get { return _strandId; } }

    //public int HelixId { get { return _head.GetComponent<NucleotideComponent>().HelixId; } }

    // This strand's color.
    private Color _color;
    public Color Color { get { return _color; } }

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
    private List<GameObject> _xovers = new List<GameObject>();
    public List<GameObject> Xovers { get { return _xovers; } }

    // List of this strand's crossover suggestions.
    private List<GameObject> _xoverSuggestions;

    private GameObject _bezier;

    // private String _sequence;
    //private bool _assignedSequence = false;

    public bool AssignedSequence { get { return _head.GetComponent<NucleotideComponent>().Sequence != ""; } }//set { _assignedSequence = value; } }

    public string Sequence 
    { 
        get 
        {
            string sequence = "";
            for (int i = _nucleotides.Count - 1; i >= 0; i--)
            {
                var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
                if (ntc != null)
                {
                    sequence += ntc.Sequence;
                }
            }
            return sequence;
        }
        set { SetSequence(value); }//SetComplementary(); } 
    }

    private bool _isScaffold = false;
    public bool IsScaffold 
    { 
        get { return _isScaffold; } 
        set 
        { 
            _isScaffold = value; 
            if (value)
            {
                _color = Color.blue;
            }
            else
            {
                _color = Colors[s_numStrands % Colors.Length];
            }
            SetComponents(); // Updates all strand objects colors
        } 
    }

    public int Length 
    { 
        get 
        {
            int count = 0;
            foreach (GameObject nucl in _nucleotides)
            {
                var ntc = nucl.GetComponent<NucleotideComponent>();
                if (ntc != null)
                {
                    count += 1 + ntc.Insertion;
                }
            }
            return count;
        }
    }

    // Whether or not strand is circular
    private bool _isCircular = false;
    public bool IsCircular { get { return _isCircular; } set { _isCircular = value; } }


    // Strand constructor.
    public Strand(List<GameObject> nucleotides, int strandId, Color color)
    {
        _nucleotides = new List<GameObject>(nucleotides);
        _strandId = strandId;
        _color = color;
        _head = _nucleotides[0];
        _tail = _nucleotides.Last();
        _cone = DrawPoint.MakeCone();
        _bezier = null;
        _direction = nucleotides[0].GetComponent<NucleotideComponent>().Direction;
        //SetComponents();
        //s_strandDict.Add(strandId, this);
        //CheckForXoverSuggestions();
        //s_numStrands += 1;
    }

    public List<GameObject> GetNucleotides(int startIndex, int endIndex) 
    {
        int count = Math.Abs(endIndex - startIndex) + 1;
        return _nucleotides.GetRange(startIndex, count); 
    }

    // Returns head GameObject.
    public GameObject Head { get { return _head; } }

    // Returns tail GameObject.
    public GameObject Tail { get { return _tail; } }

    
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
        _cone.SetActive(enabled);
    }

    public void ShowHideXovers(bool enabled)
    {
        foreach (GameObject xover in _xovers)
        {
            xover.SetActive(enabled);
        }
    }

    // Adds crossover to crossover list.
    /*public void AddXover(GameObject xover) 
    { 
        _xovers.Add(xover);
    }

    public void AddXovers(List<GameObject> xovers)
    {
        _xovers.AddRange(xovers);
    }*/

    /*public void RemoveXovers(List<GameObject> xovers)
    {
        _xovers.RemoveAll(xover => xovers.Contains(xover));
    }

    public bool HasXover()
    {
        return _xovers.Count > 0;
    }*/

    // Adds GameObject to front of nucleotide list.
    public void AddToHead(GameObject newNucl)
    {
        _nucleotides.Insert(0, newNucl);
        _head = _nucleotides[0];
        SetCone();
        UpdateXovers();
        //_cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
    }

    // Adds list of GameObjects to front of nucleotide list.
    public void AddToHead(List<GameObject> newNucls) 
    {
        _nucleotides.InsertRange(0, newNucls);
        _head = _nucleotides[0];
        //_cone.transform.position = _head.transform.position;
        SetCone();
        UpdateXovers();
        //CheckForXoverSuggestions();
    }

    // Adds GameObject to end of nucleotide list.
    public void AddToTail(GameObject newNucl)
    {
        _nucleotides.Add(newNucl);
        _tail = _nucleotides.Last();
        UpdateXovers();
    }

    // Adds list of GameObjects to end of nucleotide list.
    public void AddToTail(List<GameObject> newNucls)
    {
        _nucleotides.AddRange(newNucls);
        _tail = _nucleotides.Last();
        UpdateXovers();
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
        UpdateXovers();
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
        UpdateXovers();
        ResetComponents(nucleotides);
    }

    /// <summary>
    /// Removes strand and resets all GameObjects.
    /// </summary>
    public void RemoveStrand()
    {
        GameObject.Destroy(_cone);
        s_strandDict.Remove(_strandId);
    }

    public void DeleteStrand()
    {
        ResetComponents(_nucleotides);
        DeleteXovers();
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

        splitList.AddRange(_nucleotides.GetRange(0, splitIndex));
        if (splitList.Count % 2 == 0)
        {
            GameObject backbone = splitList[splitList.Count - 1];
            splitList.Remove(backbone);
            backbone.GetComponent<BackBoneComponent>().ResetComponent();
        }
      
        _nucleotides.RemoveRange(0, splitIndex);
        _head = _nucleotides[0];
        SetCone();
        UpdateXovers();
        return splitList;
    }

    public void SplitCircularBefore(GameObject go)
    {
        int splitIndex = _nucleotides.IndexOf(go);
        int tailIndex = _nucleotides.IndexOf(_tail);
        GameObject backbone = _nucleotides[splitIndex - 1];
        backbone.GetComponent<BackBoneComponent>().ResetComponent();
        List<GameObject> removed = _nucleotides.GetRange(splitIndex, tailIndex - splitIndex + 1);
        _nucleotides.RemoveRange(splitIndex, tailIndex - splitIndex + 1);
        _nucleotides.InsertRange(0, removed);
        _head = _nucleotides[0];
        _tail = _nucleotides.Last();
        _isCircular = false;
        ShowHideCone(true);
        SetCone();
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
        int count = _nucleotides.Count - splitIndex - 1;
        
        splitList.AddRange(_nucleotides.GetRange(splitIndex + 1, count));
        if (splitList.Count % 2 == 0)
        {
            GameObject backbone = splitList[0];
            splitList.Remove(backbone);
            backbone.GetComponent<BackBoneComponent>().ResetComponent();
        }
      
        _nucleotides.RemoveRange(splitIndex + 1, count);
        _tail = _nucleotides.Last();
        UpdateXovers();
        return splitList;
    }

    public void SplitCircularAfter(GameObject go)
    {
        int splitIndex = _nucleotides.IndexOf(go);
        int tailIndex = _nucleotides.IndexOf(_tail);
        GameObject backbone = _nucleotides[splitIndex + 1];
        backbone.GetComponent<BackBoneComponent>().ResetComponent();
        List<GameObject> removed = _nucleotides.GetRange(splitIndex + 2, tailIndex - (splitIndex + 2) + 1);
        _nucleotides.RemoveRange(splitIndex + 2, tailIndex - (splitIndex + 2) + 1);
        _nucleotides.InsertRange(0, removed);
        _head = _nucleotides[0];
        _tail = _nucleotides.Last();
        _isCircular = false;
        ShowHideCone(true);
        SetCone();
    }

    public void DeleteXovers()
    {
        for (int i = 0; i < _nucleotides.Count; i++)
        {
            var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
            if (ntc != null && ntc.HasXover)
            {
                GameObject.Destroy(ntc.Xover);
                ntc.Xover = null;
            }
        }
        _xovers.Clear();
    }

    public void DeleteXover(GameObject xover)
    {
        var xoverComp = xover.GetComponent<XoverComponent>();
        var prevNtc = xoverComp.PrevGO.GetComponent<NucleotideComponent>();
        var nextNtc = xoverComp.NextGO.GetComponent<NucleotideComponent>();
        prevNtc.Xover = null;
        nextNtc.Xover = null;
        _xovers.Remove(xover);
        GameObject.Destroy(xover);
    }


    // Sets variables of each GameObject's component (strandId, color, etc).
    public void SetComponents()
    {
        for (int i = _nucleotides.Count - 1; i >= 0; i--)
        {
            var dnaComp = _nucleotides[i].GetComponent<DNAComponent>();
            var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
            dnaComp.Selected = true;
            dnaComp.StrandId = _strandId;
            dnaComp.Color = _color;
            
            if (ntc != null && ntc.HasXover)
            {
                var xoverComp = ntc.Xover.GetComponent<XoverComponent>();
                xoverComp.Color = _color;
            }
        }
        /*for (int i = 0; i < _xovers.Count; i++)
        {
            SetXoverColor(_xovers[i]);
        }*/
        //SetSequence();
        SetCone();
        UpdateXovers();
    }

    public void SetSequence(string sequence)
    {
        if (sequence.Equals("")) return;

        int strandLength = this.Length;
        if (sequence.Length < strandLength)
        {
            for (int i = 0; i < strandLength - sequence.Length; i++)
            {
                sequence += "?";
            }
        }

        int seqCount = 0;
        for (int i = _nucleotides.Count - 1; i >= 0; i--)
        {
            var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
            // Sets DNA sequence to nucleotides
            if (ntc != null)
            {
                if (ntc.IsDeletion)
                {
                    ntc.Sequence = "X";
                }
                else
                {
                    ntc.Sequence = sequence.Substring(seqCount, ntc.Insertion + 1);
                    seqCount += ntc.Insertion + 1;
                }
                Utils.CheckMismatch(_nucleotides[i]);
            }
        }
        //_assignedSequence = true;
    }

    // Sets cone position and rotation (pointing left or right).
    public void SetCone()
    { 
        _cone.GetComponent<ConeComponent>().Color = _color;
        int helixId = _head.GetComponent<NucleotideComponent>().HelixId;
        GameObject neighbor;
        if (s_visualMode)
        {
            neighbor = s_visHelixDict[helixId].GetHeadNeighbor(_head, _head.GetComponent<NucleotideComponent>().Direction);
        }
        else
        {
            neighbor = s_helixDict[helixId].GetHeadNeighbor(_head, _head.GetComponent<NucleotideComponent>().Direction);
        }
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
        //_head.GetComponent<NucleotideComponent>().Cone = _cone;
    }

    // Resets all GameObject components in the nucleotides list.
    public void ResetComponents(List<GameObject> nucleotides)
    {
        if (nucleotides == null) { return; }
        for (int i = 0; i < nucleotides.Count; i++)
        {
            var dnaComp = nucleotides[i].GetComponent<DNAComponent>();
            dnaComp.ResetComponent();

            var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            if (ntc != null)
            { 
                ntc.ResetComponent(); 
            }
        }
    }

    private void UpdateXovers()
    {
        _xovers.Clear();
        for (int i = 0; i < _nucleotides.Count; i++)
        {
            var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
            if (ntc != null && ntc.HasXover)
            {
                _xovers.Add(ntc.Xover);
                i++;
            }
        }
    }

    public bool MoreThanOneGrid()
    {
        string gridId = _head.GetComponent<DNAComponent>().GridId;
        for (int i = _nucleotides.Count - 1; i >= 0; i--)
        {
            var dnaComp = _nucleotides[i].GetComponent<DNAComponent>();
            if (!dnaComp.GridId.Equals(gridId))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets all the nucleotides of the strand.
    /// </summary>
    /// <returns>List of nucleotides (their components, not as gameObjects).</returns>
    public List<NucleotideComponent> GetNucleotidesOnly()
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
            _bezier = DrawPoint.MakeBezier(_nucleotides, _color);
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
