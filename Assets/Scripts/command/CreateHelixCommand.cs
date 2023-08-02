/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;

public class CreateHelixCommand : ICommand
{
    // Helix id.
    private int _id;
    private Vector3 _startPoint;
    private Vector3 _endPoint;

    // Number of nucleotides in helix.
    private int _length;
    private string _orientation;

    // 3D position of grid circle.
    private GridComponent _gridComp;

    // Grid that helix belongs to.
    private Grid _grid;

    public CreateHelixCommand(int id, Vector3 startPoint, Vector3 endPoint, string orientation, GridComponent gridComponent, Grid grid)
    {
        //_grid = grid;
        _id = id;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _length = 64;
        _orientation = orientation;
        _gridComp = gridComponent;
        _grid = grid;
    }

    public void Do()
    {
        _grid.AddHelix(_id, _startPoint, _orientation, _length, _gridComp);
    }

    public void Redo()
    {
        _grid.AddHelix(_id, _startPoint, _orientation, _length, _gridComp);
    }

    public void Undo()
    {
        s_helixDict.TryGetValue(_id, out Helix helix);
        _length = helix.Length;
        SelectHelix.DeleteHelix(_id);
    }
}
