/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public class GridComponent : MonoBehaviour
{
    protected Renderer _ntRenderer;
    protected Outline _outline;

    // Whether or not this grid component has been clicked on before.
    public bool Selected { get; set; }
  
    // Helix on grid component.
    public Helix Helix { get; set; }
    // 3D position of grid component.
    //private Vector3 _position;
    public Vector3 Position { get { return gameObject.transform.position; }}
    // 2D point of grid component on grid.
    public GridPoint GridPoint { get; set; }
    // Grid that this grid component is apart of.
    public DNAGrid Grid { get; set; }

    public string GridId { get { return Grid.Id; } }

    /// <summary>
    /// Returns neighboring grid components of this grid component.
    /// </summary>
    /// <returns>List of neighboring grid components.</returns>
    public List<GridComponent> getNeighborGridComponents()
    {
        return Grid.GetNeighborGridComponents(GridPoint);
    }

    protected virtual void Awake()
    {
        _ntRenderer = GetComponent<Renderer>();
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }
}
