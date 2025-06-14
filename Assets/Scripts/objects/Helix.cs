/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static GlobalVariables;
using static Utils;
using Debug = UnityEngine.Debug;

/// <summary>
/// Helix object keeps track of nucleotides in the helix.
/// </summary>
public class Helix
{
    private const float ADJUSTMENT = 0.05f; // Accounts for Icosphere prefab's weird positioning 

    // Helix id.
    private int _id;
    public int Id { get { return _id; } set { _id = value; } }

    public string GridId { get { return _gridComponent.GridId; } }

    //private Vector3 _startPoint;
    public Vector3 StartPoint { get { return _gridComponent.Position; } }

    private Vector3 _endPoint;
    public Vector3 EndPoint { get { return _endPoint; } set { _endPoint = value; } }

    private string _orientation;
    public string Orientation { get { return _orientation; } }

    // Number of nucleotides in helix.
    private int _length;
    public int Length { get { return _length; } }

    // Grid Component that helix is on.
    // TODO: make all public references to _gridComponent use the property instead.
    public GridComponent _gridComponent;
    public GridComponent GridComponent { get { return _gridComponent; } }

    // Mesh Combiner component of GridComponent.
    //private MeshCombiner _meshCombiner;

    // List containing all nucleotides in spiral going in direction 1.
    private List<GameObject> _nucleotidesA;
    public List<GameObject> NucleotidesA { get { return _nucleotidesA; } }


    // List containing all backbones in spiral going in direction 1.
    private List<GameObject> _backbonesA;
    public List<GameObject> BackbonesA { get { return _backbonesA; } }

    // List containing all nucleotides in spiral going in direction 0.
    private List<GameObject> _nucleotidesB;
    public List<GameObject> NucleotidesB { get { return _nucleotidesB; } }

    // List containing all backbones in spiral going in direction 0.
    private List<GameObject> _backbonesB;
    public List<GameObject> BackbonesB { get { return _backbonesB; } }

    // List of strand ids created on helix.
    private List<int> _strandIds;
    public List<int> StrandIds { get { return _strandIds; } }

    // Empty parent gameobject that contains the combined mesh of a Helix.
    private GameObject _parent;
    private MeshFilter _parentMesh;

    private List<GameObject> _helixA;
    private List<GameObject> _helixB;

    private Vector3 _lastPositionA;
    private Vector3 _lastPositionB;

    private List<HelixComponent> _helixViewCylinders;
    //private GameObject _collider;

    // Lists to store instance matrices for GPU instancing.
    private List<Matrix4x4> nucleotideMatricesA = new List<Matrix4x4>();
    private List<Matrix4x4> nucleotideMatricesB = new List<Matrix4x4>();
    private List<Matrix4x4> backboneMatricesA = new List<Matrix4x4>();
    private List<Matrix4x4> backboneMatricesB = new List<Matrix4x4>();

    public List<Matrix4x4> NucleotideMatricesA { get { return nucleotideMatricesA; } }
    public List<Matrix4x4> NucleotideMatricesB { get { return nucleotideMatricesB; } }
    public List<Matrix4x4> BackboneMatricesA { get { return backboneMatricesA; } }
    public List<Matrix4x4> BackboneMatricesB { get { return backboneMatricesB; } }

    // List of bounding boxes for this helix
    private HelixBoundingBox _boundingBox = new HelixBoundingBox();
    public HelixBoundingBox BoundingBox { get { return _boundingBox; } }

    // Also store nucleotide positions so we can compute backbone matrices.
    private List<Vector3> nucleotidePositionsA = new List<Vector3>();
    private List<Vector3> nucleotidePositionsB = new List<Vector3>();

    private List<NucleotideData> nucleotideDataA = new List<NucleotideData>();
    private List<NucleotideData> nucleotideDataB = new List<NucleotideData>();
    public List<NucleotideData> NucleotideDataA { get { return nucleotideDataA; } }
    public List<NucleotideData> NucleotideDataB { get { return nucleotideDataB; } }

    private List<Extension> extensions = new List<Extension>();
    public List<Extension> Extensions { get { return extensions; } }

    private HelixComponent helixCollider;
    public HelixComponent HelixCollider { get { return helixCollider; } }

    private List<XoverComponent> xovers = new List<XoverComponent>();
    public List<XoverComponent> Xovers { get { return xovers; } }

    private List<Color> nucleotideColorA;
    private List<Color> nucleotideColorB;
    private List<Color> backboneColorA;
    private List<Color> backboneColorB;

