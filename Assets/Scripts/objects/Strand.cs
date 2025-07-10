/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using static GlobalVariables;

/// <summary>
/// Strand object keeps track of an individual strand of nucleotides.
/// </summary>
public class Strand
{
    private readonly List<Domain> domains = new List<Domain>();
    public List<Domain> Domains { get => domains; }

    public Strand(Domain domain, int strandId, Color color, bool isOxview = false) : this(new List<Domain> { domain }, strandId, color, isScaffold: false, isOxview) { }

    public Strand(List<Domain> domains, int strandId, Color color, bool isScaffold, bool isOxview = false)
    {
        _strandId = strandId;
        _color = color;
        _isOxview = isOxview;
        _isScaffold = isScaffold;
        this.domains.AddRange(domains);
    }

    public Domain GetDomain(int domainIdx)
    {
        return domains[domainIdx];
    }

    public Domain GetHeadDomain()
    {
        return GetDomain(0);
    }

    public Domain GetTailDomain()
    {
        return GetDomain(domains.Count - 1);
    }

    public NucleotideData GetHead()
    {
        return GetHeadDomain().GetHeadData();
    }

    public NucleotideData GetTail()
    {
        return GetTailDomain().GetTailData();
    }

    // This strand's id.
    private int _strandId;
    public int Id { get { return _strandId; } }

    //public int HelixId { get { return _head.GetComponent<NucleotideComponent>().HelixId; } }

    // This strand's color.
    private Color _color;
    public Color Color { get { return _color; } }

    public List<(int, int, NucleotideData)> GetInsertions()
    {
        List<(int, int, NucleotideData)> insertions = new List<(int, int, NucleotideData)>();
        foreach (Domain domain in domains)
        {
            foreach (var insertion in domain.Insertions)
            {
                insertions.Add((insertion.Key, insertion.Value, domain.GetNucleotideData(insertion.Key)));
            }
        }
        return insertions;
    }

    public List<(int, NucleotideData)> GetDeletions()
    {
        List<(int, NucleotideData)> deletions = new List<(int, NucleotideData)>();
        foreach (Domain domain in domains)
        {
            foreach (var deletion in domain.Deletions)
            {
                deletions.Add((deletion, domain.GetNucleotideData(deletion)));
            }
        }
        return deletions;
    }

    /// <summary>
    /// Whether or not _sequence has potentially changed since last Sequence call.
    /// </summary>
    private bool _sequenceWasChanged = true;
    private String _sequence;
    public string Sequence 
    { 
        get
        {
            StringBuilder sb = new StringBuilder();
            foreach (Domain domain in domains)
            {
                sb.Append(domain.GetSequence());
            }
            return sb.ToString();
        }
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
            int length = 0;
            foreach (Domain domain in domains)
            {
                length += domain.GetLength();
            }
            return length;
        }
    }

    // Whether or not strand is circular
    private bool _isCircular = false;
    public bool IsCircular { get { return _isCircular; } set { _isCircular = value; } }

    // Whether or not strand is from .oxview import
    private bool _isOxview;

    public void AddToHead(Domain domain)
    {
        domains.Insert(0, domain);
    }

    public void AddToHead(List<Domain> domains)
    {
        this.domains.InsertRange(0, domains);
    }

    public void AddToTail(Domain domain)
    {
        domains.Add(domain);
    }

    public void AddToTail(List<Domain> domains)
    {
        this.domains.AddRange(domains);
    }

    /// <summary>
    /// Reset strand domains.
    /// </summary>
    public void DeleteStrand()
    {
        foreach (Domain domain in domains)
        {
            domain.Reset(domain.StartId, domain.EndId);
        }
    }

    public List<Domain> Split(NucleotideData nd, bool splitAfter)
    {
        Domain domain = GetDomain(nd.DomainIdx);
        Domain newDomain;

        if (splitAfter)
        {
            newDomain = domain.SplitAfter(nd);
        }
        else
        {
            newDomain = domain.SplitBefore(nd);
        }
        
        List<Domain> newStrandDomains = new List<Domain>
        {
            newDomain
        };
        newStrandDomains.AddRange(domains.GetRange(nd.DomainIdx + 1, domains.Count - (nd.DomainIdx + 1)));
        domains.RemoveRange(nd.DomainIdx + 1, domains.Count - (nd.DomainIdx + 1));
        return newStrandDomains;
    }

    public List<Domain> Split(NucleotideData nd)
    {
        List<Domain> newStrandDomains = new List<Domain>();
        newStrandDomains.AddRange(domains.GetRange(nd.DomainIdx + 1, domains.Count - (nd.DomainIdx + 1)));
        domains.RemoveRange(nd.DomainIdx + 1, domains.Count - (nd.DomainIdx + 1));
        return newStrandDomains;
    }

    public void SetDomainsRevamp()
    {
        for (int i = 0; i < domains.Count; i++)
        {
            Domain domain = domains[i];
            domain.SetDomain(i, _strandId, _color);
        }
    }

    /// <summary>
    /// Sets nucleotide and cone colors whenever Strand is set or unset to scaffold.
    /// </summary>
    public void SetColors()
    {
        foreach (Domain domain in domains)
        {
            domain.SetColor(_color);
        }
    }


    public void SetSequenceRevamp(string sequence)
    {
        if (sequence.Equals("")) return;

        int strandLength = GetLength();
        if (sequence.Length < strandLength)
        {
            for (int i = 0; i < strandLength - sequence.Length; i++)
            {
                sequence += "?";
            }
        }
        
        _sequence = sequence;

        int seqCount = 0;
        for (int i = 0; i < domains.Count; i++)
        {
            Domain domain = domains[i];
            int domainLength = domain.GetLength();
            //Debug.Log(string.Format("Strand {0} domain {1} length: {2}", _strandId, i, domainLength));
            domain.SetSequence(sequence.Substring(seqCount, domainLength));
            seqCount += domainLength;

            if (domain.NextXover != null && domain.NextXover.IsLoopout)
            {
                LoopoutComponent loopout = (LoopoutComponent) domain.NextXover;
                loopout.Sequence = sequence.Substring(seqCount, loopout.SequenceLength);
                //Debug.Log("Loopout length: " + loopout.SequenceLength);
                //Debug.Log(string.Format("SeqCount: {0}, Sequence: {1}", seqCount, loopout.Sequence));
                seqCount += loopout.SequenceLength;
            }
        }
    }

    public int GetLength()
    {
        int length = 0;
        foreach (Domain domain in domains)
        {
            length += domain.GetLength();
            if (domain.NextXover != null && domain.NextXover.IsLoopout)
            {
                LoopoutComponent loopout = (LoopoutComponent) domain.NextXover;
                length += loopout.SequenceLength;
            }
        }
        return length;
    }

    public bool MoreThanOneGrid()
    {
        string gridId = GetHeadDomain().GetGridId();
        foreach (Domain domain in domains)
        {
            if (!domain.GetGridId().Equals(gridId))
            {
                return true;
            }
        }
        return false;
    }

    public Dictionary<int, int> GetLoopouts()
    {
        Dictionary<int, int> loopouts = new Dictionary<int, int>();
        foreach (Domain domain in domains)
        {
            if (domain.NextXover != null && domain.NextXover.IsLoopout)
            {
                LoopoutComponent loopout = (LoopoutComponent)domain.NextXover;
                loopouts.Add(domain.Id, loopout.SequenceLength);
            }
        }
        return loopouts;
    }
}
