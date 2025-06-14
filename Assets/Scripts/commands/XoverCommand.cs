/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static Utils;

public class XoverCommand : ICommand
{
    private XoverComponent _xover; 
    private Color _prevColor;

    private int _startId;
    private int _startHelixId;
    private int _startDirection;
    private int _endId;
    private int _endHelixId;
    private int _endDirection;
    private int _prevStrandId;

    public XoverCommand(NucleotideData first, NucleotideData second)
    {
        _prevColor = second.Color;
        _prevStrandId = second.StrandId;

        _startId = first.Id;
        _startHelixId = first.HelixId;
        _startDirection = first.Direction;

        _endId = second.Id;
        _endHelixId = second.HelixId;
        _endDirection = second.Direction;
    }

    public void Do()
    {
        NucleotideData nd1 = FindNucleotideData(_startId, _startHelixId, _startDirection);
        NucleotideData nd2 = FindNucleotideData(_endId, _endHelixId, _endDirection);
        _xover = DrawCrossover.CreateXover(nd1, nd2);
    }

    public void Undo()
    {
        DrawCrossover.EraseXover(_xover, _prevStrandId, _prevColor);
    }

    public void Redo()
    {
        NucleotideData nd1 = FindNucleotideData(_startId, _startHelixId, _startDirection);
        NucleotideData nd2 = FindNucleotideData(_endId, _endHelixId, _endDirection);
        _xover = DrawCrossover.CreateXover(nd1, nd2);
    }
}
