using System.Collections;
using UnityEngine;

/// <summary>
/// Component attached to Helix collider GameObject for Helix View mode.
/// Not to be confused with the Helix object found in Scripts/Objects/Helix
/// </summary>
 public class HelixComponent : MonoBehaviour
 {

    private Helix _helix;
    public Helix Helix { get => _helix; set => _helix = value; }
}