    private List<Color> nucleotideHighlightA;
    private List<Color> nucleotideHighlightB;

    private bool nucleotideColorAChanged = true;
    private bool nucleotideColorBChanged = true;
    private bool backboneColorAChanged = true;
    private bool backboneColorBChanged = true;
    private bool nucleotideHighlightAChanged = true;
    private bool nucleotideHighlightBChanged = true;

    public bool NucleotideColorAChanged { set => nucleotideColorAChanged = value; }
    public bool NucleotideColorBChanged { set => nucleotideColorBChanged = value; }
    public bool BackboneColorAChanged { set => backboneColorAChanged = value; }
    public bool BackboneColorBChanged { set => backboneColorBChanged = value; }
    public bool NucleotideHighlightAChanged { set => nucleotideHighlightAChanged = value; }
    public bool NucleotideHighlightBChanged { set => nucleotideHighlightBChanged = value; }

    private bool isHelixView = false;
    public bool IsHelixView { get => isHelixView; }

    // Helix constructor.
    public Helix(int id, string orientation, int length, GridComponent gridComponent)
    {
        _id = id;
        _length = 0;
        _orientation = orientation;
        _gridComponent = gridComponent;
        _nucleotidesA = new List<GameObject>();
        _backbonesA = new List<GameObject>();
        _nucleotidesB = new List<GameObject>();
        _backbonesB = new List<GameObject>();
        _strandIds = new List<int>();
        _helixA = new List<GameObject>();
        _helixB = new List<GameObject>();
        _helixViewCylinders = new List<HelixComponent>();
        _boundingBox.Helix = this;
    }

    /// <summary>
    /// Extends the DNA helix by the specified number of base pairs.
    /// Instead of creating GameObjects, we compute and store transformation matrices
    /// for both the nucleotides and backbones.
    /// </summary>
    public void Extend(int length)
    {
        int prevLength = _length;
        _length += length;

        for (int i = prevLength; i < _length; i++)
        {
            // Calculate the positions for the two nucleotides at index i.
            CalculateNextNucleotidePositions(i, out Vector3 posA, out Vector3 posB);

            // Update the bounding box for this helix.
            _boundingBox.Extend(posA);
            _boundingBox.Extend(posB);

            // Save positions (for backbone computations)
            nucleotidePositionsA.Add(posA);
            nucleotidePositionsB.Add(posB);

            // Create TRS matrices for the nucleotides.
            // (You can add rotation/scale as needed; here we use identity rotation and uniform scale.)
            Matrix4x4 matrixA = Matrix4x4.TRS(posA, Quaternion.identity, new Vector3(NUCL_RAD, NUCL_RAD, NUCL_RAD));
            Matrix4x4 matrixB = Matrix4x4.TRS(posB, Quaternion.identity, new Vector3(NUCL_RAD, NUCL_RAD, NUCL_RAD));
            nucleotideMatricesA.Add(matrixA);
            nucleotideMatricesB.Add(matrixB);

            // Create and store the data.
            NucleotideData nuclA = new NucleotideData(nucleotideDataA.Count, _id, 1);
            NucleotideData nuclB = new NucleotideData(nucleotideDataB.Count, _id, 0);

            nucleotideDataA.Add(nuclA);
            nucleotideDataB.Add(nuclB);

            // For nucleotides beyond the first, compute backbone matrices connecting the previous nucleotide to the current one.
            if (i > 0)
            {
                Vector3 prevPosA = nucleotidePositionsA[i - 1];
                Matrix4x4 backboneMatrixA = GetBackboneMatrix(prevPosA, posA, fiveToThree: true);
                backboneMatricesA.Add(backboneMatrixA);

                Vector3 prevPosB = nucleotidePositionsB[i - 1];
                Matrix4x4 backboneMatrixB = GetBackboneMatrix(prevPosB, posB, fiveToThree: false);
                backboneMatricesB.Add(backboneMatrixB);
            }
        }

        // Set color flags to true since we now need to update these lists to new helix length
        nucleotideColorAChanged = true;
        nucleotideColorBChanged = true;
        backboneColorAChanged = true;
        backboneColorBChanged = true;
        nucleotideHighlightAChanged = true;
        nucleotideHighlightBChanged = true;
    }


