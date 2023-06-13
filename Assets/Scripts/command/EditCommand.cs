/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using System.Collections.Generic;
using UnityEngine;

public class EditCommand : MonoBehaviour, ICommand
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
        DrawNucleotide.EditStrand(_nucleotides);
    }

    public void Undo()
    {
       
        DrawNucleotide.EraseStrand(_startGO, _nucleotides);
    }

    public void Redo()
    {
        DrawNucleotide.EditStrand(_nucleotides);
    }
}
