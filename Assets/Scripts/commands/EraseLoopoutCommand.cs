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
    private Color _savedColor;

    private int _startId;
    private int _startHelixId;
    private int _startDirection;
    private int _endId;
    private int _endHelixId;
    private int _endDirection;

    private int _sequenceLength;

    public EraseLoopoutCommand(GameObject loopout, int strandId)
    {
        _loopout = loopout;
        LoopoutComponent loopoutComponent = loopout.GetComponent<LoopoutComponent>();
        //SequenceComponent seqComp = loopout.GetComponent<SequenceComponent>();
        _startGO = loopoutComponent.PrevGO;
        _endGO = loopoutComponent.NextGO;
        _strandId = strandId;
        _savedColor = loopout.GetComponent<LoopoutComponent>().SavedColor;

        NucleotideComponent startNtc = _startGO.GetComponent<NucleotideComponent>();
        _startId = startNtc.Id;
        _startHelixId = startNtc.HelixId;
        _startDirection = startNtc.Direction;

        NucleotideComponent endNtc = _endGO.GetComponent<NucleotideComponent>();
        _endId = endNtc.Id;
        _endHelixId = endNtc.HelixId;
        _endDirection = endNtc.Direction;

        _sequenceLength = loopoutComponent.SequenceLength;
    }

    public void Do()
    {
        DrawLoopout.EraseLoopout(_loopout, _strandId, _savedColor, false);
    }

    public void Undo()
    {
        GameObject startGO = FindNucleotide(_startId, _startHelixId, _startDirection);
        GameObject endGO = FindNucleotide(_endId, _endHelixId, _endDirection);

        DrawLoopout.CreateLoopout(startGO, endGO, _sequenceLength);
    }

    public void Redo()
    {
        _loopout = FindNucleotide(_startId, _startHelixId, _startDirection).GetComponent<NucleotideComponent>().Xover;

        DrawLoopout.EraseLoopout(_loopout, _strandId, _savedColor, false);
    }
}
