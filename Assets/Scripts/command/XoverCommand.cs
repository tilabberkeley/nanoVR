using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XoverCommand : MonoBehaviour, ICommand
{
    private GameObject _startGO;
    private GameObject _endGO;
    private int _endId;
    private Color _startColor;
    private Color _endColor;

    public XoverCommand(GameObject startGO, GameObject endGO)
    {
        _startGO = startGO;
        _endGO = endGO;
        _endId = endGO.GetComponent<NucleotideComponent>().GetStrandId();
        _startColor = startGO.GetComponent<NucleotideComponent>().GetColor();
        _endColor = endGO.GetComponent<NucleotideComponent>().GetColor();
    }

    public void Do()
    {
        DrawCrossover.CreateXover(_startGO, _endGO);
    }

    public void Undo()
    {
        List<GameObject> splitNucls = DrawCrossover.SplitStrand(_startGO, false);
        DrawCrossover.CreateStrand(splitNucls, _endId, _endColor);
        DrawMerge.MergeStrand(_startGO);
        DrawMerge.MergeStrand(_endGO);
    }

    public void Redo()
    {
        DrawCrossover.CreateXover(_startGO, _endGO);
    }
}
