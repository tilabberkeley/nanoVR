/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;

public class CreateCommand : ICommand
{
    private int _strandId;
    private int _startNuclId;
    private int _helixId;
    private int _direction;
    private int _endNuclId;
    private Color _color;

    public CreateCommand(NucleotideData startNucl, NucleotideData endNucl)
    {
        _strandId = s_numStrands;
        _startNuclId = startNucl.Id;
        _helixId = startNucl.HelixId;
        _direction = startNucl.Direction;
        _endNuclId = endNucl.Id;
        _color = Colors[s_numStrands % Colors.Length];
    }

    public void Do()
    {
        NucleotideData startNucl = Utils.FindNucleotideData(_startNuclId, _helixId, _direction);
        NucleotideData endNucl = Utils.FindNucleotideData(_endNuclId, _helixId, _direction);
        DrawNucleotideDynamic.CreateStrand(startNucl, endNucl, _strandId, _color);
    }

    public void Undo()
    {
        s_strandDict.TryGetValue(_strandId, out Strand strand);
        strand.DeleteStrand();
    }

    public void Redo()
    {
        NucleotideData startNucl = Utils.FindNucleotideData(_startNuclId, _helixId, _direction);
        NucleotideData endNucl = Utils.FindNucleotideData(_endNuclId, _helixId, _direction);
        DrawNucleotideDynamic.CreateStrand(startNucl, endNucl, _strandId, _color);
    }
}
