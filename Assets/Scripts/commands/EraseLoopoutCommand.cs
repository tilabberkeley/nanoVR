/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;
using static Utils;

public class EraseLoopoutCommand : ICommand
{
    private LoopoutComponent _loopout;
    private int _strandId;
    private Color _color;

    private int _startId;
    private int _startHelixId;
    private int _startDirection;
    private int _endId;
    private int _endHelixId;
    private int _endDirection;
    private int _sequenceLength;
    private string _sequence;

    public EraseLoopoutCommand(LoopoutComponent loopout)
    {
        _loopout = loopout;
        _sequenceLength = loopout.SequenceLength;
        _sequence = loopout.Sequence;
        _strandId = s_numStrands;
        _color = loopout.SavedColor;

        NucleotideData nd1 = loopout.PrevNucl;
        _startId = nd1.Id;
        _startHelixId = nd1.HelixId;
        _startDirection = nd1.Direction;

        NucleotideData nd2 = loopout.NextNucl;
        _endId = nd2.Id;
        _endHelixId = nd2.HelixId;
        _endDirection = nd2.Direction;
    }

    public void Do()
    {
        DrawCrossover.EraseXover(_loopout, _strandId, _color);
    }

    public void Undo()
    {
        NucleotideData nd1 = FindNucleotideData(_startId, _startHelixId, _startDirection);
        NucleotideData nd2 = FindNucleotideData(_endId, _endHelixId, _endDirection);
        _loopout = DrawLoopout.CreateLoopout(nd1, nd2, _sequenceLength);
        _loopout.Sequence = _sequence;
    }

    public void Redo()
    {
        _loopout = (LoopoutComponent) FindNucleotideData(_startId, _startHelixId, _startDirection).Xover;
        DrawCrossover.EraseXover(_loopout, _strandId, _color);
    }
}
