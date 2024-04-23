using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class LoopoutCommand : ICommand
{
    private GameObject _first;
    private GameObject _second;
    private GameObject _loopout;
    private bool _firstIsHead;
    private bool _firstIsEnd;
    private bool _secondIsEnd;
    private Color _prevColor;

    private int _startId;
    private int _startHelixId;
    private int _startDirection;
    private int _endId;
    private int _endHelixId;
    private int _endDirection;

    private int _sequenceLength;

    public LoopoutCommand(GameObject first, GameObject second, bool firstIsEnd, bool secondIsEnd, bool firstIsHead, int sequenceLength)
    {
        _first = first;
        _second = second;
        _prevColor = second.GetComponent<NucleotideComponent>().Color;

        var startNtc = first.GetComponent<NucleotideComponent>();
        _startId = startNtc.Id;
        _startHelixId = startNtc.HelixId;
        _startDirection = startNtc.Direction;

        var endNtc = second.GetComponent<NucleotideComponent>();
        _endId = endNtc.Id;
        _endHelixId = endNtc.HelixId;
        _endDirection = endNtc.Direction;

        _firstIsEnd = firstIsEnd;
        _secondIsEnd = secondIsEnd;
        _firstIsHead = firstIsHead;

        _sequenceLength = sequenceLength;
    }

    public void Do()
    {
        _loopout = DrawLoopout.CreateLoopout(_first, _second, _sequenceLength);
    }

    public void Undo()
    {
        GameObject startGO = FindNucleotide(_startId, _startHelixId, _startDirection);
        GameObject endGO = FindNucleotide(_endId, _endHelixId, _endDirection);
        _loopout = startGO.GetComponent<NucleotideComponent>().Xover;
        int prevStrandId = _loopout.GetComponent<XoverComponent>().PrevStrandId;

        DrawLoopout.EraseLoopout(_loopout, prevStrandId, _prevColor, _firstIsHead);
        if (!_firstIsEnd) { DrawMerge.MergeStrand(startGO); }
        if (!_secondIsEnd) { DrawMerge.MergeStrand(endGO); }
    }

    public void Redo()
    {
        GameObject startGO = FindNucleotide(_startId, _startHelixId, _startDirection);
        GameObject endGO = FindNucleotide(_endId, _endHelixId, _endDirection);

        _loopout = DrawLoopout.CreateLoopout(startGO, endGO, _sequenceLength);
    }
}
