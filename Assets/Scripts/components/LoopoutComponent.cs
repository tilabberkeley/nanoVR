/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

/// <summary>
/// Component for loopout game objects. Treating a loopout as a crossover, except that it has a sequence associated with it. 
/// </summary>
public class LoopoutComponent : XoverComponent
{
    // Sequence length of this loopout.
    private int _sequenceLength;
    public int SequenceLength { get => _sequenceLength; set => _sequenceLength = value; }

    // Sequence of this loopout.
    private string _sequence = "";
    public string Sequence { get => _sequence; set => _sequence = value; }

    // Changing color of loopout done through mesh renderer
    public override Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            GetComponent<MeshRenderer>().material.SetColor("_Color", value);
        }
    }

    public override void UpdateXover()
    {
        // TODO: Add implementation
    }
}
