/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using static Utils;

public class XoverCommand : ICommand
{
    private GameObject _first;
    private GameObject _second;
    private GameObject _xover;
    private bool _firstIsHead;
    private bool _firstIsEnd;
    private bool _secondIsEnd;
    private Color _secondColor;

    private int _startId;
    private int _startHelixId;
    private int _startDirection;
    private int _endId;
    private int _endHelixId;
    private int _endDirection;

    public XoverCommand(GameObject first, GameObject second, bool firstIsEnd, bool secondIsEnd, bool firstIsHead)
    {
        _first = first;
        _second = second;
        _secondColor = second.GetComponent<NucleotideComponent>().Color;

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
    }

    public void Do()
    {
        DrawCrossover.CreateXover(_first, _second);
    }

    public void Undo()
    {
        //DrawSplit.SplitStrand(_startGO, _endColor, _isHead);
        GameObject startGO = FindNucleotide(_startId, _startHelixId, _startDirection);
        GameObject endGO = FindNucleotide(_endId, _endHelixId, _endDirection);
        _xover = startGO.GetComponent<NucleotideComponent>().Xover;
        DrawCrossover.EraseXover(_xover, _endId, _secondColor, _firstIsHead);
        if (!_firstIsEnd) { DrawMerge.MergeStrand(startGO); }
        if (!_secondIsEnd) { DrawMerge.MergeStrand(endGO); }
    }

    public void Redo()
    {
        GameObject startGO = FindNucleotide(_startId, _startHelixId, _startDirection);
        GameObject endGO = FindNucleotide(_endId, _endHelixId, _endDirection);
        DrawCrossover.CreateXover(startGO, endGO);
    }
}
