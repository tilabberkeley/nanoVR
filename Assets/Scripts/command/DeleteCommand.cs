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
    private List<(int, int, int)> _ids = new List<(int, int, int)>();
    private List<GameObject> _prevGOs = new List<GameObject>();
    private List<GameObject> _nextGOs = new List<GameObject>();
    private Color _color;
    public DeleteCommand(int strandId, List<GameObject> nucleotides, List<GameObject> xovers, Color color)
    {
        _strandId = strandId;
        _nucleotides = nucleotides;
        for (int i = 0; i < nucleotides.Count; i += 1)
        {
            var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            int id = ntc.Id;
            int helixId = ntc.HelixId;
            int direction = ntc.Direction;
            _ids.Add((id, helixId, direction));
        }
        for (int i = 0; i < xovers.Count; i++)
        {
            _prevGOs.Add(xovers[i].GetComponent<XoverComponent>().GetPrevGO());
            _nextGOs.Add(xovers[i].GetComponent<XoverComponent>().GetNextGO());
        }
        _color = color;
    }
    public void Do()
    {
        SelectStrand.DeleteStrand(_nucleotides[0]);
    }

    public void Redo()
    {
        GameObject start = FindNucleotide(_ids[0].Item1, _ids[0].Item2, _ids[0].Item3);
        SelectStrand.DeleteStrand(start);
    }

    public void Undo()
    {
        List<GameObject> xovers = new List<GameObject>();
        List<GameObject> nucleotides = new List<GameObject>();

        for (int i = 0; i < _ids.Count; i++)
        {
            GameObject nucl = FindNucleotide(_ids[i].Item1, _ids[i].Item2, _ids[i].Item3);
            nucleotides.Add(nucl);
        }

        // Loop through the stored prevGOs and create new xovers using the prevGO and nextGO list. Then, set the prevGO and nextGO
        // of the new xover, and add it to the xover list. Use this list to create the new strand.
        for (int i = 0; i < _prevGOs.Count; i++)
        {
            GameObject xover = DrawPoint.MakeXover(_prevGOs[i], _nextGOs[i], _strandId);
            xover.GetComponent<XoverComponent>().SetPrevGO(_prevGOs[i]);
            xover.GetComponent<XoverComponent>().SetNextGO(_nextGOs[i]);
            xovers.Add(xover);
            Debug.Log("Drawing xover! " + xover);

        }
        DrawCrossover.CreateStrand(nucleotides, xovers, _strandId, _color);
    }

    public GameObject FindNucleotide(int id, int helixId, int direction)
    {
        s_helixDict.TryGetValue(helixId, out Helix helix);
        return helix.GetNucleotide(id, direction);
    }
}
