/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static Utils;

public class MergeCommand : ICommand
{
    private GameObject _go;
    private int _id;
    private int _helixId;
    private int _direction;
    private int _neighborStrandId;
    private Color _neighborColor;

    public MergeCommand(NucleotideData nd, NucleotideData neighbor)
    {
        _id = nd.Id;
        _helixId = nd.HelixId;
        _direction = nd.Direction;
        _neighborStrandId = neighbor.StrandId;
        _neighborColor = neighbor.Color;
    }

    public void Do()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        DrawMerge.MergeStrand(nd);
    }

    public void Undo()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        DrawSplit.SplitStrand(nd, _neighborStrandId, _neighborColor);
    }

    public void Redo()
    {
        NucleotideData nd = FindNucleotideData(_id, _helixId, _direction);
        DrawMerge.MergeStrand(nd);
    }
}