    /// <summary>
    /// Calculates the native positions of the nucleotides at the given index i.
    /// </summary>
    public void CalculateNextNucleotidePositions(int i, out Vector3 posA, out Vector3 posB)
    {
        float angleA = (float)(i * (2 * Math.PI / NUM_BASE_PAIRS)); // rotation per bp in radians
        float angleB = (float)((i + MINOR_GROOVE_OFFSET) * (2 * Math.PI / NUM_BASE_PAIRS)); // 150° minor groove offset
        float axisOneChangeA = (float)(RADIUS * Mathf.Cos(angleA));
        float axisTwoChangeA = (float)(RADIUS * Mathf.Sin(angleA));
        float axisOneChangeB = (float)(RADIUS * Mathf.Cos(angleB));
        float axisTwoChangeB = (float)(RADIUS * Mathf.Sin(angleB));

        // Backbone repulsion sites for each nucleotide, unrotated.
        Vector3 positionA = StartPoint + new Vector3(axisOneChangeA, axisTwoChangeA, i * RISE);
        Vector3 positionB = StartPoint + new Vector3(axisOneChangeB, axisTwoChangeB, i * RISE);

        // Create a quaternion from the Euler angles of the grid component's transform
        Quaternion rotation = Quaternion.Euler(_gridComponent.transform.eulerAngles);

        // Apply the rotation to positionA and positionB relative to the start point. 
        Vector3 rotatedPositionA = rotation * (positionA - StartPoint) + StartPoint;
        Vector3 rotatedPositionB = rotation * (positionB - StartPoint) + StartPoint;

        posA = rotatedPositionA;
        posB = rotatedPositionB;
    }

    public List<NucleotideData> GetSubHelix(int sIndex, int eIndex, int direction)
    {
        if (sIndex < 0 || eIndex >= nucleotideDataA.Count)
        {
            Debug.Log("Nucleotides A length: " + nucleotideDataA.Count);
            return null;
        }
        List<NucleotideData> temp = new List<NucleotideData>();
        for (int i = sIndex; i <= eIndex; i++)
        {
            temp.Add(GetNucleotideData(i, direction));
        }

        if (direction == 1) { temp.Reverse(); }
        return temp;
    }

    public Matrix4x4 GetNucleotideMesh(int id, int direction)
    {
        // Need to prevent indexOutOfBounds
        if (id < 0 || id >= nucleotideMatricesA.Count) { throw new IndexOutOfRangeException(); }
        if (direction == 0)
        {
            return nucleotideMatricesB[id];
        }
        else
        {
            return nucleotideMatricesA[id];
        }
    }

    public Matrix4x4 GetBackboneMesh(int id, int direction)
    {
        if (id < 0 || id >= backboneMatricesA.Count) { throw new IndexOutOfRangeException(); }
        if (direction == 0)
        {
            return backboneMatricesB[id];
        }
        else
        {
            return backboneMatricesB[id];
        }
    }

    public NucleotideData GetNucleotideData(int id, int direction)
    {
        // Need to prevent indexOutOfBounds
        if (id < 0 || id >= nucleotideDataA.Count) { throw new IndexOutOfRangeException(); }
        if (direction == 0)
        {
            return nucleotideDataB[id];
        }
        else
        {
            return nucleotideDataA[id];
        }
    }

    public NucleotideData GetExtensionNucleotideData(int id, int extensionId)
    {
        if (extensionId < 0 || extensionId >= extensions.Count)
        {
            throw new IndexOutOfRangeException($"Extension ID {extensionId} is out of range. Valid range is 0 to {extensions.Count - 1}.");
        }

        Extension ext = extensions[extensionId];
        if (id < 0 || id >= ext.GetLength())
        {
            throw new IndexOutOfRangeException($"Nucleotide ID {id} is out of range for extension {extensionId}. Valid range is 0 to {ext.GetLength() - 1}.");
        }
        return ext.GetNucleotideData(id);
    } 

    public Color GetNucleotideColor(int id, int direction)
    {
        NucleotideData nd = GetNucleotideData(id, direction);
        return nd.Color;
    }

    public Color GetNucleotideHighlight(int id, int direction)
    {
        NucleotideData nd = GetNucleotideData(id, direction);
        return nd.Highlight;
    }

    public Color GetBackboneColor(int id, int direction)
    {
        NucleotideData nd1 = GetNucleotideData(id, direction);
        NucleotideData nd2 = GetNucleotideData(id + 1, direction);
        int domainId1 = nd1.DomainIdx;
        int domainId2 = nd2.DomainIdx;
        int strandId1 = nd1.StrandId;
        int strandId2 = nd2.StrandId;

        if (domainId1 == domainId2 && strandId1 == strandId2)
        {
            return nd1.Color;
        }
        return Color.white;
    }

