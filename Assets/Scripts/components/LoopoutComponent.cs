/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Component for loopout game objects. Treating a loopout as a crossover, except that it has a sequence associated with it. 
/// </summary>
public class LoopoutComponent : XoverComponent
{
    // Sequence length of this loopout.
    private int _sequenceLength;
    public int SequenceLength { get { return _sequenceLength; } set { _sequenceLength = value; } }

    // Sequence of this loopout.
    private string _sequence;
    public string Sequence { get { return _sequence; } set { _sequence = value; } }

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

    protected override void Update()
    {
        // TODO: Implement dynamic moving for loopouts
        return;
    }
}
