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

    //protected virtual void Update()
    //{
    //    // Dynamically update backbone gameobject when its first and second nucleotide gameobjects move
    //    // They will be moving in the simulate environment.
    //    if ((_firstNucleotide != null && _firstNucleotide.transform.hasChanged)
    //        || _secondNucleotide != null && _secondNucleotide.transform.hasChanged)
    //    {
    //        _firstNucleotide.transform.hasChanged = false;
    //        _secondNucleotide.transform.hasChanged = false;

    //        Vector3 start = _firstNucleotide.transform.position;
    //        Vector3 end = _secondNucleotide.transform.position;

    //        // Scale        
    //        float dist = Vector3.Distance(end, start);
    //        transform.localScale = new Vector3(0.005f, dist / 2, 0.005f);

    //        // Position
    //        transform.position = (end + start) / 2.0F;

    //        // Rotation
    //        transform.up = end - start;
    //    }
    //}
}
