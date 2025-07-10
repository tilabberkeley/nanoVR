/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using static Utils;

public class EditInsertionCommand : ICommand
{
    private int _id;
    private int _helixId;
    private int _direction;
    private int _length;
    private int _oldLength;

    public EditInsertionCommand(NucleotideData nd, int newLength)
    {
        _length = newLength;
        _id = nd.Id;
        _helixId = nd.HelixId;
        _direction = nd.Direction;
        _oldLength = nd.Insertion;
    }

    public void Do()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        EditOptionsManager.EditInsertion(nd, _length);
    }

    public void Redo()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        EditOptionsManager.EditInsertion(nd, _length);
    }

    public void Undo()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        EditOptionsManager.EditInsertion(nd, _oldLength);
    }
}
