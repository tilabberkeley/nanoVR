/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

/// <summary>
/// This component handles DNA sequencing. Attached along with NucleotideComponent and LoopoutComponent.
/// </summary>
public class SequenceComponent : MonoBehaviour
{
    // Sequence length of this loopout.
    private int _sequenceLength;
    public int SequenceLength { get => _sequenceLength; set => _sequenceLength = value; }

    // Sequence of this loopout.
    private string _sequence;
    public string Sequence { get => _sequence; set => _sequence = value; }
}
