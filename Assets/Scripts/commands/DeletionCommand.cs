/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static Utils;

public class DeletionCommand : ICommand
{
    private GameObject _go;
    private int _id;
    private int _helixId;
    private int _direction;
    private int _length;

    public DeletionCommand(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        _go = go;
        _id = ntc.Id;
        _helixId = ntc.HelixId;
        _direction = ntc.Direction;
    }

    public void Do()
    {
        DrawDeletion.Deletion(_go);
    }
    public void Undo()
    {
        GameObject go = FindNucleotide(_id, _helixId, _direction);
        DrawDeletion.Deletion(go);
    }

    public void Redo()
    {
        GameObject go = FindNucleotide(_id, _helixId, _direction);
        DrawDeletion.Deletion(go);
    }
}
