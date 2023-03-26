/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using System.Collections.Generic;
using UnityEngine;

public class CreateCommand : MonoBehaviour, ICommand
{
    private List<GameObject> _nucleotides;
    private int _strandId;
    public CreateCommand(List<GameObject> nucleotides, int strandId)
    {
        _nucleotides = nucleotides;
        _strandId = strandId;
    }

    public void Undo()
    {
        DrawNucleotide.EraseStrand(_nucleotides);
    }

    public void Redo()
    {
        DrawNucleotide.CreateStrand(_nucleotides, _strandId);
    }
}
