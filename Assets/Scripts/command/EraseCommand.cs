/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public class EraseCommand : MonoBehaviour, ICommand
{
    private GameObject _startGO;
    private List<GameObject> _nucleotides;

    public EraseCommand(GameObject startGO, List<GameObject> nucleotides)
    {
        _startGO = startGO;
        _nucleotides = nucleotides;
    }

    public void Do()
    {
        DrawNucleotide.EraseStrand(_startGO, _nucleotides);
    }

    public void Undo()
    {
        DrawNucleotide.EditStrand(_nucleotides);
    }

    public void Redo()
    {
        DrawNucleotide.EraseStrand(_startGO, _nucleotides);
    }
}
