/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Command for file import.
/// </summary>
public class ImportCommand : MonoBehaviour, ICommand
{
    private string _json;
    List<DNAGrid> _grids;

    public ImportCommand (string json)
    {
        _json = json;
        _grids = new List<DNAGrid>();
    }

    public void Do()
    {
        StartCoroutine(FileImport.ParseSC(_json));
    }

    public void Redo()
    {
        StartCoroutine(FileImport.ParseSC(_json));
    }

    public void Undo()
    {
        /*foreach (DNAGrid grid in _grids)
        {
            grid.DeleteGrid();
        }*/
    }
}
