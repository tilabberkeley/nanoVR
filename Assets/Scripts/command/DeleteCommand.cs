using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteCommand : ICommand
{
    private int _strandId;
    private List<GameObject> _nucleotides;
    private Color _color;
    public DeleteCommand(int strandId, List<GameObject> nucleotides, Color color)
    {
        _strandId = strandId;
        _nucleotides = nucleotides;
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
        DrawCrossover.CreateStrand(_nucleotides, _strandId, _color);
    }
}
