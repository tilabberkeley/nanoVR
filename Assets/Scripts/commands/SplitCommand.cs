/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
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

    public SplitCommand(GameObject go, int strandId, Color color)
    {
        _id = go.GetComponent<NucleotideComponent>().Id;
        _helixId = go.GetComponent<NucleotideComponent>().HelixId;
        _direction = go.GetComponent<NucleotideComponent>().Direction;
        _go = go;
        _strandId = strandId;
        _color = color;
    }

    public void Do()
    {
        var ntc = _go.GetComponent<NucleotideComponent>();
        DrawSplit.SplitStrand(_go, _strandId, _color, Convert.ToBoolean(ntc.Direction));
    }
    public void Undo()
    {
        GameObject go = FindNucleotide(_id, _helixId, _direction);
        DrawMerge.MergeStrand(go);
    }

    public void Redo()
    {
        GameObject go = FindNucleotide(_id, _helixId, _direction);
        var ntc = go.GetComponent<NucleotideComponent>();
        DrawSplit.SplitStrand(_go, _strandId, _color, Convert.ToBoolean(ntc.Direction));
    }

    public GameObject FindNucleotide(int id, int helixId, int direction)
    {
        s_helixDict.TryGetValue(helixId, out Helix helix);
        return helix.GetNucleotide(id, direction);
    }
}
