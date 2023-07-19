/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public class XoverCommand : ICommand
{
    private GameObject _startGO;
    private GameObject _endGO;
    private GameObject _xover;
    private bool _isHead;
    private bool _isFirstEnd;
    private bool _isSecondEnd;
    private int _endId;
    private Color _endColor;

    public XoverCommand(GameObject startGO, GameObject endGO, bool isHead, bool isFirstEnd, bool isSecondEnd)
    {
        _startGO = startGO;
        _endGO = endGO;
        //_endId = endGO.GetComponent<NucleotideComponent>().StrandId;
        //_endColor = endGO.GetComponent<NucleotideComponent>().GetColor();
        //_isHead = isHead;
       // _isFirstEnd = isFirstEnd;
       // _isSecondEnd = isSecondEnd;
    }

    public void Do()
    {
        DrawCrossover.CreateXover(_startGO, _endGO);
    }

    public void Undo()
    {
        //DrawSplit.SplitStrand(_startGO, _endColor, _isHead);
        _xover = _startGO.GetComponent<NucleotideComponent>().Xover;
        DrawCrossover.EraseXover(_xover);
        
       
    }

    public void Redo()
    {
        DrawCrossover.CreateXover(_startGO, _endGO);
    }
}
