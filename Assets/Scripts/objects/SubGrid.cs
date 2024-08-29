using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class SubGrid
{
    private int _id;
    public int Id { get => _id; }

    private Dictionary<int, GridComponent> _gridComponents;
    public Dictionary<int, GridComponent> GridComponents { get => _gridComponents; set { _gridComponents = value; } }

    public SubGrid(int id, List<GridComponent> gridComponents)
    {
        _id = id;

        /*
         * 1. Find Helix that is closest to SubGrid center (google this).
         * 2. Make this Helix [0, 0] of the new SubGrid.
         * 3. Calculate all the other selected Helix grid coordinates relative to this new center.
         * 4. Build a Grid with these selected helices, expand as needed (similar to file import).
         * 5. Assign selected helices to new Grid's grid circles.
         * 6. If necessary, remove Helix objects from old Grid's grid circles.
        */



        foreach (GridComponent gc in gridComponents) 
        {
            _gridComponents.Add(gc.Helix.Id, gc);
        }
    }
}
