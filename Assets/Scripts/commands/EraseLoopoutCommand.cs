using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class EraseLoopoutCommand : ICommand
{
    private GameObject _loopout;
    private GameObject _startGO;
    private GameObject _endGO;
    private int _strandId;
    private Color _color;

    private int _startId;
    private int _startHelixId;
    private int _startDirection;
    private int _endId;
    private int _endHelixId;
    private int _endDirection;

    private int _sequenceLength;

    public EraseLoopoutCommand(GameObject loopout, int strandId, Color color)
    {
        _loopout = loopout;
        var xoverComp = loopout.GetComponent<XoverComponent>();
        _startGO = xoverComp.PrevGO;
        _endGO = xoverComp.NextGO;
        _strandId = strandId;
        _color = color;

        var startNtc = _startGO.GetComponent<NucleotideComponent>();
        _startId = startNtc.Id;
        _startHelixId = startNtc.HelixId;
        _startDirection = startNtc.Direction;

        var endNtc = _endGO.GetComponent<NucleotideComponent>();
        _endId = endNtc.Id;
        _endHelixId = endNtc.HelixId;
        _endDirection = endNtc.Direction;
    }

    public void Do()
    {
        DrawLoopout.EraseLoopout(_loopout, _strandId, _color, false);
    }

    public void Undo()
    {
        // _xover does not exist after it gets erased. must created new xover
        GameObject startGO = FindNucleotide(_startId, _startHelixId, _startDirection);
        GameObject endGO = FindNucleotide(_endId, _endHelixId, _endDirection);
        _loopout = DrawLoopout.CreateLoopout(startGO, endGO, _sequenceLength);
    }

    public void Redo()
    {
        DrawLoopout.EraseLoopout(_loopout, _strandId, _color, false);
    }
}
