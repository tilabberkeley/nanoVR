/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using static Utils;

public class EraseXoverCommand : ICommand
{
    private GameObject _xover;
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

    public EraseXoverCommand(GameObject xover, int strandId, Color color)
    {
        _xover = xover;
        var xoverComp = xover.GetComponent<XoverComponent>();
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
        DrawCrossover.EraseXover(_xover, _strandId, _color, false);
    }

    public void Undo()
    {
        GameObject prevGO = FindNucleotide(_startId, _startHelixId, _startDirection);
        GameObject nextGO = FindNucleotide(_endId, _endHelixId, _endDirection);

        _xover = DrawCrossover.CreateXover(prevGO, nextGO);
    }

    public void Redo()
    {
        _xover = FindNucleotide(_startId, _startHelixId, _startDirection).GetComponent<NucleotideComponent>().Xover;

        DrawCrossover.EraseXover(_xover, _strandId, _color, false);
    }
}
