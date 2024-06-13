/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static GlobalVariables;
using static Utils;

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
    public Vector3 StartPoint { get { return _gridComponent.Position ; } }

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
    private MeshCombiner _meshCombiner;

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

    // Positions of last nucleotides in helix
    private Vector3 _lastPositionA;
    private Vector3 _lastPositionB;

    // Empty parent gameobject that contains the combined mesh of a Helix.
    private GameObject _parent;
    private MeshFilter _parentMesh;

    private List<GameObject> _helixA;
    private List<GameObject> _helixB;

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
        _lastPositionA = Vector3.zero;
        _lastPositionB = Vector3.zero;
        _helixA = new List<GameObject>();
        _helixB = new List<GameObject>();
        /*_parent = new GameObject();
        _parent.transform.position = _gridComponent.transform.position;
        _parentMesh = _parent.AddComponent<MeshFilter>();

        // Set MeshCombiner properties
        _meshCombiner = _parent.AddComponent<MeshCombiner>();
        _meshCombiner.DeactivateCombinedChildren = false;
        _meshCombiner.DestroyCombinedChildren = false;
        _meshCombiner.DeactivateCombinedChildrenMeshRenderers = false;
        _meshCombiner.CreateMultiMaterialMesh = false;
       */
        //ExtendAsync(length);
        //ChangeRendering();
    }

    /// <summary>
    /// Draws the nucleotides of the helix in background thread.
    /// </summary>
    public async Task ExtendAsync(int length)
    {
        int prevLength = _length;
        _length += length;

        for (int i = prevLength; i < _length; i++)
        {
            float angleA = (float)(i * (2 * Math.PI / NUM_BASE_PAIRS)); // rotation per bp in radians
            float angleB = (float)((i + 4.5f) * (2 * Math.PI / NUM_BASE_PAIRS)); //TODO: check this new offset
            float axisOneChangeA = (float)(RADIUS * Mathf.Cos(angleA));
            float axisTwoChangeA = (float)(RADIUS * Mathf.Sin(angleA));
            float axisOneChangeB = (float)(RADIUS * Mathf.Cos(angleB));
            float axisTwoChangeB = (float)(RADIUS * Mathf.Sin(angleB));
            _lastPositionA = StartPoint + new Vector3(axisOneChangeA, axisTwoChangeA, -i * RISE);
            _lastPositionB = StartPoint + new Vector3(axisOneChangeB, axisTwoChangeB, -i * RISE);

            GameObject sphereA = DrawPoint.MakeNucleotide(_lastPositionA, i, _id, 1);
            _nucleotidesA.Add(sphereA);
            _helixA.Add(sphereA);
            //sphereA.transform.SetParent(_parent.transform);

            GameObject sphereB = DrawPoint.MakeNucleotide(_lastPositionB, i, _id, 0);
            _nucleotidesB.Add(sphereB);
            _helixB.Add(sphereB);
            //sphereB.transform.SetParent(_parent.transform);


            // Rotate nucleotides to correct position based on grid's rotation
            sphereA.transform.RotateAround(StartPoint, Vector3.forward, _gridComponent.transform.eulerAngles.z);
            sphereA.transform.RotateAround(StartPoint, Vector3.right, _gridComponent.transform.eulerAngles.x);
            sphereA.transform.RotateAround(StartPoint, Vector3.up, _gridComponent.transform.eulerAngles.y);
            sphereB.transform.RotateAround(StartPoint, Vector3.forward, _gridComponent.transform.eulerAngles.z);
            sphereB.transform.RotateAround(StartPoint, Vector3.right, _gridComponent.transform.eulerAngles.x);
            sphereB.transform.RotateAround(StartPoint, Vector3.up, _gridComponent.transform.eulerAngles.y);
            await Task.Yield();
        }
        if (prevLength == 0)
        {
            await DrawBackbones(prevLength + 1);
        }
        else
        {
            // Needs to add backbone to connect previous set of nucleotides
            await DrawBackbones(prevLength);
        }

        /* Batches static (non-moving) gameobjects so that they are drawn together.
         * This reduces number of Draw calls and increases FPS. 
         */
        StaticBatchingUtility.Combine(_helixA.ToArray(), _helixA[0]);
        await Task.Yield();
        StaticBatchingUtility.Combine(_helixB.ToArray(), _helixB[0]);
        await Task.Yield();

        _helixA.Clear();
        _helixB.Clear();
    }

    /// <summary>
    /// Draws the backbones between the nucleotides in the helix.
    /// </summary>
    /// <param name="start">Start index of nucleotide to begin drawing backbones.</param>
    private async Task DrawBackbones(int start)
    {
        for (int i = start; i < _nucleotidesA.Count; i++)
        {
            GameObject cylinderA = DrawPoint.MakeBackbone(i - 1, _id, 1, NucleotidesA[i].transform.position, NucleotidesA[i - 1].transform.position);
            _backbonesA.Add(cylinderA);
            _helixA.Add(cylinderA);
            //cylinder.transform.SetParent(_parent.transform);

            GameObject cylinderB = DrawPoint.MakeBackbone(i - 1, _id, 0, NucleotidesB[i].transform.position, NucleotidesB[i - 1].transform.position);
            _backbonesB.Add(cylinderB);
            _helixB.Add(cylinderB);
            //cylinder.transform.SetParent(_parent.transform);
            await Task.Yield();
        }
    }


    /// <summary>
    /// Returns sublist of nucleotides and backbones from helix spiral.
    /// </summary>
    /// <param name="sIndex">Start index of sublist.</param>
    /// <param name="eIndex">End index of sublist.</param>
    /// <param name="direction">Spiral direction (determines nucleotidesA or nucleotidesB).</param>
    /// <returns>Returns sublist of nucleotides from helix spiral.</returns>
    public List<GameObject> GetHelixSub(int sIndex, int eIndex, int direction)
    {
        if (sIndex < 0 || eIndex >= _nucleotidesA.Count)
        {
            Debug.Log("Nucleotides A length: " + _nucleotidesA.Count);
            return null;
        }
        List<GameObject> temp = new List<GameObject>();
        if (direction == 0)
        {
            for (int i = sIndex; i < eIndex; i++)
            {
                temp.Add(_nucleotidesB[i]);
                temp.Add(_backbonesB[i]);
            }
            temp.Add(_nucleotidesB[eIndex]);
            return temp;
        }
        else 
        {
            for (int i = sIndex; i < eIndex; i++)
            {
                temp.Add(_nucleotidesA[i]);
                temp.Add(_backbonesA[i]);
            }
            temp.Add(_nucleotidesA[eIndex]);
            temp.Reverse();
            return temp;
        }
    }

    public GameObject GetNucleotide(int id, int direction)
    {
        // Need to prevent indexOutOfBounds
        if (id < 0 || id >= NucleotidesA.Count) { return null; }
        if (direction == 0)
        {
            return _nucleotidesB[id];
        }
        else
        {
            return _nucleotidesA[id];
        }
    }

    public GameObject GetBackbone(int id, int direction)
    {
        if (direction == 0)
        {
            return _backbonesB[id];
        }
        else
        {
            return _backbonesA[id];
        }
    }

    /// <summary>
    /// Returns nucleotide in front of head nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of the helix, 0 or 1.</param>
    /// <returns>Returns nucleotide in front of head nucleotide.</returns>
    public GameObject GetHeadNeighbor(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = _nucleotidesB.IndexOf(go);
            if (index == 0)
            {
                return null;
            }
            return _nucleotidesB[index - 1];
        }
        else
        {
            int index = _nucleotidesA.IndexOf(go);
            if (index == _nucleotidesA.Count - 1)
            {
                return null;
            }
            return _nucleotidesA[index + 1];
        }
    }

    /// <summary>
    /// Returns nucleotide behind tail nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of helix, 0 or 1.</param>
    /// <returns>Returns nucleotide behind tail nucleotide.</returns>
    public GameObject GetTailNeighbor(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = _nucleotidesB.IndexOf(go);
            if (index == _nucleotidesB.Count - 1)
            {
                return null;
            }
            return _nucleotidesB[index + 1];
        }
        else
        {
            int index = _nucleotidesA.IndexOf(go);
            if (index == 0)
            {
                return null;
            }
            return _nucleotidesA[index - 1];
        }
    }

    /// <summary>
    /// Returns backbone in front of head nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of helix, 0 or 1.</param>
    /// <returns>Returns backbone in front of head nucleotide.</returns>
    public GameObject GetHeadBackbone(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = _nucleotidesB.IndexOf(go);
            return _backbonesB[index - 1];
        }
        else
        {
            int index = _nucleotidesA.IndexOf(go);
            return _backbonesA[index];
        }
    }

    /// <summary>
    /// Returns backbone in behind tail nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of helix, 0 or 1.</param>
    /// <returns>Returns backbone in behind tail nucleotide.</returns>
    public GameObject GetTailBackbone(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = _nucleotidesB.IndexOf(go);
            return _backbonesB[index];
        }
        else
        {
            int index = _nucleotidesA.IndexOf(go);
            return _backbonesA[index - 1];
        }
    }

    public int NumModsToLeft(int id, int direction)
    {
        if (direction == 0)
        {
            int shiftCount = 0;
            for (int i = 0; i < id; i++)
            {
                var ntc = _nucleotidesB[i].GetComponent<NucleotideComponent>();
                shiftCount += ntc.Insertion;
                if (ntc.IsDeletion) { shiftCount -= 1; }
            }
            return shiftCount;
        }
        else
        {
            int shiftCount = 0;
            for (int i = 0; i < id; i++)
            {
                var ntc = _nucleotidesA[i].GetComponent<NucleotideComponent>();
                shiftCount += ntc.Insertion;
                if (ntc.IsDeletion) { shiftCount -= 1; }
            }
            return shiftCount;
        }
    }

    // Returns true if none of the helix's nucleotides are selected.
    // In other words, if there are no strands on the helix.
    public bool IsEmpty()
    {
        return IsEmpty(_nucleotidesA) && IsEmpty(_nucleotidesB) && IsEmpty(_backbonesA) && IsEmpty(_backbonesB);
    }

    // Helper method for IsEmpty().
    public bool IsEmpty(List<GameObject> lst)
    {
        foreach (GameObject nucleotide in lst)
        {
            DNAComponent dnaComponent = nucleotide.GetComponent<DNAComponent>();
            if (dnaComponent.Selected)
            {
                return false;
            }
        }
        return true;
    }

    public void ChangeStencilView()
    {
        ChangeStencilView(_nucleotidesA);
        ChangeStencilView(_nucleotidesB);
        ChangeStencilView(_backbonesA);
        ChangeStencilView(_backbonesB);
    }

    // Helper method to hide stencil.
    public void ChangeStencilView(List<GameObject> lst)
    {
        foreach (GameObject go in lst)
        {
            if (!go.GetComponent<DNAComponent>().Selected)
            {
                go.SetActive(s_hideStencils);
            }
        }
    }

    // Hides helix GameObjects in world.
    public void HideHelix()
    {
        HideObjects(_nucleotidesA);
        HideObjects(_nucleotidesB);
        HideObjects(_backbonesA);
        HideObjects(_backbonesB);
    }

    // Helper method that hides a list of GameObjects in world.
    public void HideObjects(List<GameObject> lst)
    {
        foreach (GameObject go in lst)
        {
            go.SetActive(false);
        }
    }

    /// <summary>
    /// Changes rendering of helix and its components.
    /// </summary>
    public void ChangeRendering()
    {
        for (int i = 0; i < _backbonesA.Count; i++)
        {
            _nucleotidesA[i].SetActive(s_nucleotideView);
            _backbonesA[i].SetActive(s_nucleotideView);
            _nucleotidesB[i].SetActive(s_nucleotideView);
            _backbonesB[i].SetActive(s_nucleotideView);
        }
        _nucleotidesA[_nucleotidesA.Count - 1].SetActive(s_nucleotideView);
        _nucleotidesB[_nucleotidesB.Count - 1].SetActive(s_nucleotideView);
    }

    /// <summary>
    /// Returns the helices that neighbor this helix.
    /// </summary>
    /// <returns>List of neighboring helices.</returns>
    public List<Helix> getNeighborHelices()
    {
        List<Helix> helices = new List<Helix>();
        foreach (GridComponent gridComponent in _gridComponent.getNeighborGridComponents())
        {
            Helix helix = gridComponent.Helix;
            // helix != null if there is a helix on the grid component
            if (helix != null)
            {
                helices.Add(helix);
            }
        }
        return helices;
    }

    /// <summary>
    /// Moves all GameObjects in helix.
    /// </summary>
    /// <param name="diff">Vector3 specifying how much to move the helix.</param>
    public void MoveNucleotides(Vector3 diff)
    {
        foreach (GameObject nucleotide in NucleotidesA)
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
        }
    }

    /// <summary>
    /// Deletes helix object and destroys all GameObjects.
    /// </summary>
    public void DeleteHelix()
    {
        _gridComponent.Helix = null;
        _gridComponent.Selected = false;
        s_helixDict.Remove(_id);
        foreach (GameObject nucleotide in NucleotidesA)
        {
            GameObject.Destroy(nucleotide);
        }
        foreach (GameObject nucleotide in NucleotidesB)
        {
            GameObject.Destroy(nucleotide);
        }
        foreach (GameObject nucleotide in BackbonesA)
        {
            GameObject.Destroy(nucleotide);
        }
        foreach (GameObject nucleotide in BackbonesB)
        {
            GameObject.Destroy(nucleotide);
        }
    }

    /// <summary>
    /// Sets parent transforms of all helix GameObjects to go (the transform gizmo). This helps with Grid translations and rotations.
    /// </summary>
    /// <param name="go">GameObject representing the transform gizmo.</param>
    public void SetParent(GameObject go)
    {
        //Rigidbody rb = go.GetComponent<Rigidbody>();
        foreach (GameObject nucleotide in NucleotidesA)
        {
            //nucleotide.GetComponent<FixedJoint>().connectedBody = rb;
            nucleotide.transform.SetParent(go.transform, true);
            Strand strand = Utils.GetStrand(nucleotide);
            if (strand != null)
            {
                //strand.Cone.GetComponent<FixedJoint>().connectedBody = rb;
                strand.Cone.transform.SetParent(go.transform, true);

            }
        }
        foreach (GameObject nucleotide in NucleotidesB)
        {
            nucleotide.transform.SetParent(go.transform, true);
            Strand strand = Utils.GetStrand(nucleotide);
            if (strand != null)
            { 
                strand.Cone.transform.SetParent(go.transform, true);
            }
        }
        foreach (GameObject nucleotide in BackbonesA)
        {
            nucleotide.transform.SetParent(go.transform, true);
        }
        foreach (GameObject nucleotide in BackbonesB)
        {
            nucleotide.transform.SetParent(go.transform, true);
        }
    }

    public void ResetParent()
    {
        foreach (GameObject nucleotide in NucleotidesA)
        {
            nucleotide.transform.SetParent(null);
            Strand strand = Utils.GetStrand(nucleotide);
            if (strand != null)
            {
                strand.Cone.transform.SetParent(null);
            }
        }
        foreach (GameObject nucleotide in NucleotidesB)
        {
            nucleotide.transform.SetParent(null);
            Strand strand = Utils.GetStrand(nucleotide);
            if (strand != null)
            {
                strand.Cone.transform.SetParent(null);
            }
        }
        foreach (GameObject nucleotide in BackbonesA)
        {
            nucleotide.transform.SetParent(null);
        }
        foreach (GameObject nucleotide in BackbonesB)
        {
            nucleotide.transform.SetParent(null);
        }
    }
}

