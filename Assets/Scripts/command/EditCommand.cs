/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using System.Collections.Generic;
using UnityEngine;

public class EditCommand : ICommand
{
    private GameObject _startGO;
    private List<GameObject> _nucleotides;

    public EditCommand(GameObject startGO, List<GameObject> nucleotides)
    {
        _startGO = startGO;
        _nucleotides = nucleotides;
    }

    public void Do()
    {
        DrawNucleotideDynamic.EditStrand(_nucleotides);
    }

    public void Undo()
    {
        DrawNucleotideDynamic.EraseStrand(_startGO, _nucleotides);
    }

    public void Redo()
    {
        DrawNucleotideDynamic.EditStrand(_nucleotides);
    }
}