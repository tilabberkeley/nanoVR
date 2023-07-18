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
    public DeleteHelixCommand(int id)
    {
        _id = id;
        s_helixDict.TryGetValue(id, out Helix helix);
        _startPoint = helix.StartPoint;
        _endPoint = helix.EndPoint;
        _orientation = helix.Orientation;
        _length = helix.Length;
        _gridComponent = helix.GridComponent;
    }

    public void Do()
    {
        SelectHelix.DeleteHelix(_id);
    }

    public void Redo()
    {
        SelectHelix.DeleteHelix(_id);
    }

    public void Undo()
    {
        Grid.AddHelix(_id, _startPoint, _endPoint, _orientation, _length, _gridComponent);
    }
}
