/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component for the interactable sphere of a loopout.
/// </summary>
public class LoopoutInteractableComponent : MonoBehaviour
{
    // Associated loopout
    private LoopoutComponent _loopout;
    public LoopoutComponent Loopout { get { return _loopout; } set { _loopout = value; } }
}
