/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

public class CreateCommand : ICommand
{
    private List<GameObject> _nucleotides;
    private int _strandId;
    private Color _color;
    public CreateCommand(List<GameObject> nucleotides, int strandId)
    {
        _nucleotides = nucleotides;
        _strandId = strandId;
    }

    public void Do()
    {
        DrawNucleotideDynamic.CreateStrand(_nucleotides, _strandId);
        _color = s_strandDict[_strandId].GetColor();
    }

    public void Undo()
    {
        // Delete entire strand.
        SelectStrand.DeleteStrand(_nucleotides[0]);
    }

    public void Redo()
    {
        DrawCrossover.CreateStrand(_nucleotides, _strandId, _color);
    }
}
