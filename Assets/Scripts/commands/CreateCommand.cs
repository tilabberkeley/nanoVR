/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */

using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;
using static Utils;

public class CreateCommand : ICommand
{
    private int _strandId;
    private List<GameObject> _nucleotides;
    private List<GameObject> _xovers;
    private List<(int, int, int, bool)> _nucleotideIds = new List<(int, int, int, bool)>();
    private List<(int, int, int, bool)> _prevGOs = new List<(int, int, int, bool)>();
    private List<(int, int, int, bool)> _nextGOs = new List<(int, int, int, bool)>();
    private List<(int, int, int)> _deletions = new List<(int, int, int)>();
    private List<(int, int, int, int)> _insertions = new List<(int, int, int, int)>();
    private Color _color;
    private string _sequence;
    private bool _isScaffold;

    public CreateCommand(List<GameObject> nucleotides, int strandId)
    {
        _strandId = strandId;
        _nucleotides = nucleotides;

        for (int i = 0; i < nucleotides.Count; i += 1)
        {
            var dnaComp = nucleotides[i].GetComponent<DNAComponent>();

            int id = dnaComp.Id;
            int helixId = dnaComp.HelixId;
            int direction = dnaComp.Direction;
            bool isBackbone = dnaComp.IsBackbone;
            _nucleotideIds.Add((id, helixId, direction, isBackbone));

            var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            if (ntc != null)
            {
                if (ntc.IsDeletion)
                {
                    _deletions.Add((id, helixId, direction));
                }
                if (ntc.IsInsertion)
                {
                    _insertions.Add((id, helixId, direction, ntc.Insertion));
                }
            }
            if (ntc != null && ntc.HasXover)
            {
                var xoverComp = ntc.Xover.GetComponent<XoverComponent>();
                if (xoverComp.PrevGO == nucleotides[i])
                {
                    _prevGOs.Add((id, helixId, direction, isBackbone));
                } 
                else if (xoverComp.NextGO == nucleotides[i])
                {
                    _nextGOs.Add((id, helixId, direction, isBackbone));
                }
            }

        }

        /*for (int i = 0; i < xovers.Count; i++)
        {
            var ntc = xovers[i].GetComponent<XoverComponent>().PrevGO.GetComponent<DNAComponent>();
            int id = ntc.Id;
            int helixId = ntc.HelixId;
            int direction = ntc.Direction;
            bool isBackbone = ntc.IsBackbone;
            _prevGOs.Add((id, helixId, direction, isBackbone));

            ntc = xovers[i].GetComponent<XoverComponent>().NextGO.GetComponent<DNAComponent>();
            id = ntc.Id;
            helixId = ntc.HelixId;
            direction = ntc.Direction;
            isBackbone = ntc.IsBackbone;
            _nextGOs.Add((id, helixId, direction, isBackbone));
        }*/
    }

    public void Do()
    {
        CreateStrand(_nucleotides, _strandId);
        _color = s_strandDict[_strandId].Color;
    }

    public void Undo()
    {
        // Delete entire strand.
        GameObject start = FindGameObject(_nucleotideIds[0].Item1, _nucleotideIds[0].Item2, _nucleotideIds[0].Item3, _nucleotideIds[0].Item4);
        _sequence = s_strandDict[start.GetComponent<NucleotideComponent>().StrandId].Sequence;
        _isScaffold = s_strandDict[start.GetComponent<NucleotideComponent>().StrandId].IsScaffold;
        SelectStrand.DeleteStrand(start);
    }

    public void Redo()
    {
        List<GameObject> nucleotides = new List<GameObject>();

        for (int i = 0; i < _nucleotideIds.Count; i++)
        {
            GameObject nucl = FindGameObject(_nucleotideIds[i].Item1, _nucleotideIds[i].Item2, _nucleotideIds[i].Item3, _nucleotideIds[i].Item4);
            nucleotides.Add(nucl);
        }

        // Loop through the stored prevGOs and create new xovers using the prevGO and nextGO list. Then, set the prevGO and nextGO
        // of the new xover, and add it to the xover list. Use this list to create the new strand.
        for (int i = 0; i < _prevGOs.Count; i++)
        {
            GameObject prevGO = FindGameObject(_prevGOs[i].Item1, _prevGOs[i].Item2, _prevGOs[i].Item3, _prevGOs[i].Item4);
            GameObject nextGO = FindGameObject(_nextGOs[i].Item1, _nextGOs[i].Item2, _nextGOs[i].Item3, _nextGOs[i].Item4);

            // TODO: FIND BETTER ALTNERATIVE TO -1 FOR PREV ID
            DrawPoint.MakeXover(prevGO, nextGO, _strandId, -1);
        }
        List<(GameObject, int)> insertions = new List<(GameObject, int)>();
        List<GameObject> deletions = new List<GameObject>();
        for (int i = 0; i < _insertions.Count; i++)
        {
            GameObject nt = FindNucleotide(_insertions[i].Item1, _insertions[i].Item2, _insertions[i].Item3);
            insertions.Add((nt, _insertions[i].Item4));
        }

        for (int i = 0; i < _deletions.Count; i++)
        {
            GameObject nt = FindNucleotide(_deletions[i].Item1, _deletions[i].Item2, _deletions[i].Item3);
            deletions.Add(nt);
        }
      
        CreateStrand(nucleotides, _strandId, _color, insertions, deletions, _sequence, _isScaffold);
    }

    public GameObject FindGameObject(int id, int helixId, int direction, bool isBackbone)
    {
        s_helixDict.TryGetValue(helixId, out Helix helix);
        if (isBackbone)
        {
            return helix.GetBackbone(id, direction);
        }
        return helix.GetNucleotide(id, direction);
    }
}
