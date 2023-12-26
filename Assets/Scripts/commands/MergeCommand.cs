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
    private int _strandId;
    private Color _color;
    private bool _splitAfter;

    public MergeCommand(GameObject go, int strandId, Color color, bool splitAfter)
    {
        _go = go;
        _id = go.GetComponent<NucleotideComponent>().Id;
        _helixId = go.GetComponent<NucleotideComponent>().HelixId;
        _direction = go.GetComponent<NucleotideComponent>().Direction;
        _strandId = strandId;
        _color = color;
        _splitAfter = splitAfter;
    }

    public void Do()
    {
        DrawMerge.MergeStrand(_go);
    }

    public void Undo()
    {
        GameObject go = FindNucleotide(_id, _helixId, _direction);
        DrawSplit.SplitStrand(go, _strandId, _color, _splitAfter);
    }

    public void Redo()
    {
        GameObject go = FindNucleotide(_id, _helixId, _direction);
        DrawMerge.MergeStrand(go);
    }

    public GameObject FindNucleotide(int id, int helixId, int direction)
    {
        s_helixDict.TryGetValue(helixId, out Helix helix);
        return helix.GetNucleotide(id, direction);
    }
}
