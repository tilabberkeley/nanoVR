/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public class XoverCommand : MonoBehaviour, ICommand
{
    private GameObject _startGO;
    private GameObject _endGO;
    private bool _isHead;
    private bool _isFirstEnd;
    private bool _isSecondEnd;
    private int _endId;
    private Color _endColor;

    public XoverCommand(GameObject startGO, GameObject endGO, bool isHead, bool isFirstEnd, bool isSecondEnd)
    {
        _startGO = startGO;
        _endGO = endGO;
        _endId = endGO.GetComponent<NucleotideComponent>().GetStrandId();
        _endColor = endGO.GetComponent<NucleotideComponent>().GetColor();
        _isHead = isHead;
        _isFirstEnd = isFirstEnd;
        _isSecondEnd = isSecondEnd;
    }

    public void Do()
    {
        DrawCrossover.CreateXover(_startGO, _endGO);
    }

    public void Undo()
    {
        //DrawSplit.SplitStrand(_startGO, _endColor, _isHead);
        List<GameObject> splitNucls = DrawCrossover.SplitStrand(_startGO, _isHead);
        DrawCrossover.CreateStrand(splitNucls, _endId, _endColor);
        if (!_isFirstEnd)
        {
            DrawMerge.MergeStrand(_startGO);
        }
        if (!_isSecondEnd)
        {
            DrawMerge.MergeStrand(_endGO);
        }
    }

    public void Redo()
    {
        DrawCrossover.CreateXover(_startGO, _endGO);
    }
}
