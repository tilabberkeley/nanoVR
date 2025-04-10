/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Abstract class for Grid objects.
/// </summary>
public abstract class DNAGrid
{
    /* Constants. Increase GRIDCIRCLESIZEFACTOR to decrease distance between grid circles and vice versa. */
    protected const int STARTLENGTH = 5;
    protected const int STARTWIDTH = 5;
    /*protected const float GRIDCIRCLESIZEFACTOR = 6.0f;
    protected const float DIAMETER = 1 / GRIDCIRCLESIZEFACTOR;
    protected const float RADIUS = DIAMETER / 2;*/

    public abstract string Type { get; }

    protected string _id;
    public string Id { get { return _id; } }

    protected string _plane;
    public string Plane { get { return _plane; } }

    /// <summary>
    /// Grid's position based on (0, 0, 0) coordinate. This is used by .sc files.
    /// </summary>
    private Vector3 _position;
    public Vector3 Position
    {
        get
        {
            int i = GridXToIndex(0);
            int j = GridYToIndex(0);
            if (_grid2D[i, j] == null)
            {
                return _position;
            }
            return _grid2D[i, j].Position;
        }
    }

    /// <summary>
    /// Location, orientation, and scale of this grid.
    /// This main matrix defines how the grid circles will be positioned, rotated, and scaled.
    /// Each of the grid circles have their own local matrix describing their location in the grid.
    /// </summary>
    private Matrix4x4 _gridMatrix = Matrix4x4.identity;
    public Matrix4x4 GridMatrix { get { return _gridMatrix; } }

    public GridCircleData StartGridCircle
    {
        get
        {
            int i = GridXToIndex(0);
            int j = GridYToIndex(0);
            if (_grid2D[i, j] == null)
            {
                return null;
            }
            return _grid2D[i, j];
        }
    }

    protected GridCircleData[,] _grid2D;
    public GridCircleData[,] Grid2D { get { return _grid2D; } }
 
    protected int _length;
    public int Length { get { return _length; } }

    protected int _width;
    public int Width { get { return _width; } }

    protected int _size;
    protected GridPoint _minimumBound;
    public GridPoint MinimumBound { get => _minimumBound; }
    protected GridPoint _maximumBound;
    public GridPoint MaximumBound { get => _maximumBound; }


    /* Need to keep track of the number of south and west positions because they are used to
     * calculate the offset from the _startPos to generate the grid circles in the scene. 
     * For example, if expandWest was called multiple times, expandNorth needs to know how 
     * many times that happened to correctly offset its new grid circle generations. 
     */
    protected int _numSouthExpansions;
    protected int _numWestExpansions;

    /// <summary>
    /// Grid constructor. 
    /// </summary>
    /// <param name="id">Id number of this grid.</param>
    /// <param name="plane">Plane defintion.</param>
    /// <param name="startPos">3D location of where this grid starts.</param>
    public DNAGrid(string id, string plane, Vector3 startPos)
    {
        _id = id;
        _plane = plane;
        _size = 0;
        _position = startPos;
        SetBounds();

        // Decide how to orient the grid’s plane:
        Quaternion planeRotation;
        switch (_plane)
        {
            case "XY":
                planeRotation = Quaternion.identity;
                // or some custom rotation if needed
                break;
            case "XZ":
                // Rotate -90 around X so that local "y" becomes world "z"
                planeRotation = Quaternion.Euler(-90f, 0f, 0f);
                break;
            case "YZ":
                // Rotate 90 around Y so that local "x" becomes world "z"
                planeRotation = Quaternion.Euler(0f, 90f, 0f);
                break;
            default:
                Debug.LogWarning($"Unknown plane type '{_plane}', defaulting to XY");
                planeRotation = Quaternion.identity;
                break;
        }

        // Construct the grid's transform:
        // - Position = startPos (the "center" of the grid)
        // - Rotation = planeRotation
        // - Scale = previous scale of the old grid circle prefab
        _gridMatrix = Matrix4x4.TRS(startPos, planeRotation, Vector3.one);

        _grid2D = new GridCircleData[_length, _width];

        DrawGrid();
    }

    /// <summary>
    /// Sets fields for bounds and expansions.
    /// </summary>
    private void SetBounds()
    {
        _length = STARTLENGTH;
        _width = STARTWIDTH;
        _minimumBound = new GridPoint(-_length / 2, -_width / 2);
        _maximumBound = new GridPoint(_length / 2, _width / 2);
        _numWestExpansions = 0;
        _numSouthExpansions = 0;
    }

