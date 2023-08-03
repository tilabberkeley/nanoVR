/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

public class MoveHelixCommand : ICommand
{
    private GameObject _oldCircle;
    private GameObject _newCircle;
    public MoveHelixCommand(GameObject oldCircle, GameObject newCircle)
    {
        _oldCircle = oldCircle;
        _newCircle = newCircle;
    }
       
    public void Do()
    {
        MoveHelix.Move(_oldCircle, _newCircle);
    }

    public void Redo()
    {
        MoveHelix.Move(_oldCircle, _newCircle);

    }

    public void Undo()
    {
        MoveHelix.Move(_newCircle, _oldCircle);

    }
}
