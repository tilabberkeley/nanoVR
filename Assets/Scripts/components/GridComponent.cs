/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public class GridComponent : MonoBehaviour
{ 
    // Whether or not this grid component has been clicked on before.
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

    // Grid id that this grid component is apart of.
    private int _gridId;
    public int GridId { get { return _gridId; } set { _gridId = value; } }

    /// <summary>
    /// Returns neighboring grid components of this grid component.
    /// </summary>
    /// <returns>List of neighboring grid components.</returns>
    public List<GridComponent> getNeighborGridComponents()
    {
        return Grid.GetNeighborGridComponents(GridPoint);
    }
}
