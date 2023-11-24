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
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _isBackbone = true;
    }
}
