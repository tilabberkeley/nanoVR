/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;
using static Utils;

public class EraseXoverCommand : ICommand
{
    private XoverComponent _xover;
    private int _strandId;
    private Color _color;

    private int _startId;
    private int _startHelixId;
    private int _startDirection;
    private int _endId;
    private int _endHelixId;
    private int _endDirection;

    public EraseXoverCommand(XoverComponent xover)
    {
        _xover = xover;
        _strandId = s_numStrands;
        _color = xover.SavedColor;

        NucleotideData nd1 = xover.PrevNucl;
        _startId = nd1.Id;
        _startHelixId = nd1.HelixId;
        _startDirection = nd1.Direction;

        NucleotideData nd2 = xover.NextNucl;
        _endId = nd2.Id;
        _endHelixId = nd2.HelixId;
        _endDirection = nd2.Direction;
    }

    public void Do()
    {
        DrawCrossover.EraseXover(_xover, _strandId, _color);
    }

    public void Undo()
    {
        NucleotideData nd1 = FindNucleotideData(_startId, _startHelixId, _startDirection);
        NucleotideData nd2 = FindNucleotideData(_endId, _endHelixId, _endDirection);
        DrawCrossover.CreateXover(nd1, nd2);
    }

    public void Redo()
    {
        _xover = FindNucleotideData(_startId, _startHelixId, _startDirection).Xover;
        DrawCrossover.EraseXover(_xover, _strandId, _color);
    }
}
