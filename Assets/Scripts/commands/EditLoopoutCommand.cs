/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using static Utils;

public class EditLoopoutCommand : ICommand
{
    LoopoutComponent _loopout;

    private int _firstId;
    private int _firstHelixId;
    private int _firstDirection;

    private int _newLength;
    private int _oldLength;

    public EditLoopoutCommand(LoopoutComponent loopout, int newLength)
    {
        _loopout = loopout;

        NucleotideData nd = loopout.NextNucl;
        _firstId = nd.Id;
        _firstHelixId = nd.HelixId;
        _firstDirection = nd.Direction;

        _newLength = newLength;
        _oldLength = loopout.SequenceLength;
    }

    public void Do()
    {
        EditOptionsManager.EditLoopout(_loopout, _newLength);
    }

    public void Redo()
    {
        LoopoutComponent loopout = (LoopoutComponent)FindNucleotideData(_firstId, _firstHelixId, _firstDirection).Xover;
        EditOptionsManager.EditLoopout(loopout, _newLength);
    }

    public void Undo()
    {
        LoopoutComponent loopout = (LoopoutComponent)FindNucleotideData(_firstId, _firstHelixId, _firstDirection).Xover;
        EditOptionsManager.EditLoopout(loopout, _oldLength);
    }
}
