/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static Utils;

/// <summary>
/// Command to undo/redo nucleotide sequence edits.
/// </summary>
public class EditNucleotideCommand : ICommand
{
    private int _id;
    private int _helixId;
    private int _direction;
    private string _prevSequence;
    private string _newSequence;
    private bool _changedComplement;

    public EditNucleotideCommand(NucleotideData nd, string newSequence, bool changedComplement)
    {
        _id = nd.Id;
        _helixId = nd.HelixId;
        _direction = nd.Direction;
        _prevSequence = nd.Sequence;
        _newSequence = newSequence;
        _changedComplement = changedComplement;
    }

    public void Do()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        NucleotideEdit.SetNucleotide(nd, _newSequence, _changedComplement);
    }

    public void Redo()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        NucleotideEdit.SetNucleotide(nd, _newSequence, _changedComplement);
    }

    public void Undo()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        NucleotideEdit.SetNucleotide(nd, _prevSequence, _changedComplement);
    }
}
