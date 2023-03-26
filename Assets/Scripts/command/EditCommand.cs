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

    public void Undo()
    {
        DrawNucleotide.EraseStrand(_nucleotides);
    }

    public void Redo()
    {
        DrawNucleotide.EditStrand(_nucleotides);
    }
}
