/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;

public class XoverCommand : ICommand
{
    private GameObject _startGO;
    private GameObject _endGO;
    private GameObject _xover;
    private bool _isHead;
    //private bool _isFirstEnd;
    //private bool _isSecondEnd;
    private Color _endColor;

    private int _startId;
    private int _startHelixId;
    private int _startDirection;
    private int _endId;
    private int _endHelixId;
    private int _endDirection;

    public XoverCommand(GameObject startGO, GameObject endGO, bool isHead, bool isFirstEnd, bool isSecondEnd)
    {
        _startGO = startGO;
        _endGO = endGO;
        _endColor = endGO.GetComponent<NucleotideComponent>().Color;
        _isHead = isHead;

        var startNtc = startGO.GetComponent<NucleotideComponent>();
        _startId = startNtc.Id;
        _startHelixId = startNtc.HelixId;
        _startDirection = startNtc.Direction;

        var endNtc = endGO.GetComponent<NucleotideComponent>();
        _endId = endNtc.Id;
        _endHelixId = endNtc.HelixId;
        _endDirection = endNtc.Direction;


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
        GameObject startGO = FindNucleotide(_startId, _startHelixId, _startDirection);
        _xover = startGO.GetComponent<NucleotideComponent>().Xover;
        DrawCrossover.EraseXover(_xover, _endId, _endColor, _isHead);
    }

    public void Redo()
    {
        GameObject startGO = FindNucleotide(_startId, _startHelixId, _startDirection);
        GameObject endGO = FindNucleotide(_endId, _endHelixId, _endDirection);
        DrawCrossover.CreateXover(startGO, endGO);
    }

    public GameObject FindNucleotide(int id, int helixId, int direction)
    {
        s_helixDict.TryGetValue(helixId, out Helix helix);
        return helix.GetNucleotide(id, direction);
    }
}
