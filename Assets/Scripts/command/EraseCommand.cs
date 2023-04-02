using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraseCommand : MonoBehaviour, ICommand
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
