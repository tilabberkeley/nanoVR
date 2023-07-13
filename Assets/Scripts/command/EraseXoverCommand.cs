/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;

public class EraseXoverCommand : ICommand
{
    private GameObject _xover;
    private GameObject _startGO;
    private GameObject _endGO;
    private int _strandId;
    private Color _color;

    public EraseXoverCommand(GameObject xover, int strandId, Color color)
    {
        _xover = xover;
        var xoverComp = xover.GetComponent<XoverComponent>();
        _startGO = xoverComp.GetPrevGO();
        _endGO = xoverComp.GetNextGO();
        _strandId = strandId;
        _color = color;
    }

    public void Do()
    {
        DrawCrossover.EraseXover(_xover, _strandId, _color, false);
    }

    public void Undo()
    {
        // _xover does not exist after it gets erased. must created new xover
        _xover = DrawCrossover.CreateXover(_startGO, _endGO);
    }

    public void Redo()
    {
        DrawCrossover.EraseXover(_xover, _strandId, _color, false);
    }
}
