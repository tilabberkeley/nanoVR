/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public class EraseCommand : ICommand
{
    private List<GameObject> _nucleotides;

    public EraseCommand(List<GameObject> nucleotides)
    {
        _nucleotides = nucleotides;
    }

    public void Do()
    {
        DrawNucleotide.EraseStrand(_nucleotides);
    }

    public void Undo()
    {
        DrawNucleotide.EditStrand(_nucleotides);
    }

    public void Redo()
    {
        DrawNucleotide.EraseStrand(_nucleotides);
    }
}
