/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static Utils;

public class MoveStrandCommand : ICommand
{
    private GameObject _oldNucl;
    private int _oldId;
    private int _oldHelixId;
    private int _oldDirection;
    private GameObject _newNucl;
    private int _newId;
    private int _newHelixId;
    private int _newDirection;

    public MoveStrandCommand(GameObject oldNucl, GameObject newNucl)
    {
        _oldNucl = oldNucl;
        _newNucl = newNucl;
        var oldNtc = oldNucl.GetComponent<NucleotideComponent>();
        var newNtc = newNucl.GetComponent<NucleotideComponent>();
        _oldId = oldNtc.Id;
        _oldHelixId = oldNtc.HelixId;
        _oldDirection = oldNtc.Direction;
        _newId = newNtc.Id;
        _newHelixId = newNtc.HelixId;
        _newDirection = newNtc.Direction;
    }
    public void Do()
    {
        MoveStrand.Move(_oldNucl, _newNucl);
    }

    public void Redo()
    {
        GameObject oldNucl = FindNucleotide(_oldId, _oldHelixId, _oldDirection);
        GameObject newNucl = FindNucleotide(_newId, _newHelixId, _newDirection);
        MoveStrand.Move(oldNucl, newNucl);
    }

    public void Undo()
    {
        GameObject oldNucl = FindNucleotide(_oldId, _oldHelixId, _oldDirection);
        GameObject newNucl = FindNucleotide(_newId, _newHelixId, _newDirection);
        MoveStrand.Move(newNucl, oldNucl);
    }
}
