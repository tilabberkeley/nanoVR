/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

public class EditCommand : ICommand
{
    private int _startId;
    private int _endId;
    private int _helixId;
    private int _direction;

    public EditCommand(NucleotideData start, NucleotideData end)
    {
        _startId = start.Id;
        _endId = end.Id;
        _helixId = start.HelixId;
        _direction = start.Direction;
    }

    public void Do()
    {
        NucleotideData nd1 = Utils.FindNucleotideData(_startId, _helixId, _direction);
        NucleotideData nd2 = Utils.FindNucleotideData(_endId, _helixId, _direction);
        DrawNucleotideDynamic.EditStrand(nd1, nd2);
    }

    public void Undo()
    {
        NucleotideData nd1 = Utils.FindNucleotideData(_startId, _helixId, _direction);
        NucleotideData nd2 = Utils.FindNucleotideData(_endId, _helixId, _direction);
        DrawNucleotideDynamic.EraseStrand(nd2, nd1);
    }

    public void Redo()
    {
        NucleotideData nd1 = Utils.FindNucleotideData(_startId, _helixId, _direction);
        NucleotideData nd2 = Utils.FindNucleotideData(_endId, _helixId, _direction);
        DrawNucleotideDynamic.EditStrand(nd1, nd2);
    }
}