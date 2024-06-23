/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using static GlobalVariables;
using static UnityEngine.EventSystems.EventTrigger;

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

    private List<NucleotideComponent> _nucleotidesOnly;
    public List<NucleotideComponent> NucleotidesOnly
    {
        get { return _nucleotidesOnly; }
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

    /// <summary>
    /// Whether or not xovers list has potentially changed since last Xovers call.
    /// </summary>
    private bool _xoversWasChanged = true;
    // List of this strand's crossovers and loopouts (needs testing).
    private List<GameObject> _xovers;
    public List<GameObject> Xovers 
    { 
        get 
        { 
            if (_xoversWasChanged)
            {
                _xoversWasChanged = false;
                UpdateXovers();
            }
            return _xovers;
        } 
    }

    // List of this strand's crossover suggestions.
    private List<GameObject> _xoverSuggestions;

    private List<GameObject> _beziers;

    private List<DomainComponent> _domains;
    public List<DomainComponent> Domains { get { return _domains; } }

    /// <summary>
    /// Whether or not _sequence has potentially changed since last Sequence call.
    /// </summary>
    private bool _sequenceWasChanged = true;
    private String _sequence;
    public string Sequence 
    { 
        get 
        {
            if (_sequenceWasChanged)
            {
                _sequenceWasChanged = false;
                StringBuilder sequence = new StringBuilder();
                for (int i = _nucleotides.Count - 1; i >= 0; i--)
                {
                    var ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
                    if (ntc != null)
                    {
                        sequence.Append(ntc.Sequence);
                    }
                }
                _sequence = sequence.ToString();
            }
            return _sequence;
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
            SetColors(); // Updates all strand objects colors
        } 
    }

    /// <summary>
    /// Whether or not _length has potentially changed since last Length call.
    /// </summary>
    private bool _lengthWasChanged = true;
    private int _length;
    public int Length 
    { 
        get 
        {
            if (_lengthWasChanged)
            {
                _lengthWasChanged = false;
                int count = 0;
                for (int i = _nucleotides.Count - 1; i >=0; i--)
                {
                    NucleotideComponent ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
                    if (ntc != null)
                    {
                        // Nucleotide
                        count += 1 + ntc.Insertion;
                    }
                    else
                    {
                        // Backbone
                        continue;
                    }

                    if (ntc.HasXover)
                    {
                        LoopoutComponent loopComp = ntc.Xover.GetComponent<LoopoutComponent>();
                        if (loopComp != null && loopComp.NextGO == ntc.gameObject)
                        {
                            count += loopComp.SequenceLength;
                        }
                    }
                }
                _length = count;
            }
            return _length;
        }
    }

    // Whether or not strand is circular
    private bool _isCircular = false;
    public bool IsCircular { get { return _isCircular; } set { _isCircular = value; } }


    // Strand constructor.
    public Strand(List<GameObject> nucleotides, int strandId, Color color)
    {
        _nucleotides = new List<GameObject>(nucleotides);
        _nucleotidesOnly = new List<NucleotideComponent>();
        SetNucleotidesOnly();
        _strandId = strandId;
        _color = color;
        _head = _nucleotides[0];
        _tail = _nucleotides.Last();
        _cone = DrawPoint.MakeCone();
        _beziers = new List<GameObject>();
        _direction = nucleotides[0].GetComponent<NucleotideComponent>().Direction;
        _xovers = new List<GameObject>();
        _domains = new List<DomainComponent>();
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
        _nucleotidesOnly.Insert(0, newNucl.GetComponent<NucleotideComponent>());
        _head = _nucleotides[0];
        SetCone();
        ResetDomains();
        _sequenceWasChanged = true;
        _lengthWasChanged = true;
        //_cone.transform.position = _head.transform.position + new Vector3(0.015f, 0, 0);
    }

    /// <summary>
    /// Adds list of GameObjects to front of nucleotide list.
    /// </summary>
    public void AddToHead(List<GameObject> newNucls) 
    {
        _nucleotides.InsertRange(0, newNucls);
        List<NucleotideComponent> nucleotideComponents = new List<NucleotideComponent>();
        newNucls.ForEach(n => nucleotideComponents.Add(n.GetComponent<NucleotideComponent>()));
        _nucleotidesOnly.InsertRange(0, nucleotideComponents);
        _head = _nucleotides[0];
        //_cone.transform.position = _head.transform.position;
        SetCone();
        ResetDomains();
        _xoversWasChanged = true;
        _sequenceWasChanged = true;
        _lengthWasChanged = true;

        // Append "?" to DNA sequence if this strand has been assigned a sequence and newNucls haven't.
        if (newNucls[0].GetComponent<NucleotideComponent>().Sequence.Equals("") 
            && !this._nucleotides[0].GetComponent<NucleotideComponent>().Sequence.Equals(""))
        {
            Utils.SetUnknownSequence(newNucls);
        }
        //CheckForXoverSuggestions();
    }

    // Adds GameObject to end of nucleotide list.
    public void AddToTail(GameObject newNucl)
    {
        _nucleotides.Add(newNucl);
        _nucleotidesOnly.Add(newNucl.GetComponent<NucleotideComponent>());
        _tail = _nucleotides.Last();
        _sequenceWasChanged = true;
        _lengthWasChanged = true;
    }

    /// <summary>
    /// Adds list of GameObjects to end of nucleotide list.
    /// </summary>
    public void AddToTail(List<GameObject> newNucls)
    {
        _nucleotides.AddRange(newNucls);
        List<NucleotideComponent> nucleotideComponents = new List<NucleotideComponent>();
        newNucls.ForEach(n => nucleotideComponents.Add(n.GetComponent<NucleotideComponent>()));
        _nucleotidesOnly.AddRange(nucleotideComponents);
        _tail = _nucleotides.Last();
        ResetDomains();
        _xoversWasChanged = true;
        _sequenceWasChanged = true;
        _lengthWasChanged = true;

        // Append "?" to DNA sequence if this strand has been assigned a sequence and newNucls haven't.
        if (newNucls[0].GetComponent<NucleotideComponent>().Sequence.Equals("")
            && !this._nucleotides[0].GetComponent<NucleotideComponent>().Sequence.Equals(""))
        {
            Utils.SetUnknownSequence(newNucls);
        }
        //CheckForXoverSuggestions();
    }

    /// <summary>
    /// Removes list of GameObjects from front of nucleotide list. If all nucleotides are removed, delete the strand. Reset components of removed nucleotides (reset strandId, color, etc).
    /// </summary>
    /// <param name="nucleotides">List of GameObjects being removed from nucleotide list.</param>
    public void RemoveFromHead(List<GameObject> nucleotides)
    {
        _nucleotides.RemoveAll(nucleotide => nucleotides.Contains(nucleotide));
        nucleotides.ForEach(n => _nucleotidesOnly.Remove(n.GetComponent<NucleotideComponent>()));
        if (_nucleotides.Count > 0)
        {
            _head = _nucleotides[0];
            SetCone();
        }
        ResetComponents(nucleotides);
        ResetDomains();
        _xoversWasChanged = true;
        _sequenceWasChanged = true;
        _lengthWasChanged = true;
    }

    /// <summary>
    /// Removes list of GameObjects from end of nucleotide list. If all nucleotides are removed, delete the strand. Reset components of removed nucleotides (reset strandId, color, etc).
    /// </summary>
    /// <param name="nucleotides">List of GameObjects being removed from nucleotide list.</param>
    public void RemoveFromTail(List<GameObject> nucleotides)
    {
        _nucleotides.RemoveAll(nucleotide => nucleotides.Contains(nucleotide));
        nucleotides.ForEach(n => _nucleotidesOnly.Remove(n.GetComponent<NucleotideComponent>()));
        if (_nucleotides.Count > 0)
        {
            _tail = _nucleotides.Last();
        }
        ResetComponents(nucleotides);
        ResetDomains();
        _xoversWasChanged = true;
        _sequenceWasChanged = true;
        _lengthWasChanged = true;
    }

    /// <summary>
    /// Removes strand and resets all GameObjects. This is used when merging strands together.
    /// </summary>
    public void RemoveStrand()
    {
        GameObject.Destroy(_cone);
        DeleteDomains();
        s_strandDict.Remove(_strandId);
    }

    /// <summary>
    /// Completely deletes all strand objects and removes it from dictionary.
    /// </summary>
    public void DeleteStrand()
    {
        ResetComponents(_nucleotides);
        DeleteXovers();
        DeleteDomains();
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
        SetNucleotidesOnly();
        ResetComponents(splitList);
        // UpdateFirstDomain();
        ResetDomains();
        _xoversWasChanged = true;
        _sequenceWasChanged = true;
        _lengthWasChanged = true;
        return splitList;
    }

    public void UpdateFirstDomain()
    {
        DNAComponent dnaComponent = _nucleotides[0].GetComponent<DNAComponent>();
        DomainComponent domainComponent = dnaComponent.Domain;
        List<DNAComponent> domain = new List<DNAComponent>();
        for (int i = 0; i < _nucleotides.Count; i++)
        {
            DNAComponent dnaComp = _nucleotides[i].GetComponent<DNAComponent>();
            NucleotideComponent ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
            domain.Add(dnaComp);
            if (ntc != null && ntc.HasXover)
            {
                break;
            }
        }
        domainComponent.Nucleotides = domain;
        domainComponent.UpdateCapsuleCollider();
    }

    public void SplitCircularBefore(GameObject go)
    {
        //TODO: Add DomainCollider logic
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
        _xoversWasChanged = true;
        _sequenceWasChanged = true;
        _lengthWasChanged = true;
        SetNucleotidesOnly();
        ResetComponents(splitList);
        // UpdateLastDomain();
        ResetDomains();
        return splitList;
    }

    public void UpdateLastDomain()
    {
        DNAComponent dnaComponent = _nucleotides.Last().GetComponent<DNAComponent>();
        DomainComponent domainComponent = dnaComponent.Domain;
        List<DNAComponent> domain = new List<DNAComponent>();
        for (int i = _nucleotides.Count - 1; i >= 0; i--)
        {
            DNAComponent dnaComp = _nucleotides[i].GetComponent<DNAComponent>();
            NucleotideComponent ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
            domain.Add(dnaComp);
            if (ntc != null && ntc.HasXover)
            {
                break;
            }
        }
        domain.Reverse();
        domainComponent.Nucleotides = domain;
        domainComponent.UpdateCapsuleCollider();
    }

    public void SplitCircularAfter(GameObject go)
    {
        // TODO: Add DomainCollider logic
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
        if (xover == null)
        {
            return;
        }

        var xoverComp = xover.GetComponent<XoverComponent>();
        var prevNtc = xoverComp.PrevGO.GetComponent<NucleotideComponent>();
        var nextNtc = xoverComp.NextGO.GetComponent<NucleotideComponent>();
        prevNtc.Xover = null;
        nextNtc.Xover = null;

        _xovers.Remove(xover);
        GameObject.Destroy(xover);
    }

    private void DeleteDomains()
    {
        foreach (DomainComponent domainComponent in _domains)
        {
            GameObject.Destroy(domainComponent.gameObject);
        }
        _domains.Clear();
    }

    /// <summary>
    /// Sets the domains for this strand.
    /// </summary>
    public void SetDomains()
    {
        ResetDomains();
    }

    /// <summary>
    /// Resets the domains for this strand.
    /// </summary>
    private void ResetDomains()
    {
        DeleteDomains();

        bool xoverBefore = false;
        List<DNAComponent> domain = new List<DNAComponent>();
        for (int i = _nucleotides.Count - 1; i >= 0; i--)
        {
            DNAComponent dnaComp = _nucleotides[i].GetComponent<DNAComponent>();
            NucleotideComponent ntc = _nucleotides[i].GetComponent<NucleotideComponent>();

            domain.Add(dnaComp);

            if (ntc != null && ntc.HasXover)
            {
                if (!xoverBefore)
                {
                    DomainComponent nextDomianComponent = DrawPoint.MakeDomain(domain, this);
                    _domains.Add(nextDomianComponent);
                    domain.Clear();
                    // Since xover endpoint nucleotides are sequential, this creating more domains than needed.
                    xoverBefore = true;
                }
                else
                {
                    xoverBefore = false; 
                }
            }
        }

        // Always create a domain component for the last domain
        DomainComponent lastDomainComponent = DrawPoint.MakeDomain(domain, this);
        _domains.Add(lastDomainComponent);
    }

    /// <summary>
    /// Sets variables of each GameObject's component (strandId, color, etc).
    /// </summary>
    public void SetComponents()
    {
        for (int i = _nucleotides.Count - 1; i >= 0; i--)
        {
            DNAComponent dnaComp = _nucleotides[i].GetComponent<DNAComponent>();
            NucleotideComponent ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
            dnaComp.ResetComponent();
            dnaComp.Selected = true;
            dnaComp.StrandId = _strandId;
            dnaComp.Color = _color;

            if (ntc != null && ntc.HasXover)
            {
                XoverComponent xoverComp = ntc.Xover.GetComponent<XoverComponent>();
                xoverComp.Color = _color;
            }
        }

        ResetDomains();
        SetCone();
    }

    /// <summary>
    /// Sets nucleotide and cone colors whenever Strand is set or unset to scaffold.
    /// </summary>
    public void SetColors()
    {
        for (int i = _nucleotides.Count - 1; i >= 0; i--)
        {
            DNAComponent dnaComp = _nucleotides[i].GetComponent<DNAComponent>();
            dnaComp.Color = _color;
        }
        _cone.GetComponent<ConeComponent>().Color = _color;
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

        _sequence = sequence;

        int seqCount = 0;
        for (int i = _nucleotides.Count - 1; i >= 0; i--)
        {
            NucleotideComponent ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
            //SequenceComponent ntSequence = _nucleotides[i].GetComponent<SequenceComponent>();

            // Sets DNA sequence to nucleotides
            if (ntc != null)
            {
                if (ntc.IsDeletion)
                {
                    ntc.Sequence = "X";
                }
                else
                {
                    ntc.Sequence = sequence.Substring(seqCount, ntc.Insertion + 1); // TODO: Change ntc.Insertion to seqComp.SequenceLength??
                    seqCount += ntc.Insertion + 1;
                }

                if (ntc.HasXover)
                {
                    LoopoutComponent loopComp = ntc.Xover.GetComponent<LoopoutComponent>();
                    //SequenceComponent loopSequence = ntc.Xover.GetComponent<SequenceComponent>();

                    if (loopComp != null && loopComp.NextGO == ntc.gameObject)
                    {
                        loopComp.Sequence = sequence.Substring(seqCount, loopComp.SequenceLength);
                        seqCount += loopComp.SequenceLength;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets cone position and rotation (pointing left or right).
    /// </summary>
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
    }

    // Resets all GameObject components in the nucleotides list.
    public void ResetComponents(List<GameObject> nucleotides)
    {
        if (nucleotides == null) { return; }
        for (int i = 0; i < nucleotides.Count; i++)
        {
            DNAComponent dnaComp = nucleotides[i].GetComponent<DNAComponent>();
            dnaComp.ResetComponent();

            NucleotideComponent ntc = nucleotides[i].GetComponent<NucleotideComponent>();
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
            NucleotideComponent ntc = _nucleotides[i].GetComponent<NucleotideComponent>();
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
            DNAComponent dnaComp = _nucleotides[i].GetComponent<DNAComponent>();
            if (!dnaComp.GridId.Equals(gridId))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Sets _nucleotidesOnly with only the nucleotides of the strand.
    /// </summary>
    public void SetNucleotidesOnly()
    {
        _nucleotidesOnly.Clear();
        foreach (GameObject gameObject in _nucleotides)
        {
            DNAComponent dnaComponent = gameObject.GetComponent<DNAComponent>();
            if (!dnaComponent.IsBackbone)
            {
                _nucleotidesOnly.Add((NucleotideComponent) dnaComponent);
            }
        }
    }

    /// <summary>
    /// Creates cross over suggestion game objects if there is a close enough strand in the opposite direction.
    /// Closeness is defined by the period of when the nucleotides are close to each other. 
    /// </summary>
    private void CheckForXoverSuggestions()
    {
        List<NucleotideComponent> nucleotides = _nucleotidesOnly;

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
    
    /// <summary>
    /// Draws the simplified bezier curve representation of this strand.
    /// </summary>
    public void DrawBezier()
    {
        foreach (DomainComponent domain in _domains)
        {
            domain.HideNucleotides();
        }
    }

    /// <summary>
    /// Returns this strand from the simplified bezier curve representation to
    /// the default nucleotide view.
    /// </summary>
    public void DeleteBezier()
    {
        foreach (DomainComponent domain in _domains)
        {
            domain.ShowNucleotides();
        }
    }

    /// <summary>
    /// Provides a different strand color than the one inputted.
    /// </summary>
    /// <param name="color">Color that is different than the output.</param>
    /// <returns>Color that is different than the input.</returns>
    public static Color GetDifferentColor(Color color)
    {
        Color nextColor = Colors[s_numStrands % Colors.Length];
        if (nextColor.Equals(color))
        {
            return Colors[(s_numStrands + 1) % Colors.Length];
        }
        return nextColor;
    }

    /// <summary>
    /// Activates or deactivates all domain components of this strand based on input.
    /// </summary>
    public void SetDomainActivity(bool active)
    {
        foreach (DomainComponent domain in _domains)
        {
            domain.gameObject.SetActive(active);
        }
    }
}
