/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */using UnityEngine;


public class GridComponent : MonoBehaviour
{
    private bool _selected = false;
    private Line _line = null;
    private Helix _helix = null;

    public bool Selected { get; set; }
    public Line Line { get; set; }

    public Helix Helix { get; set; }



}
