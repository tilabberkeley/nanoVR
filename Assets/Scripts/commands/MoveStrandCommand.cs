/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

public class MoveStrandCommand : ICommand
{
    private GameObject _oldNucl;
    private GameObject _newNucl;

    public MoveStrandCommand(GameObject oldNucl, GameObject newNucl)
    {
        _oldNucl = oldNucl;
        _newNucl = newNucl;
    }
    public void Do()
    {
        MoveStrand.Move(_oldNucl, _newNucl);
    }

    public void Redo()
    {
        MoveStrand.Move(_oldNucl, _newNucl);
    }

    public void Undo()
    {
        MoveStrand.Move(_newNucl, _oldNucl);
    }
}
