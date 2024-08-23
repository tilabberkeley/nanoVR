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
        foreach (GridComponent gc in gridComponents) 
        {
            _gridComponents.Add(gc.Helix.Id, gc);
        }
    }
}