    /// <summary>
    /// Computes the local local XY offset of the grid circle depending on the x and y offset in the grid. 
    /// </summary>
    /// <param name="gridPoint">Grid point to generate circle at.</param>
    /// <param name="xOffset">x direction offset (depends on expansions).</param>
    /// <param name="yOffset">y direction offset (depends on expansions).</param>
    /// <returns>Local XY offset of the grid circle.</returns>
    protected abstract Vector2 ComputeLocalOffset(GridPoint gridPoint, int xOffset, int yOffset);

    /// <summary>
    /// Creates a grid circle at the specified grid point.
    /// </summary>
    /// <param name="gridPoint">Grid point to generate circle at.</param>
    /// <param name="xOffset">x direction offset (depends on expansions).</param>
    /// <param name="yOffset">y direction offset (depends on expansions).</param>
    /// <param name="i">i memory location of grid circle in grid 2D.</param>
    /// <param name="j">j memory location of grid circle in grid 2D.</param>
    private void CreateGridCircle(GridPoint gridPoint, int xOffset, int yOffset, int i, int j)
    {
        Vector2 localOffset = ComputeLocalOffset(gridPoint, xOffset, yOffset);

        // Make a data object.
        GridCircleData circleData = new GridCircleData(_id, gridPoint, localOffset);

        // Place it in the 2D array
        _grid2D[i, j] = circleData;
    }

