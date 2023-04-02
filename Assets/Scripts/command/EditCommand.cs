/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using System.Collections.Generic;
using UnityEngine;

public class EditCommand : MonoBehaviour, ICommand
{
    private List<GameObject> _nucleotides;

    public EditCommand(List<GameObject> nucleotides)
    {
        _nucleotides = nucleotides;
    }

    public void Do()
    {
        DrawNucleotide.EditStrand(_nucleotides);
    }

    public void Undo()
    {
        List<GameObject> reversedNucls = new List<GameObject>(_nucleotides);
        reversedNucls.Reverse();
        DrawNucleotide.EraseStrand(reversedNucls);
    }

    public void Redo()
    {
        DrawNucleotide.EditStrand(_nucleotides);
    }
}
