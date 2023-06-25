using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeCommand : ICommand
{
    private GameObject _go;
    private Color _color;
    private bool _splitAfter;

    public MergeCommand(GameObject go, Color color, bool splitAfter)
    {
        _go = go;
        _color = color;
        _splitAfter = splitAfter;
    }

    public void Do()
    {
        DrawMerge.MergeStrand(_go);
    }

    public void Undo()
    {
        DrawSplit.SplitStrand(_go, _color, _splitAfter);
    }

    public void Redo()
    {
        DrawMerge.MergeStrand(_go);
    }
}
