/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;

public class SplitCommand : ICommand
{
    private GameObject _go;
    private Color _color;

    public SplitCommand(GameObject go, Color color)
    {
        _go = go;
        _color = color;
    }

    public void Do()
    {
        DrawSplit.SplitStrand(_go, _color, false);
    }
    public void Undo()
    {
        DrawMerge.MergeStrand(_go);
    }

    public void Redo()
    {
        DrawSplit.SplitStrand(_go, _color, false);
    }
}
