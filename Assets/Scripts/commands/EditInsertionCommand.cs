/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static Utils;

public class EditInsertionCommand : ICommand
{
    private GameObject _go;
    private int _id;
    private int _helixId;
    private int _direction;
    private int _length;
    private int _oldLength;

    public EditInsertionCommand(GameObject go, int newLength)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        _go = go;
        _length = newLength;
        _id = ntc.Id;
        _helixId = ntc.HelixId;
        _direction = ntc.Direction;
        _oldLength = ntc.Insertion;
    }

    public void Do()
    {
        DrawInsertion.EditInsertion(_go, _length);
    }

    public void Redo()
    {
        GameObject go = FindNucleotide(_id, _helixId, _direction);
        DrawInsertion.EditInsertion(go, _oldLength);
        _oldLength = _length;
        _length = go.GetComponent<NucleotideComponent>().Insertion;
    }

    public void Undo()
    {
        GameObject go = FindNucleotide(_id, _helixId, _direction);
        DrawInsertion.EditInsertion(go, _oldLength);
        _oldLength = _length;
        _length = go.GetComponent<NucleotideComponent>().Insertion;
    }
}
