/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

/// <summary>
/// Script component for the backbone game object.
/// </summary>
public class BackBoneComponent : MonoBehaviour
{
    private Outline _outline;

    // Start is called before the first frame update
    void Awake()
    {
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }
}
