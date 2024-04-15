using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class EditLoopoutCommand : ICommand
{
    GameObject _loopout;

    private NucleotideComponent _first;
    private int _firstId;
    private int _firstHelixId;
    private int _firstDirection;

    private NucleotideComponent _second;
    private int _secondId;
    private int _secondHelixId;
    private int _secondDirection;

    private int _newLength;
    private int _oldLength;

    public EditLoopoutCommand(GameObject loopout, int newLength)
    {
        _loopout = loopout;
        LoopoutComponent loopoutComponent = loopout.GetComponent<LoopoutComponent>();

        _first = loopoutComponent.NextGO.GetComponent<NucleotideComponent>();
        _firstId = _first.Id;
        _firstHelixId = _first.HelixId;
        _firstDirection = _first.Direction;

        _second = loopoutComponent.PrevGO.GetComponent<NucleotideComponent>();
        _secondId = _second.Id;
        _secondHelixId = _second.HelixId;
        _secondDirection = _second.Direction;

        _newLength = newLength;
        _oldLength = loopoutComponent.SequenceLength;
    }

    public void Do()
    {
        DrawLoopout.EditLoopout(_loopout, _newLength);
    }

    public void Redo()
    {
        GameObject startGO = FindNucleotide(_firstId, _firstHelixId, _firstDirection);

        GameObject loopout = startGO.GetComponent<NucleotideComponent>().Xover;

        DrawLoopout.EditLoopout(loopout, _oldLength);
    }

    public void Undo()
    {
        GameObject startGO = FindNucleotide(_firstId, _firstHelixId, _firstDirection);

        GameObject loopout = startGO.GetComponent<NucleotideComponent>().Xover;

        DrawLoopout.EditLoopout(loopout, _newLength);
    }
}
