/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using UnityEngine.InputSystem;

public class XRInputOptimizer : MonoBehaviour
{
    private void Awake()
    {
        /* These two lines should optimize the new Input System.
         * @source: https://forum.unity.com/threads/why-is-the-new-input-system-so-slow.1245331/
         * @source: https://twitter.com/pvncher/status/1620516968037244929?s=20
         */
        InputSystem.settings.SetInternalFeatureFlag("USE_OPTIMIZED_CONTROLS", true);
        InputSystem.settings.SetInternalFeatureFlag("USE_READ_VALUE_CACHING", true);
        //InputSystem.settings.SetInternalFeatureFlag("PARANOID_READ_VALUE_CACHING_CHECKS", true);
    }
}
