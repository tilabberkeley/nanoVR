/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

public class GridComponent : MonoBehaviour
{ 
    public bool Selected { get; set; }
    public Line Line { get; set; }
    public Helix Helix { get; set; }
    public Vector3 Position { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}