    /// <summary>
    /// Returns list of nucleotide colors in helix.
    /// </summary>
    /// <param name="direction">Direction 1 corresponds to nucleotideA and direction 0 corresponds to nucleotideB.</param>
    /// <returns></returns>
    public List<Color> GetNucleotideColors(int direction)
    {
        if (direction == 0)
        {
            if (nucleotideColorBChanged)
            {
                nucleotideColorB = new List<Color>();
                for (int i = 0; i < nucleotideMatricesB.Count; i++)
                {
                    nucleotideColorB.Add(GetNucleotideColor(i, direction));
                }
                nucleotideColorBChanged = false;
            }
            return nucleotideColorB;
        }
        else
        {
            if (nucleotideColorAChanged)
            {
                nucleotideColorA = new List<Color>();
                for (int i = 0; i < nucleotideMatricesA.Count; i++)
                {
                    nucleotideColorA.Add(GetNucleotideColor(i, direction));
                }
                nucleotideColorAChanged = false;
            }
            return nucleotideColorA;
        }
    }

    /// <summary>
    /// Returns list of nucleotide colors in helix.
    /// </summary>
    /// <param name="direction">Direction 1 corresponds to nucleotideA and direction 0 corresponds to nucleotideB.</param>
    /// <returns></returns>
    public List<Color> GetNucleotideHighlights(int direction)
    {
        if (direction == 0)
        {
            if (nucleotideHighlightBChanged)
            {
                nucleotideHighlightB = new List<Color>();
                for (int i = 0; i < nucleotideMatricesB.Count; i++)
                {
                    nucleotideHighlightB.Add(GetNucleotideHighlight(i, direction));
                }
                nucleotideHighlightBChanged = false;
            }
            return nucleotideHighlightB;
        }
        else
        {
            if (nucleotideHighlightAChanged)
            {
                nucleotideHighlightA = new List<Color>();
                for (int i = 0; i < nucleotideMatricesA.Count; i++)
                {
                    nucleotideHighlightA.Add(GetNucleotideHighlight(i, direction));
                }
                nucleotideHighlightAChanged = false;
            }
            return nucleotideHighlightA;
        }
    }


    /// <summary>
    /// Returns list of backbone colors in helix.
    /// </summary>
    /// <param name="direction">Direction 1 corresponds to nucleotideA and direction 0 corresponds to nucleotideB.</param>
    /// <returns></returns>
    public List<Color> GetBackboneColors(int direction)
    {
        if (direction == 0)
        {
            if (backboneColorBChanged)
            {
                backboneColorB = new List<Color>();
                for (int i = 0; i < backboneMatricesB.Count; i++)
                {
                    backboneColorB.Add(GetBackboneColor(i, direction));
                }
                backboneColorBChanged = false;
            }
            return backboneColorB;
        }
        else
        {
            if (backboneColorAChanged)
            {
                backboneColorA = new List<Color>();
                for (int i = 0; i < backboneMatricesA.Count; i++)
                {
                    backboneColorA.Add(GetBackboneColor(i, direction));
                }
                backboneColorAChanged = false;
            }
            return backboneColorA;
        }
    }

    public void UpdatePosition(int id, int direction, Vector3 newPos)
    {
        if (direction == 0)
        {
            Matrix4x4 mat = nucleotideMatricesB[id];
            mat.SetColumn(3, new Vector4(newPos.x, newPos.y, newPos.z, 1f));
            nucleotideMatricesB[id] = mat;
        }
        else
        {
            Matrix4x4 mat = nucleotideMatricesA[id];
            mat.SetColumn(3, new Vector4(newPos.x, newPos.y, newPos.z, 1f));
            nucleotideMatricesA[id] = mat;
        }
    }

    /// <summary>
    /// Updates transform of XoverComponent attached to this helix.
    /// </summary>
    public void UpdateXovers()
    {
        // If a xover is on a deleted strand, we remove it from the list.
        xovers.RemoveAll(xover => xover == null);

        foreach (XoverComponent xover in xovers)
        {
            xover.UpdateXover();
        }

        // Debug.Log("Updating xovers");
    }

    /// <summary>
    /// Adds xover to xovers list
    /// </summary>
    public void AddXover(XoverComponent xover)
    {
        xovers.Add(xover);
    }

    public void RemoveXover(XoverComponent xover)
    {
        xovers.Remove(xover);
    }

