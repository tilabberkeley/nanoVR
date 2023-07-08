using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteCommand : ICommand
{
    private int _strandId;
    private List<GameObject> _nucleotides;
    private List<GameObject> _prevGOs = new List<GameObject>();
    private List<GameObject> _nextGOs = new List<GameObject>();
    private Color _color;
    public DeleteCommand(int strandId, List<GameObject> nucleotides, List<GameObject> xovers, Color color)
    {
        _strandId = strandId;
        _nucleotides = nucleotides;
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
        SelectStrand.DeleteStrand(_nucleotides[0]);
    }

    public void Undo()
    {
        List<GameObject> xovers = new List<GameObject>();

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
        DrawCrossover.CreateStrand(_nucleotides, xovers, _strandId, _color);
    }
}
