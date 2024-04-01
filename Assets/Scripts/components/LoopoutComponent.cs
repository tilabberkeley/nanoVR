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
    // Sequence of this loopout.
    private string _sequence;
    public string Sequence { get { return _sequence; } set { _sequence = value; } }

    public override Color Color { get { return _color; } set { _color = value; } }
}
