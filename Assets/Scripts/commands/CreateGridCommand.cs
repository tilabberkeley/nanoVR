/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

public class CreateGridCommand : ICommand
{
    private DNAGrid _grid;
    private string _id;
    private string _plane;
    private Vector3 _startPos;
    private string _gridType;

    public CreateGridCommand(string id, string plane, Vector3 startPos, string gridType)
    {
        _id = id;
        _plane = plane;
        _startPos = startPos;
        _gridType = gridType;
    }

    public void Do()
    {
        _grid = DrawGrid.CreateGrid(_id, _plane, _startPos, _gridType);
    }

    public void Redo()
    {
        _grid = DrawGrid.CreateGrid(_id, _plane, _startPos, _gridType);
    }

    public void Undo()
    {
        _grid.DeleteGrid();
    }
}