    /// <summary>
    /// Returns nucleotide in front of head nucleotide.
    /// </summary>
    public NucleotideData GetHeadNeighbor(NucleotideData nd, int direction)
    {
        int index = nd.Id;

        // NOTE: This direction is correct - the old code had a direction mismatch with scadnano.
        if (direction == 1)
        {
            if (index == 0)
            {
                return null;
            }
            return nucleotideDataA[index - 1];
        }
        else
        {
            if (index == nucleotideDataA.Count - 1)
            {
                return null;
            }
            return nucleotideDataB[index + 1];
        }
    }

    /// <summary>
    /// Returns nucleotide behind tail nucleotide.
    /// </summary>
    public NucleotideData GetTailNeighbor(NucleotideData nd, int direction)
    {
        int index = nd.Id;

        // NOTE: This direction is correct - the old code had a direction mismatch with scadnano.
        if (direction == 1)
        {
            if (index == nucleotideDataB.Count - 1)
            {
                return null;
            }
            return nucleotideDataA[index + 1];
        }
        else
        {
            if (index == 0)
            {
                return null;
            }
            return nucleotideDataB[index - 1];
        }
    }

    public void ToHelixView()
    {
        if (isHelixView) return;

        isHelixView = true;

        int startIdx = -1;

        // type stores what type of strand we are currently iterating over
        // -1 indicates an empty section
        // 0 indicates single strand section
        // 1 indicates double strand section
        int type = -1;
        bool singleStrandRegion = false;

        for (int i = 0; i < nucleotideDataA.Count; i++)
        {
            NucleotideData nuclA = nucleotideDataA[i];
            NucleotideData nuclB = nucleotideDataB[i];

            if ((nuclA.IsSelected() && !nuclB.IsSelected()) || (!nuclA.IsSelected() && nuclB.IsSelected()))
            {
                if (type == 1)
                {
                    CreateCylinder(startIdx, i, singleStrandRegion);
                    startIdx = -1;
                }
                if (startIdx == -1)
                {
                    startIdx = i;
                    type = 0;
                    singleStrandRegion = true;
                }
            }
            if (nuclA.IsSelected() && nuclB.IsSelected())
            {
                if (type == 0)
                {
                    CreateCylinder(startIdx, i, singleStrandRegion);
                    startIdx = -1;
                }
                if (startIdx == -1)
                {
                    startIdx = i;
                    type = 1;
                    singleStrandRegion = false;
                }
            }
            if (!nuclA.IsSelected() && !nuclB.IsSelected())
            {
                if (type == 0 || type == 1)
                {
                    CreateCylinder(startIdx, i, singleStrandRegion);
                    startIdx = -1;
                }
                type = -1;
            }
        }

        // Check if we want to hide xovers - only hide if helices of both endpoints are in helix view.
        foreach (XoverComponent xover in xovers)
        {
            NucleotideData prevNucl = xover.PrevNucl;
            NucleotideData nextNucl = xover.NextNucl;

            if (prevNucl.GetHelix().IsHelixView && nextNucl.GetHelix().IsHelixView)
            {
                xover.gameObject.SetActive(false);
            }
        }
    }

    public void ToNucleotideView()
    {
        if (!isHelixView) return;

        DestroyCylinders();
        foreach (XoverComponent xover in xovers)
        {
            xover.gameObject.SetActive(true);
        }
        isHelixView = false;
    }

    /// <summary>
    /// Private helper function to figure out what color to make the Helix cylinder
    /// </summary>
    private void CreateCylinder(int startIdx, int endIdx, bool singleStrandRegion) {
        if (startIdx == -1)
        {
            return;
        }
        Vector3 startPos = _gridComponent.transform.position + (RISE * startIdx * _gridComponent.transform.forward);
        Vector3 endPos = _gridComponent.transform.position + (endIdx * RISE * _gridComponent.transform.forward);
       
        Color color;
        if (singleStrandRegion)
        {
            ColorUtility.TryParseHtmlString("#C75B7A", out color);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#7FA1C3", out color);
        }
        _helixViewCylinders.Add(DrawPoint.MakeHelixCylinder(this, startPos, endPos, color));
    }

    /// <summary>
    /// Destroys cylinders representing Helix in Helix view.
    /// </summary>
    public void DestroyCylinders()
    {
        if (_helixViewCylinders.Count == 0)
        {
            return;
        }
        foreach (HelixComponent cylinder in _helixViewCylinders)
        {
            if (cylinder) GameObject.Destroy(cylinder.gameObject);
        }

        _helixViewCylinders.Clear();
    }

