/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using static Utils;

public class DeletionCommand : ICommand
{
    private int _id;
    private int _helixId;
    private int _direction;

    public DeletionCommand(NucleotideData nd)
    {
        _id = nd.Id;
        _helixId = nd.HelixId;
        _direction = nd.Direction;
    }

    public void Do()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        DrawDeletion.Deletion(nd);
    }
    public void Undo()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        DrawDeletion.Deletion(nd);
    }

    public void Redo()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        DrawDeletion.Deletion(nd);
    }
}
