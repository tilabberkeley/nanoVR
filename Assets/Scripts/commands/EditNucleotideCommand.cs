/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

/// <summary>
/// Command to undo/redo nucleotide sequence edits.
/// </summary>
public class EditNucleotideCommand : ICommand
{
    private NucleotideComponent _ntc;
    private int _id;
    private int _helixId;
    private int _direction;
    private string _prevSequence;
    private string _newSequence;
    private bool _changedComplement;

    // TODO: Implement this
    public EditNucleotideCommand(NucleotideComponent ntc, string newSequence, bool changedComplement)
    {
        _ntc = ntc;
        _id = ntc.Id;
        _helixId = ntc.HelixId;
        _direction = ntc.Direction;
        _prevSequence = ntc.Sequence;
        _newSequence = newSequence;
        _changedComplement = changedComplement;
    }

    public void Do()
    {
        NucleotideEdit.SetNucleotide(_ntc, _newSequence, _changedComplement);
    }

    public void Redo()
    {
        GameObject nucleotide = Utils.FindNucleotide(_id, _helixId, _direction);
        _ntc = nucleotide.GetComponent<NucleotideComponent>();
        NucleotideEdit.SetNucleotide(_ntc, _newSequence, _changedComplement);
    }

    public void Undo()
    {
        GameObject nucleotide = Utils.FindNucleotide(_id, _helixId, _direction);
        _ntc = nucleotide.GetComponent<NucleotideComponent>();
        NucleotideEdit.SetNucleotide(_ntc, _prevSequence, _changedComplement);
    }
}
