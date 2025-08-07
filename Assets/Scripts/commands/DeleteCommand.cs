/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using static GlobalVariables;

public class DeleteCommand : ICommand
{
    private JArray _jsonStrands;
    private List<Strand> _strands;

    public DeleteCommand(List<Strand> strands)
    {
        _strands = new List<Strand>(strands);
        _jsonStrands = FileExport.StrandsExport(strands);
    }
    public void Do()
    {
        foreach (Strand strand in _strands)
        {
            SelectStrand.DeleteStrand(strand);
        }
    }

    public void Redo()
    {
        foreach (Strand strand in _strands)
        {
            SelectStrand.DeleteStrand(strand);
        }
        _strands.Clear();
    }

    public void Undo()
    {
        CoRunner.Instance.Run(FileImport.Instance.ParseStrands(_jsonStrands, s_numHelices, _strands, new Dictionary<int, int>()));
    }
}