    /// <summary>
    /// Deletes helix object and destroys all GameObjects.
    /// </summary>
    public void DeleteHelix()
    {
        _gridComponent.Helix = null;
        _gridComponent.Selected = false;
        s_helixDict.Remove(_id);
        foreach (HelixComponent hc in _helixViewCylinders)
        {
            GameObject.Destroy(hc.gameObject);
        }
        foreach (XoverComponent xover in xovers)
        {
            DrawCrossover.DeleteXover(xover);
        }
    }

    /// <summary>
    /// Moves all GameObjects in helix.
    /// </summary>
    /// <param name="diff">Vector3 specifying how much to move the helix.</param>
    public void MoveNucleotides(Vector3 diff)
    {
        /*foreach (GameObject nucleotide in NucleotidesA)
        {
            nucleotide.transform.position += diff;
            Strand strand = Utils.GetStrand(nucleotide);
            if (strand != null && strand.Head == nucleotide)
            {
                strand.SetCone();
            }
        }
        foreach (GameObject nucleotide in NucleotidesB)
        {
            nucleotide.transform.position += diff;
           Strand strand = Utils.GetStrand(nucleotide);
            if (strand != null && strand.Head == nucleotide)
            {
                strand.SetCone();
            }
        }
        foreach (GameObject backbone in BackbonesA)
        {
            backbone.transform.position += diff;
        }
        foreach (GameObject backbone in BackbonesB)
        {
            backbone.transform.position += diff;
        }*/
    }

    /// <summary>
    /// Reflect entire helix vertically across y-coordinate.
    /// </summary>
    public void ReflectVectical(float distY)
    {
        ReflectVectical(_nucleotidesA, distY);
        ReflectVectical(_nucleotidesB, distY);
        ReflectVectical(_backbonesA, distY);
        ReflectVectical(_backbonesB, distY);
    }

    /// <summary>
    /// Helper function to reflect a list of gameobjects across y-coordinate
    /// </summary>
    private void ReflectVectical(List<GameObject> gos, float distY)
    {
        foreach (GameObject go in gos)
        {
            Transform transform = go.transform;
            transform.position = new Vector3(transform.position.x, transform.position.y - distY, transform.position.z);
        }
    }

    /// <summary>
    /// Reflect entire helix vertically across y-coordinate.
    /// </summary>
    public void ReflectHorizontal(float distX)
    {
        ReflectHorizontal(_nucleotidesA, distX);
        ReflectHorizontal(_nucleotidesB, distX);
        ReflectHorizontal(_backbonesA, distX);
        ReflectHorizontal(_backbonesB, distX);
    }


    /// <summary>
    /// Helper function to reflect a list of gameobjects across y-coordinate
    /// </summary>
    private void ReflectHorizontal(List<GameObject> gos, float distX)
    {
        foreach (GameObject go in gos)
        {
            Transform transform = go.transform;
            transform.position = new Vector3(transform.position.x - distX, transform.position.y, transform.position.z);
        }
    }

    public void ReflectVerticalLocal(Vector3 displacement)
    {
        ReflectLocal(_nucleotidesA, displacement);
        ReflectLocal(_nucleotidesB, displacement);
        ReflectLocal(_backbonesA, displacement);
        ReflectLocal(_backbonesB, displacement);
    }

    public void ReflectHorizontalLocal(Vector3 displacement)
    {
        ReflectLocal(_nucleotidesA, displacement);
        ReflectLocal(_nucleotidesB, displacement);
        ReflectLocal(_backbonesA, displacement);
        ReflectLocal(_backbonesB, displacement);
    }

    private void ReflectLocal(List<GameObject> gos, Vector3 displacement)
    {
        foreach (GameObject go in gos)
        {
            Transform transform = go.transform;
            Vector3 objectPosition = transform.position;

            // Calculate the new world position by adding the reflected vector to the reflection point
            transform.position = objectPosition + displacement;
        }
    }

    public DNAGrid GetGrid()
    {
        return _gridComponent.Grid;
    }

    public Matrix4x4 GetCurrentOffset()
    {
        return GetGrid().CurrTransformOffset;
    }

    public Matrix4x4 GetOldOffset()
    {
        return GetGrid().OldTransformOffset;
    }

    public void SetCylinderParents(Transform parent)
    {
        foreach (HelixComponent cylinder in _helixViewCylinders)
        {
            cylinder.transform.SetParent(parent, true);
        }
    }
}

