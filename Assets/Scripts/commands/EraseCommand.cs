/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */

using System.Collections.Generic;
using static Utils;

public class EraseCommand : ICommand
{
    private int _startId;
    private int _endId;
    private int _helixId;
    private int _direction;

    private string _domainSequence;
    private Dictionary<int, int> _insertions;
    private List<int> _deletions;

    public EraseCommand(NucleotideData start, NucleotideData end)
    {
        _startId = start.Id;
        _endId = end.Id;
        _helixId = start.HelixId;
        _direction = start.Direction;

        Domain domain = start.GetDomain();
        _domainSequence = domain.GetSequence();
        _insertions = new Dictionary<int, int>(domain.Insertions);
        _deletions = new List<int>(domain.Deletions);
    }

    public void Do()
    {
        NucleotideData nd1 = FindNucleotideData(_startId, _helixId, _direction);
        NucleotideData nd2 = FindNucleotideData(_endId, _helixId, _direction);
        DrawNucleotideDynamic.EraseStrand(nd1, nd2);
    }

    public void Undo()
    {
        NucleotideData nd1 = FindNucleotideData(_startId, _helixId, _direction);
        NucleotideData nd2 = FindNucleotideData(_endId, _helixId, _direction);
        DrawNucleotideDynamic.EditStrand(nd2, nd1);

        // Add back domain sequence, insertions, and deletions.
        Domain domain = nd1.GetDomain();
        domain.SetSequence(_domainSequence);
        foreach(var insertion in _insertions)
        {
            NucleotideData nd = FindNucleotideData(insertion.Key, _helixId, _direction);
            DrawInsertion.Insertion(nd, insertion.Value);
        }
        foreach(int deletion in _deletions)
        {
            NucleotideData nd = FindNucleotideData(deletion, _helixId, _direction);
            DrawDeletion.Deletion(nd);
        }
    }

    public void Redo()
    {
        NucleotideData nd1 = FindNucleotideData(_startId, _helixId, _direction);
        NucleotideData nd2 = FindNucleotideData(_endId, _helixId, _direction);
        DrawNucleotideDynamic.EraseStrand(nd1, nd2);
    }
}