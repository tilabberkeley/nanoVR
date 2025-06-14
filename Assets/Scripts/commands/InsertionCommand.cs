/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using static Utils;

public class InsertionCommand : ICommand
{
    private int _id;
    private int _helixId;
    private int _direction;
    private int _length;

    public InsertionCommand(NucleotideData nd, int length)
    {
        _length = length;
        _id = nd.Id;
        _helixId = nd.HelixId;
        _direction = nd.Direction;
    }

    public void Do()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        DrawInsertion.Insertion(nd, _length);
    }
    public void Undo()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        DrawInsertion.Insertion(nd, _length);
    }

    public void Redo()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        DrawInsertion.Insertion(nd, _length);
    }
}
