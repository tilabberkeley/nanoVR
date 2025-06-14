/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;

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
        NucleotideData nd = Utils.FindNucleotideData(_id, _helixId, _direction);
        DrawMerge.MergeStrand(nd);
    }

    public void Undo()
    {
        NucleotideData nd = Utils.FindNucleotideData(_id, _helixId, _direction);
        DrawSplit.SplitStrand(nd, _neighborStrandId, _neighborColor);
    }

    public void Redo()
    {
        NucleotideData nd = Utils.FindNucleotideData(_id, _helixId, _direction);
        DrawMerge.MergeStrand(nd);
    }

    public GameObject FindNucleotide(int id, int helixId, int direction)
    {
        s_helixDict.TryGetValue(helixId, out Helix helix);
        return helix.GetNucleotide(id, direction);
    }
}
