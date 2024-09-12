/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

public class SelectHelix : MonoBehaviour
{
    [SerializeField] private XRNode _leftXRNode;
    [SerializeField] private XRNode _rightXRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _leftDevice;
    private InputDevice _rightDevice;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool rightTriggerReleased = true;
    private bool leftTriggerReleased = true;
    private bool axisReleased = true;
    private bool selectMultiple = false;
    private static RaycastHit s_hit;
    public static List<GridComponent> selectedHelices = new List<GridComponent>();

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_leftXRNode, _devices);
        if (_devices.Count > 0)
        {
            _leftDevice = _devices[0];
        }

        InputDevices.GetDevicesAtXRNode(_rightXRNode, _devices);
        if (_devices.Count > 0)
        {
            _rightDevice = _devices[0];
        }
    }

    private void OnEnable()
    {
        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }
    }

    private void Update()
    {
        if (s_hideStencils)
        {
            return;
        }

        if (!_leftDevice.isValid || !_rightDevice.isValid)
        {
            GetDevice();
        }

        _leftDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue);
        _rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool axisClick);

        /*if (leftTriggerValue)
        {
            leftTriggerReleased = false;
            selectMultiple = true;
        }*/

        if (rightTriggerValue && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            rightTriggerReleased = false;
            ResetHelices();
        }

        if (rightTriggerReleased && rightTriggerValue
           && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            rightTriggerReleased = false;
            GridComponent gc = s_hit.collider.gameObject.GetComponent<GridComponent>();
            if (gc != null && gc.Selected)
            {
/*                if (!selectMultiple)
                {
                    ResetHelices();
                }*/
                HighlightHelix(gc.gameObject);
                selectedHelices.Add(gc);
            }
        }

        if (axisClick && axisReleased)
        {
            axisReleased = false;
            // Note: Make undo/redo command? DY 9/11
            foreach (GridComponent gc in selectedHelices)
            {
                DeleteHelix(gc.Helix.Id);
            }
        }

        if (!axisClick)
        {
            axisReleased = true;
        }

        // Resets triggers do avoid multiple selections.                                              
        if (!rightTriggerValue)
        {
            rightTriggerReleased = true;
        }

/*        if (!leftTriggerValue)
        {
            leftTriggerReleased = true;
            selectMultiple = false;
        }*/

    }

    /// <summary>
    /// Resets the start and end nucleotides.
    /// </summary>
    public static void ResetHelices()
    {
        UnhighlightHelices();
        selectedHelices.Clear();
    }

    public static void HighlightHelix(GameObject go)
    {
        GridComponent gc = go.GetComponent<GridComponent>();
        Highlight.HighlightGridCircle(gc);
    }

    public static void UnhighlightHelices()
    {
        foreach (GridComponent gc in selectedHelices)
        {
            UnhighlightHelix(gc.gameObject);

        }
    }

    public static void UnhighlightHelix(GameObject go)
    {
        if (go == null) { return; }
        GridComponent gc = go.GetComponent<GridComponent>();
        Highlight.UnhighlightGridCircle(gc);
    }

    public static void DoDeleteHelix(int id)
    {
        ICommand command = new DeleteHelixCommand(id);
        CommandManager.AddCommand(command);
    }

    public static void DeleteHelix(int id)
    {
        s_helixDict.TryGetValue(id, out Helix helix);
        if (!helix.IsEmpty())
        {
            Debug.Log("Helix not empty. Cannot delete");
            return;
        }
        helix.DeleteHelix();
    }

    /// <summary>
    /// Creates new Grid object which contains a collection of selected Helices made by user.
    /// Called by CreateGridCollection button in scene.
    /// </summary>
    public static void CreateGridCollection()
    {
        /*
         * 1. Find Helix that is closest to SubGrid center (google this).
         * 2. Make this Helix [0, 0] of the new SubGrid.
         * 3. Calculate all the other selected Helix grid coordinates relative to this new center.
         * 4. Build a Grid with these selected helices, expand as needed (similar to file import).
         * 5. Assign selected helices to new Grid's grid circles.
         * 6. If necessary, remove Helix objects from old Grid's grid circles.
        */

        // Check selected helices are from same grid
        bool valid = CheckHelicesFromSameGrid(selectedHelices);
        if (!valid)
        {
            Debug.Log("Selected helices must be from same Grid to create new Grid collection.");
            return;
        }

        // Step 1
        float meanX = (float) selectedHelices.Average(gc => gc.GridPoint.X);
        float meanY = (float) selectedHelices.Average(gc => gc.GridPoint.Y);
        Vector2 meanPoint = new Vector2(meanX, meanY);

        Vector2 closestCoordinate = Vector2.zero;
        float closestDistance = float.MaxValue;

        foreach (GridComponent gc in selectedHelices)
        {
            Vector2 coord = new Vector2(gc.GridPoint.X, gc.GridPoint.Y);
            float distance = Vector2.Distance(meanPoint, coord);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCoordinate = coord;
            }
        }

        // Step 2 & 3
        Dictionary<GridPoint, GridComponent> newCoordinates = new Dictionary<GridPoint, GridComponent>();
        foreach (GridComponent gc in selectedHelices)
        {
            Vector2 coord = new Vector2(gc.GridPoint.X, gc.GridPoint.Y);
            Vector2 newCoord = coord - closestCoordinate;
            GridPoint gp = new GridPoint((int) newCoord.x, (int) newCoord.y);
            newCoordinates.Add(gp, gc);
        }

        // Step 4/5
        DNAGrid grid = DrawGrid.CreateGrid(s_numGrids.ToString(), selectedHelices[0].Grid.Plane, selectedHelices[0].Grid.Position, selectedHelices[0].Grid.Type);

        foreach (var pair in newCoordinates)
        {
            int xGrid = pair.Key.X;
            int yGrid = pair.Key.Y;

            /**
             * Expands grid if necessary so that helix coordinates exist.
             */
            GridPoint minBound = grid.MinimumBound;
            GridPoint maxBound = grid.MaximumBound;
            while (xGrid <= minBound.X)
            {
                grid.ExpandWest();
            }
            while (xGrid >= maxBound.X)
            {
                grid.ExpandEast();
            }
            while (yGrid <= minBound.Y)
            {
                grid.ExpandSouth();
            }
            while (yGrid >= maxBound.Y)
            {
                grid.ExpandNorth();
            }

            int xInd = grid.GridXToIndex(xGrid);
            int yInd = grid.GridYToIndex(yGrid);
            GridComponent gc = grid.Grid2D[xInd, yInd];
            gc.Helix = pair.Value.Helix;
        }
    }

    private static bool CheckHelicesFromSameGrid(List<GridComponent> selectedHelices)
    {
        string gridId = selectedHelices[0].GridId;
        foreach(GridComponent gc in selectedHelices)
        {
            if (gc.GridId.Equals(gridId))
            {
                return false;
            }
        }
        return true;
    }
}
