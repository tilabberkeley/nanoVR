/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static Utils;

public class InsertionCommand : ICommand
{
    private GameObject _go;
    private int _id;
    private int _helixId;
    private int _direction;
    private int _length;

    public InsertionCommand(GameObject go, int length)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        _go = go;
        _length = length;
        _id = ntc.Id;
        _helixId = ntc.HelixId;
        _direction = ntc.Direction;
    }

    public void Do()
    {
        DrawInsertion.Insertion(_go, _length);
    }
    public void Undo()
    {
        GameObject go = FindNucleotide(_id, _helixId, _direction);
        DrawInsertion.Insertion(go, _length);
    }

    public void Redo()
    {
        GameObject go = FindNucleotide(_id, _helixId, _direction);
        DrawInsertion.Insertion(go, _length);
    }
}
