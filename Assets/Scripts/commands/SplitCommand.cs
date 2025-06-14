/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;

public class SplitCommand : ICommand
{
    private GameObject _go;
    private int _id;
    private int _helixId;
    private int _direction;
    private int _strandId;
    private Color _color;

    public SplitCommand(NucleotideData nd)
    {
        _id = nd.Id;
        _helixId = nd.HelixId;
        _direction = nd.Direction;
        _strandId = s_numStrands;
        _color = Colors[s_numStrands % Colors.Length];
    }

    public void Do()
    {
        NucleotideData nd = Utils.FindNucleotideData(_id, _helixId, _direction);
        DrawSplit.SplitStrand(nd, _strandId, _color);
    }

    public void Undo()
    {
        NucleotideData nd = Utils.FindNucleotideData(_id, _helixId, _direction);
        DrawMerge.MergeStrand(nd);
    }

    public void Redo()
    {
        NucleotideData nd = Utils.FindNucleotideData(_id, _helixId, _direction);
        DrawSplit.SplitStrand(nd, _strandId, _color);
    }
}
