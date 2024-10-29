/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

/// <summary>
/// Script component for the backbone game object.
/// </summary>
public class BackBoneComponent : DNAComponent
{
    /// <summary>
    /// First nucleotide game object this backbone is connected to.
    /// Note the direction of the nucleotides isn't necessary here because this 
    /// is only used for simulation purposes 9/23/24
    /// So, they will only be set (not be null) in the simulation context.
    /// </summary>
    private GameObject _firstNucleotide = null;
    public GameObject FirstNucleotide { get { return _firstNucleotide; } set { _firstNucleotide = value; } }

    /// <summary>
    /// Second nucleotide game object this backbone is connected to.
    /// Note the direction of the nucleotides isn't necessary here because this 
    /// is only used for simulation purposes 9/23/24.
    /// So, they will only be set (not be null) in the simulation context.
    /// </summary>
    private GameObject _secondNucleotide = null;
    public GameObject SecondNucleotide { get { return _secondNucleotide; } set { _secondNucleotide = value; } }

    protected override void Awake()
    {
        base.Awake();
        _isBackbone = true;
    }
}
