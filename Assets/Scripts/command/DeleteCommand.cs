/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

public class DeleteCommand : ICommand
{
    private int _strandId;
    private List<GameObject> _nucleotides;
    private List<(int, int, int, bool)> _nucleotideIds = new List<(int, int, int, bool)>();
    private List<(int, int, int, bool)> _prevGOs = new List<(int, int, int, bool)>();
    private List<(int, int, int, bool)> _nextGOs = new List<(int, int, int, bool)>();
    //private List<int> _helixIds = new List<int>();
    private Color _color;

    public DeleteCommand(int strandId, List<GameObject> nucleotides, List<GameObject> xovers, Color color)
    {
        _strandId = strandId;
        _nucleotides = nucleotides;
        //_helixIds = helixIds;
        for (int i = 0; i < nucleotides.Count; i += 1)
        {
            var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            
            int id = ntc.Id;
            int helixId = ntc.HelixId;
            int direction = ntc.Direction;
            bool isBackbone = ntc.IsBackbone;
            _nucleotideIds.Add((id, helixId, direction, isBackbone));
           
        }
        for (int i = 0; i < xovers.Count; i++)
        {
            var ntc = xovers[i].GetComponent<XoverComponent>().GetPrevGO().GetComponent<NucleotideComponent>();
            int id = ntc.Id;
            int helixId = ntc.HelixId;
            int direction = ntc.Direction;
            bool isBackbone = ntc.IsBackbone;
            _prevGOs.Add((id, helixId, direction, isBackbone));


            ntc = xovers[i].GetComponent<XoverComponent>().GetNextGO().GetComponent<NucleotideComponent>();
            id = ntc.Id;
            helixId = ntc.HelixId;
            direction = ntc.Direction;
            isBackbone = ntc.IsBackbone;
            _nextGOs.Add((id, helixId, direction, isBackbone));
        }
        _color = color;
    }
    public void Do()
    {
        SelectStrand.DeleteStrand(_nucleotides[0]);
    }

    public void Redo()
    {
        GameObject start = FindGameObject(_nucleotideIds[0].Item1, _nucleotideIds[0].Item2, _nucleotideIds[0].Item3, _nucleotideIds[0].Item4);
        SelectStrand.DeleteStrand(start);
    }

    public void Undo()
    {
        List<GameObject> xovers = new List<GameObject>();
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

            GameObject xover = DrawPoint.MakeXover(prevGO, nextGO, _strandId);
            xover.GetComponent<XoverComponent>().SetPrevGO(prevGO);
            xover.GetComponent<XoverComponent>().SetNextGO(nextGO);
            xovers.Add(xover);
            // Debug.Log("Drawing xover! " + xover);
        }
        DrawCrossover.CreateStrand(nucleotides, xovers, _strandId, _color);
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
