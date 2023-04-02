/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

public class CreateCommand : MonoBehaviour, ICommand
{
    private List<GameObject> _nucleotides;
    private int _strandId;
    public CreateCommand(List<GameObject> nucleotides, int strandId)
    {
        _nucleotides = nucleotides;
        _strandId = strandId;
    }

    public void Do()
    {
        DrawNucleotide.CreateStrand(_nucleotides, _strandId);
    }

    public void Undo()
    {
        // Delete entire strand.
        return;
    }

    public void Redo()
    {
        DrawCrossover.CreateStrand(_nucleotides, _strandId, s_strandDict[_strandId].GetColor());
    }
}
