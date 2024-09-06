/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using UnityEngine.UI;
using static GlobalVariables;

/// <summary>
/// Handles all the operations for creating DNAGrids and adding Helices.
/// </summary>
public class DrawGrid : MonoBehaviour
{
    [SerializeField] private Dropdown directionDropdown;
    [SerializeField] private Dropdown gridTypeDropdown;
    private string plane;

    /// <summary>
    /// Creates either an empty Grid or Grid with selected helices.
    /// Called by New Grid button in Scene.
    /// </summary>
    public void CreateGrid()
    {
        if (SelectHelix.selectedHelices.Count == 0)
        {
            CreateGridHelper();
        }
        else
        {
            SelectHelix.CreateGridCollection();
        }
    }

    /// <summary>
    /// Creates a new blank grid. 
    /// </summary>
    public void CreateGridHelper()
    {
        plane = directionDropdown.options[directionDropdown.value].text;
        Vector3 direction = Camera.main.transform.rotation * Vector3.forward;
        Vector3 currPoint = Camera.main.transform.position + direction * 0.2f;
        ICommand command = new CreateGridCommand(s_numGrids.ToString(), plane, currPoint, gridTypeDropdown.options[gridTypeDropdown.value].text);
        CommandManager.AddCommand(command);
        //command.Do();
    }

    public static DNAGrid CreateGrid(string gridId, string plane, Vector3 position, string gridType)
    {
        DNAGrid grid;
        if (gridType.Equals("Square") || gridType.Equals("square"))
        {
            grid = new SquareGrid(gridId, plane, position);
        }
        else if (gridType.Equals("Honeycomb") || gridType.Equals("honeycomb"))
        {
            grid = new HoneycombGrid(gridId, plane, position);
        }
        else
        {
            grid = new HexGrid(gridId, plane, position);
        }
        if (s_visualMode)
        {
            s_visGridDict.Add(gridId, grid);
            s_numVisGrids += 1;
        }
        else
        {
            s_gridDict.Add(gridId, grid); 
            s_gridCopies.Add(s_numGrids.ToString(), 0);
            ObjectListManager.CreateGridButton(gridId);
            s_numGrids += 1;
        }
        
        return grid;
    }


    public static void CreateHelix(int id, Vector3 startPos, int length, string orientation, GridComponent gc)
    {
        if (!gc.Selected)
        {
            gc.Grid.DoAddHelix(id, startPos, length, orientation, gc);
            gc.Grid.CheckExpansion(gc);
        }
    }
}
