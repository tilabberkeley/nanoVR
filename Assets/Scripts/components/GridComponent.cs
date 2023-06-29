/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public class GridComponent : MonoBehaviour
{ 
    public bool Selected { get; set; }
    // Line on grid component.
    public Line Line { get; set; }
    // Helix on grid component.
    public Helix Helix { get; set; }
    // 3D position of grid component.
    public Vector3 Position { get; set; }
    // 2D point of grid component on grid.
    public GridPoint GridPoint { get; set; }
    // Grid that this grid component is apart of.
    public Grid Grid { get; set; }

    /// <summary>
    /// Returns neighboring grid components of this grid component.
    /// </summary>
    /// <returns>List of neighboring grid components.</returns>
    public List<GridComponent> getNeighborGridComponents()
    {
        return Grid.getNeighborGridComponents(GridPoint);
    }
}