    /// <summary>
    /// Draws the grid in the XY direction.
    /// </summary>
    protected void DrawGrid()
    {
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                int x = IndexToGridX(i);
                int y = IndexToGridY(j);
                GridPoint gridPoint = new GridPoint(x, y);
                CreateGridCircle(gridPoint, i - 2, j - 2, i, j); // Note: Changed XY offset 8/27 DY
                _size++;
            }
        }
    }

    /// <summary>
    /// Converts 2D array row index to grid point x value.
    /// </summary>
    /// <param name="i">2D array row index.</param>
    /// <returns>grid point x value.</returns>
    public int IndexToGridX(int i)
    {
        return i + _minimumBound.X;
    }

    /// <summary>
    /// Converts 2D array column index to grid point y value.
    /// </summary>
    /// <param name="j">2D array column index.</param>
    /// <returns>grid point y value.</returns>
    public int IndexToGridY(int j)
    {
        return j + _minimumBound.Y;
    }

    /// <summary>
    /// Converts grid point x value to 2D array row index.
    /// </summary>
    /// <param name="x">grid point x value.</param>
    /// <returns>2D array row index.</returns>
    public int GridXToIndex(int x)
    {
        return x - _minimumBound.X;
    }

    /// <summary>
    /// Converts grid point y value to 2D array column index.
    /// </summary>
    /// <param name="y">grid point y value.</param>
    /// <returns>2D array column index.</returns>
    public int GridYToIndex(int y)
    {
        return y - _minimumBound.Y;
    }

    /// <summary>
    /// Expands this grid north.
    /// </summary>
    public void ExpandNorth()
    {
        CopyNorth();

        // create new grid circles
        for (int i = 0; i < _length; i++)
        {
            int newJ = _width - 1;
            int xCreationOffset = i - _numWestExpansions;
            int yCreationOffset = newJ - _numSouthExpansions;
            int x = IndexToGridX(i);
            int y = IndexToGridY(newJ);
            GridPoint gridPoint = new GridPoint(x, y);
            CreateGridCircle(gridPoint, xCreationOffset - 2, yCreationOffset - 2, i, newJ);
            _size++;
        }
    }

    /// <summary>
    /// Expands this grid east.
    /// </summary>
    public void ExpandEast()
    {
        CopyEast();

        // create new grid circles
        for (int j = 0; j < _width; j++)
        {
            int newI = _length - 1;
            int xCreationOffset = newI - _numWestExpansions;
            int yCreationOffset = j - _numSouthExpansions;
            int x = IndexToGridX(newI);
            int y = IndexToGridY(j);
            GridPoint gridPoint = new GridPoint(x, y);
            CreateGridCircle(gridPoint, xCreationOffset - 2, yCreationOffset - 2, newI, j);
            _size++;
        }
    }

    /// <summary>
    /// Expands this grid south.
    /// </summary>
    public void ExpandSouth()
    {
        CopySouth();

        // create new grid circles
        for (int i = 0; i < _length; i++)
        {
            int newJ = 0;
            int xCreationOffset = i - _numWestExpansions;
            int yCreationOffset = newJ - _numSouthExpansions - 1;
            int x = IndexToGridX(i);
            int y = IndexToGridY(newJ);
            GridPoint gridPoint = new GridPoint(x, y);
            CreateGridCircle(gridPoint, xCreationOffset - 2, yCreationOffset - 2, i, newJ);
            _size++;
        }

        _numSouthExpansions++;
    }

    /// <summary>
    /// Expands this grid west.
    /// </summary>
    public void ExpandWest()
    {
        CopyWest();

        // create new grid circles
        for (int j = 0; j < _width; j++)
        {
            int newI = 0;
            int xCreationOffset = newI - _numWestExpansions - 1;
            int yCreationOffset = j - _numSouthExpansions;
            int x = IndexToGridX(newI);
            int y = IndexToGridY(j);
            GridPoint gridPoint = new GridPoint(x, y);
            CreateGridCircle(gridPoint, xCreationOffset - 2 , yCreationOffset - 2, newI, j);
            _size++;
        }

        _numWestExpansions++;
    }

    /// <summary>
    /// Helper method for expandNorth. Expands _grid2D to become large enough for a north expansion.
    /// </summary>
    protected void CopyNorth()
    {
        // increase maximum y bound
        _width++;
        _maximumBound.Y++;
        GridCircleData[,] newGrid2D = new GridCircleData[_length, _width];

        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width - 1; j++)
            {
                newGrid2D[i, j] = _grid2D[i, j];
            }
        }

        _grid2D = newGrid2D;
    }

    /// <summary>
    /// Helper method for expandEast. Expands _grid2D to become large enough for a east expansion.
    /// </summary>
    protected void CopyEast()
    {
        // increase maximum x bound
        _length++;
        _maximumBound.X++;
        GridCircleData[,] newGrid2D = new GridCircleData[_length, _width];
        
        for (int i = 0; i < _length - 1; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                newGrid2D[i, j] = _grid2D[i, j];
            }
        }

        _grid2D = newGrid2D;
    }

    /// <summary>
    /// Helper method for expandSouth. Expands _grid2D to become large enough for a south expansion.
    /// </summary>
    protected void CopySouth()
    {
        // decrease minimum y bound
        _width++;
        _minimumBound.Y--;
        GridCircleData[,] newGrid2D = new GridCircleData[_length, _width];

        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width - 1; j++)
            {
                newGrid2D[i, j + 1] = _grid2D[i, j];
            }
        }

        _grid2D = newGrid2D;
    }

    /// <summary>
    /// Helper method for expandWest. Expands _grid2D to become large enough for a west expansion.
    /// </summary>
    protected void CopyWest()
    {
        // increase minimum x bound
        _length++;
        _minimumBound.X--;
        GridCircleData[,] newGrid2D = new GridCircleData[_length, _width];

        for (int i = 0; i < _length - 1; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                newGrid2D[i + 1, j] = _grid2D[i, j];
            }
        }

        _grid2D = newGrid2D;
    }

    /// <summary>
    /// Expands the grid if the position of the grid component is on the edge of the grid.
    /// </summary>
    /// <param name="gridComponent">Grid component to check if on the edge.</param>
    public void CheckExpansion(GridCircleData gridCircleData)
    {
        int x = gridCircleData.GridPoint.X;
        int y = gridCircleData.GridPoint.Y;
        int maxX = _maximumBound.X;
        int maxY = _maximumBound.Y;
        int minX = _minimumBound.X;
        int minY = _minimumBound.Y;
        if (x == maxX)
        {
            ExpandEast();
        }
        else if (x == minX)
        {
            ExpandWest();
        }
        if (y == maxY)
        {
            ExpandNorth();
        }
        else if (y == minY)
        {
            ExpandSouth();
        }
    }

    /// <summary>
    /// Builds a list of final (world-space) matrices for each grid circle,
    /// by combining the grid's transform with each circle's local matrix.
    /// </summary>
    public List<Matrix4x4> GetCircleMatrices()
    {
        // We'll store the final transform for each circle here
        List<Matrix4x4> finalMatrices = new List<Matrix4x4>();

        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                GridCircleData circleData = _grid2D[i, j];

                // The local matrix for this circle
                Matrix4x4 localMatrix = circleData.LocalMatrix;

                // Multiply by this grid's parent transform (GridMatrix) 
                // to get the circle's final world-space transform
                Matrix4x4 finalMatrix = GridMatrix * localMatrix;

                finalMatrices.Add(finalMatrix);
            }
        }

        return finalMatrices;
    }


    /// <summary>
    /// Returns neighboring grid components of provided grid component.
    /// </summary>
    /// <param name="gridPoint">Location of grid component.</param>
    /// <returns>List of neighboring grid components.</returns>
    /// TODO: Revamp this to grid circle data if this method is useful in the future.
    public abstract List<GridComponent> GetNeighborGridComponents(GridPoint gridPoint);

    // TODO: Convert grid component parameter to grid circle data.
    public void DoAddHelix(int id, Vector3 startPoint, int length, string orientation, GridComponent gridComponent)
    {
        ICommand command = new CreateHelixCommand(id, startPoint, length, orientation, gridComponent, this);
        CommandManager.AddCommand(command);
    }

    public Helix AddHelix(int id, GridCircleData girdCircleData)
    {
        Helix helix = new Helix(id, girdCircleData);
        girdCircleData.HelixId = id;
        // gridComponent.Selected = true;
        if (s_visualMode)
        {
            s_visHelixDict.Add(id, helix);
            s_numVisHelices += 1;
        }
        else
        {
            s_helixDict.Add(id, helix);
            s_numHelices += 1;
        }
        return helix;
     }

    public void ChangeStencilView()
    {
        // TODO
        //for (int i = 0; i < _length; i++)
        //{
        //    for (int j = 0; j < _width; j++)
        //    {
        //        GameObject go = _grid2D[i, j].gameObject;
        //        go.SetActive(!s_hideStencils);
        //        _grid2D[i, j].Helix?.ChangeStencilView();
        //    }
        //}
    }

    public void Rotate(float pitch, float roll, float yaw)
    {
        // TODO: Adjust grid's transform matrix by rotating it.
        // Attach parent transforms
        TransformHandle.ShowTransform(this);
        TransformHandle.AttachChildren(new List<DNAGrid> { this });
        TransformHandle.Gizmos.transform.rotation = Quaternion.Euler(yaw, pitch, roll);
        TransformHandle.DetachChildren();

        // haven't tested yet?
        //gridStartTransform.rotation = Quaternion.Euler(roll, yaw, pitch);
        //gridStartTransform.rotation = Quaternion.Euler(yaw, roll, pitch);

        /*GameObject gridStart = Grid2D[0, 0].gameObject; // TODO: check, might need to change this
        Transform gridStartTransform = gridStart.transform;
        for (int i = 0; i < Length; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                Grid2D[i, j].gameObject.transform.parent = gridStartTransform;
                Grid2D[i, j].Helix?.SetParent(gridStartTransform);
            }
        }

        // Rotate grid
        //gridStartTransform.rotation = Utils.ToQuaternion(roll, pitch, yaw);
        //gridStartTransform.rotation = Quaternion.Euler(pitch, yaw, roll);
        gridStartTransform.rotation = Quaternion.Euler(roll, pitch, yaw);





        // Detach parent transforms
        for (int i = 0; i < gridStartTransform.childCount; i++)
        {
            Transform child = gridStartTransform.GetChild(i);
            child.SetParent(null);
        }*/
    }

    /// <summary>
    /// Shows or hides grid circles depending on parameter showCircles.
    /// </summary>
    /// <param name="showCircles">Whether or not to show grid circles</param>
    public void ToggleGridCircles(bool showCircles)
    {
        // TODO
        //for (int i = 0; i < _length; i++)
        //{
        //    for (int j = 0; j < _width; j++)
        //    {
        //        GridComponent gc = _grid2D[i, j];
        //        gc.gameObject.SetActive(showCircles);
        //    }
        //}
    }

    /// <summary>
    /// Deletes grid.
    /// </summary>
    public void DeleteGrid()
    {
        // TODO
//        // Delete Grid object
//        if (!IsEmpty())
//        {
//            Debug.Log("Cannot delete grid while strands remain");
//            return;
//        }

//        for (int i = 0; i < _length; i++)
//        {
//            for (int j = 0; j < _width; j++)
//            {
//                GridComponent gc = _grid2D[i, j];
//                gc.Helix?.DeleteHelix();
//#if UNITY_EDITOR
//                GameObject.DestroyImmediate(gc.gameObject);
//#else
//                GameObject.Destroy(gc.gameObject);
//#endif

//            }
//        }

//        s_gridDict.Remove(_id);
//        s_gridCopies[_id] -= 1;
//        if (s_gridCopies[_id] < 0)
//        {
//            s_gridCopies.Remove(_id);
//        }
    }

    /// <summary>
    /// Returns true if grid is empty (has no strands on it).
    /// </summary>
    public bool IsEmpty()
    {
        //return false;
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                GridCircleData gridCircleData = _grid2D[i, j];
                if (gridCircleData.Helix != null && !gridCircleData.Helix.IsEmpty())
                {
                    return false;
                }
            }
        }
        return true;
    }
}
