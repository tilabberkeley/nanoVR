using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

public class DeleteHelixCommand : ICommand
{
    private int _id;
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private string _orientation;
    private int _length;
    private GridComponent _gridComponent;
    private DNAGrid _grid;
    public DeleteHelixCommand(int id)
    {
        _id = id;
        s_helixDict.TryGetValue(id, out Helix helix);
        _startPoint = helix.StartPoint;
        _endPoint = helix.EndPoint;
        _orientation = helix.Orientation;
        _length = helix.Length;
        _gridComponent = helix._gridComponent;
        _grid = _gridComponent.Grid;
    }

    public void Do()
    {
        SelectHelix.DeleteHelix(_id);
    }

    public void Redo()
    {
        s_helixDict.TryGetValue(_id, out Helix helix);
        _startPoint = helix.StartPoint;
        _length = helix.Length;
        _gridComponent = helix._gridComponent;
        SelectHelix.DeleteHelix(_id);
    }

    public void Undo()
    {
        _grid.AddHelix(_id, _startPoint, _length, _orientation, _gridComponent);
    }
}
