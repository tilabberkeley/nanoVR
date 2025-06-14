/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;

/// <summary>
/// Command for file import.
/// </summary>
public class ImportCommand : ICommand
{
    private string _json;
    private List<DNAGrid> _grids;

    public ImportCommand (string json)
    {
        _json = json;
    }

    public void Do()
    {
        _grids = FileImport.Instance.ParseSC(_json);
    }

    public void Redo()
    {
        FileImport.Instance.ParseSC(_json);
    }

    public void Undo()
    {
        foreach (DNAGrid grid in _grids)
        {
            grid.DeleteGrid();
        }
    }
}
