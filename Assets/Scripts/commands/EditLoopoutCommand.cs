using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class EditLoopoutCommand : ICommand
{
    private GameObject _go;
    private int _id;
    private int _helixId;
    private int _direction;
    private int _length;
    private int _oldLength;

    public EditLoopoutCommand(GameObject go, int newLength)
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
