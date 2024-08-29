/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    Stopwatch sw = new Stopwatch();

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
    }

    /// <summary>
    /// Draws the nucleotides of the helix in background thread.
    /// </summary>
    public async Task ExtendAsync(int length, bool hideNucleotides = false)
    {
        int prevLength = _length;
        _length += length;

        // Draw double helix
        // First check if ObjectPool has enough GameObjects to use (length is doubled to account for double Helix).
        // If not, generate them async.
        int count64 = length / 64;
        if (ObjectPoolManager.Instance.CanGetNucleotides(2 * length) && ObjectPoolManager.Instance.CanGetBackbones(2 * (length - 1)))
        {
            _nucleotidesA.AddRange(ObjectPoolManager.Instance.GetNucleotides(length));
            await Task.Yield();
            _nucleotidesB.AddRange(ObjectPoolManager.Instance.GetNucleotides(length));
            await Task.Yield();

            _backbonesA.AddRange(ObjectPoolManager.Instance.GetBackbones(length - 1));
            await Task.Yield();
            _backbonesB.AddRange(ObjectPoolManager.Instance.GetBackbones(length - 1));
            await Task.Yield();

            //await GenerateGameObjects(length, hideNucleotides);
            for (int i = prevLength; i < _length; i++)
            {
                //sw.Start();
                float angleA = (float)(i * (2 * Math.PI / NUM_BASE_PAIRS)); // rotation per bp in radians
                float angleB = (float)((i + 4.5f) * (2 * Math.PI / NUM_BASE_PAIRS)); //TODO: check this new offset
                float axisOneChangeA = (float)(RADIUS * Mathf.Cos(angleA));
                float axisTwoChangeA = (float)(RADIUS * Mathf.Sin(angleA));
                float axisOneChangeB = (float)(RADIUS * Mathf.Cos(angleB));
                float axisTwoChangeB = (float)(RADIUS * Mathf.Sin(angleB));
                _lastPositionA = StartPoint + new Vector3(axisOneChangeA, axisTwoChangeA, -i * RISE);
                _lastPositionB = StartPoint + new Vector3(axisOneChangeB, axisTwoChangeB, -i * RISE);

                GameObject sphereA = _nucleotidesA[i];
                GameObject sphereB = _nucleotidesB[i];
                DrawPoint.SetNucleotide(sphereA, _lastPositionA, i, _id, 1, hideNucleotides);
                DrawPoint.SetNucleotide(sphereB, _lastPositionB, i, _id, 0, hideNucleotides);

                _helixA.Add(sphereA);
                _helixB.Add(sphereB);

                sphereA.transform.RotateAround(StartPoint, Vector3.forward, _gridComponent.transform.eulerAngles.z);
                sphereA.transform.RotateAround(StartPoint, Vector3.right, _gridComponent.transform.eulerAngles.x);
                sphereA.transform.RotateAround(StartPoint, Vector3.up, _gridComponent.transform.eulerAngles.y);
                sphereB.transform.RotateAround(StartPoint, Vector3.forward, _gridComponent.transform.eulerAngles.z);
                sphereB.transform.RotateAround(StartPoint, Vector3.right, _gridComponent.transform.eulerAngles.x);
                sphereB.transform.RotateAround(StartPoint, Vector3.up, _gridComponent.transform.eulerAngles.y);


                // Draw backbones
                if (i > 0)
                {
                    GameObject cylinderA = _backbonesA[i - 1];
                    DrawPoint.SetBackbone(cylinderA, i - 1, _id, 1, _nucleotidesA[i].transform.position, _nucleotidesA[i - 1].transform.position, hideNucleotides);
                    _helixA.Add(cylinderA);

                    GameObject cylinderB = _backbonesB[i - 1];
                    DrawPoint.SetBackbone(cylinderB, i - 1, _id, 0, _nucleotidesB[i].transform.position, _nucleotidesB[i - 1].transform.position, hideNucleotides);
                    _helixB.Add(cylinderB);
                }
            }
        }
        else
        {
            //await GenerateGameObjects(length, hideNucleotides);
            for (int i = prevLength; i < _length; i++)
            {
                //sw.Start();
                float angleA = (float)(i * (2 * Math.PI / NUM_BASE_PAIRS)); // rotation per bp in radians
                float angleB = (float)((i + 4.5f) * (2 * Math.PI / NUM_BASE_PAIRS)); //TODO: check this new offset
                float axisOneChangeA = (float)(RADIUS * Mathf.Cos(angleA));
                float axisTwoChangeA = (float)(RADIUS * Mathf.Sin(angleA));
                float axisOneChangeB = (float)(RADIUS * Mathf.Cos(angleB));
                float axisTwoChangeB = (float)(RADIUS * Mathf.Sin(angleB));
                _lastPositionA = StartPoint + new Vector3(axisOneChangeA, axisTwoChangeA, -i * RISE);
                _lastPositionB = StartPoint + new Vector3(axisOneChangeB, axisTwoChangeB, -i * RISE);

                GameObject sphereA = DrawPoint.MakeNucleotide(_lastPositionA, i, _id, 1, hideNucleotides);
                GameObject sphereB = DrawPoint.MakeNucleotide(_lastPositionB, i, _id, 0, hideNucleotides);
                _nucleotidesA.Add(sphereA);
                _nucleotidesB.Add(sphereB);

                _helixA.Add(sphereA);
                _helixB.Add(sphereB);

                sphereA.transform.RotateAround(StartPoint, Vector3.forward, _gridComponent.transform.eulerAngles.z);
                sphereA.transform.RotateAround(StartPoint, Vector3.right, _gridComponent.transform.eulerAngles.x);
                sphereA.transform.RotateAround(StartPoint, Vector3.up, _gridComponent.transform.eulerAngles.y);
                sphereB.transform.RotateAround(StartPoint, Vector3.forward, _gridComponent.transform.eulerAngles.z);
                sphereB.transform.RotateAround(StartPoint, Vector3.right, _gridComponent.transform.eulerAngles.x);
                sphereB.transform.RotateAround(StartPoint, Vector3.up, _gridComponent.transform.eulerAngles.y);


                // Draw backbones
                if (i > 0)
                {
                    GameObject cylinderA = DrawPoint.MakeBackbone(i - 1, _id, 1, _nucleotidesA[i].transform.position, _nucleotidesA[i - 1].transform.position, hideNucleotides);
                    _helixA.Add(cylinderA);
                    _backbonesA.Add(cylinderA);

                    GameObject cylinderB = DrawPoint.MakeBackbone(i - 1, _id, 0, _nucleotidesB[i].transform.position, _nucleotidesB[i - 1].transform.position, hideNucleotides);
                    _helixB.Add(cylinderB);
                    _backbonesB.Add(cylinderB);
                }

                if (i % 6 == 0)
                {
                    await Task.Yield();
                }
            }

        }

        /* Batches static (non-moving) gameobjects so that they are drawn together.
         * This reduces number of Draw calls and increases FPS. 
         */

        StaticBatchingUtility.Combine(_helixA.ToArray(), _helixA[0]);
        StaticBatchingUtility.Combine(_helixB.ToArray(), _helixB[0]);
        _helixA.Clear();
        _helixB.Clear();

        await Task.Yield();
    }

    /*public async Task ExtendAsync(int length, bool hideNucleotides = false)
    {
        int prevLength = _length;
        _length += length;
        float intialPositionTotal = 0f;
        float instantiationTotal = 0f;
        float rotationTotal = 0f;
        float staticBatchingTotal = 0f;
        float clearingHelixTotal = 0f;
        sw.Start();

        for (int i = prevLength; i < _length; i++)
        {
            //sw.Start();
            float angleA = (float)(i * (2 * Math.PI / NUM_BASE_PAIRS)); // rotation per bp in radians
            float angleB = (float)((i + 4.5f) * (2 * Math.PI / NUM_BASE_PAIRS)); //TODO: check this new offset
            float axisOneChangeA = (float)(RADIUS * Mathf.Cos(angleA));
            float axisTwoChangeA = (float)(RADIUS * Mathf.Sin(angleA));
            float axisOneChangeB = (float)(RADIUS * Mathf.Cos(angleB));
            float axisTwoChangeB = (float)(RADIUS * Mathf.Sin(angleB));
            _lastPositionA = StartPoint + new Vector3(axisOneChangeA, axisTwoChangeA, -i * RISE);
            _lastPositionB = StartPoint + new Vector3(axisOneChangeB, axisTwoChangeB, -i * RISE);
            //sw.Stop();
            //intialPositionTotal += sw.ElapsedMilliseconds;

            //sw.Reset();
            //sw.Start();
            GameObject sphereA = DrawPoint.MakeNucleotide(_lastPositionA, i, _id, 1, hideNucleotides);
            _nucleotidesA.Add(sphereA);
            _helixA.Add(sphereA);
            //sphereA.transform.SetParent(_parent.transform);

            GameObject sphereB = DrawPoint.MakeNucleotide(_lastPositionB, i, _id, 0, hideNucleotides);
            _nucleotidesB.Add(sphereB);
            _helixB.Add(sphereB);
            //sphereB.transform.SetParent(_parent.transform);
            //sw.Stop();
            //instantiationTotal += sw.ElapsedMilliseconds;

            //sw.Reset();
            //sw.Start();
            // Rotate nucleotides to correct position based on grid's rotation
            sphereA.transform.RotateAround(StartPoint, Vector3.forward, _gridComponent.transform.eulerAngles.z);
            sphereA.transform.RotateAround(StartPoint, Vector3.right, _gridComponent.transform.eulerAngles.x);
            sphereA.transform.RotateAround(StartPoint, Vector3.up, _gridComponent.transform.eulerAngles.y);
            sphereB.transform.RotateAround(StartPoint, Vector3.forward, _gridComponent.transform.eulerAngles.z);
            sphereB.transform.RotateAround(StartPoint, Vector3.right, _gridComponent.transform.eulerAngles.x);
            sphereB.transform.RotateAround(StartPoint, Vector3.up, _gridComponent.transform.eulerAngles.y);
            //sw.Stop();
            //rotationTotal += sw.ElapsedMilliseconds;


            // Draw backbones
            if (_nucleotidesA.Count > 1)
            {
                GameObject cylinderA = DrawPoint.MakeBackbone(i - 1, _id, 1, _nucleotidesA[i].transform.position, _nucleotidesA[i - 1].transform.position, hideNucleotides);
                _backbonesA.Add(cylinderA);
                _helixA.Add(cylinderA);

                GameObject cylinderB = DrawPoint.MakeBackbone(i - 1, _id, 0, _nucleotidesB[i].transform.position, _nucleotidesB[i - 1].transform.position, hideNucleotides);
                _backbonesB.Add(cylinderB);
                _helixB.Add(cylinderB);
            }
            //await Task.Yield();

            if (i % 2 == 0)
            {
                await Task.Yield();
            }
        }
        if (prevLength == 0)
        {
            await DrawBackbones(prevLength + 1, hideNucleotides);
        }
        else
        {
            // Needs to add backbone to connect previous set of nucleotides
            await DrawBackbones(prevLength, hideNucleotides);
        }

        *//* Batches static (non-moving) gameobjects so that they are drawn together.
         * This reduces number of Draw calls and increases FPS. 
         *//*
        //sw.Reset();
        //sw.Start();
        StaticBatchingUtility.Combine(_helixA.ToArray(), _helixA[0]);
        //sw.Stop();
        //Debug.Log(string.Format("Static batching helixA took {0}ms to complete", sw.ElapsedMilliseconds));
        //staticBatchingTotal += sw.ElapsedMilliseconds * 2;
        //await Task.Yield();
        StaticBatchingUtility.Combine(_helixB.ToArray(), _helixB[0]);
        await Task.Yield();

        //sw.Reset();
        //sw.Start();
        _helixA.Clear();
        _helixB.Clear();
        sw.Stop();
        //Debug.Log(string.Format("Clearing both helices took {0}ms to complete", sw.ElapsedMilliseconds));
        //clearingHelixTotal += sw.ElapsedMilliseconds;
        //Debug.Log(string.Format("Total time for helix length 64 creation: ~{0}", sw.ElapsedMilliseconds));

        *//*Debug.Log(string.Format("Total time for helix length 64 creation: ~{0}", intialPositionTotal + instantiationTotal + rotationTotal + staticBatchingTotal + clearingHelixTotal));
        Debug.Log(string.Format("Total time for intial nucleotide position calcution: ~{0}", intialPositionTotal));
        Debug.Log(string.Format("Total time for instiating nucleotides: ~{0}", instantiationTotal));
        Debug.Log(string.Format("Total time for rotating nucleotide calcution: ~{0}", rotationTotal));
        Debug.Log(string.Format("Total time for static batching: ~{0}", staticBatchingTotal));
        Debug.Log(string.Format("Total time for clearing helices: ~{0}", clearingHelixTotal));
        Debug.Log(string.Format("There are {0} helices in the scene", s_numHelices));*//*
    }*/

    /*private async Task GenerateGameObjects(int length, bool hideGameObjects)
    {
        for (int i = 0; i < length; i++)
        {
            _nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_1, hideGameObjects));
            _nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_1, hideGameObjects));
            if (i > 0)
            {
                _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
                _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
            }
            if (i % 4 == 0)
            {
                await Task.Yield();
            }
        }
    }*/

    private async Task GenerateGameObjects(int length, bool hideGameObjects)
    {
        //int num64nt = length / 64;
        //length %= 64;
        //int num32nt = length / 32;
        //length %= 32;
        int num16nt = length / 16;
        length %= 16; 
        int num8nt = length / 8;
        length %= 8;
        int num4nt = length / 4;
        length %= 4;
        int num2nt = length / 2;
        length %= 2;
        int num1nt = length / 1;

        //int numConnectingBackbones = num64nt + num32nt + num16nt + num8nt + num4nt + num2nt + num1nt - 1;
        int numConnectingBackbones = num16nt + num8nt + num4nt + num2nt + num1nt - 1;


        /*for (int i = 0; i < num64nt; i++)
        {
            _nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_64, hideGameObjects));
            await Task.Yield();

            _nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_64, hideGameObjects));
            await Task.Yield();

            _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_63, hideGameObjects));
            await Task.Yield();

            _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_63, hideGameObjects));
            await Task.Yield();

        }
        //await Task.Yield();
            for (int i = 0; i < num32nt; i++)
            {
                _nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_32, hideGameObjects));
                await Task.Yield();

                _nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_32, hideGameObjects));
                await Task.Yield();

                _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_31, hideGameObjects));
                await Task.Yield();

                _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_31, hideGameObjects));
                await Task.Yield();

            }*/

        for (int i = 0; i < num16nt; i++)
        {
            /*_nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_16, hideGameObjects));
            await Task.Yield();

            _nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_16, hideGameObjects));
            await Task.Yield();*/

            _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_15, hideGameObjects)); 
            await Task.Yield();

            _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_15, hideGameObjects));
            await Task.Yield();
        }


        for (int i = 0; i < num8nt; i++)
        {
            //_nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_8, hideGameObjects));
            //_nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_8, hideGameObjects));
            _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_7, hideGameObjects));
            _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_7, hideGameObjects));
        }
        await Task.Yield();

        for (int i = 0; i < num4nt; i++)
        {
            //_nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_4, hideGameObjects));
            //_nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_4, hideGameObjects));
            _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_3, hideGameObjects));
            _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_3, hideGameObjects));
        }

        for (int i = 0; i < num2nt; i++)
        {
            //_nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_2, hideGameObjects));
            //_nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_2, hideGameObjects));
            _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
            _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
        }

        for (int i = 0; i < num1nt; i++)
        {
            //_nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_1, hideGameObjects));
            //_nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_1, hideGameObjects));
        }

        for (int i = 0; i < numConnectingBackbones; i++)
        {
            _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
            _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
        }
        await Task.Yield();
    }

    /// <summary>
    /// Draws the backbones between the nucleotides in the helix.
    /// </summary>
    /// <param name="start">Start index of nucleotide to begin drawing backbones.</param>
    private async Task DrawBackbones(int start, bool hideBackbone)
    {
        for (int i = start; i < _nucleotidesA.Count; i++)
        {
            GameObject cylinderA = DrawPoint.MakeBackbone(i - 1, _id, 1, NucleotidesA[i].transform.position, NucleotidesA[i - 1].transform.position, hideBackbone);
            _backbonesA.Add(cylinderA);
            _helixA.Add(cylinderA);
            //cylinder.transform.SetParent(_parent.transform);

            GameObject cylinderB = DrawPoint.MakeBackbone(i - 1, _id, 0, NucleotidesB[i].transform.position, NucleotidesB[i - 1].transform.position, hideBackbone);
            _backbonesB.Add(cylinderB);
            _helixB.Add(cylinderB);
            //cylinder.transform.SetParent(_parent.transform);
            
            if (i % 8 == 0)
            {
                await Task.Yield();
            }
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

    /// <summary>
    /// Changes rendering of helix and its components.
    /// </summary>
    public void ChangeRendering()
    {
        Debug.Log("nucleotide view: " + s_nucleotideView);
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
        foreach (GameObject nucleotide in _nucleotidesA)
        {
            nucleotide.transform.SetParent(go.transform, true);
            //Strand strand = Utils.GetStrand(nucleotide);
            //strand?.Cone.transform.SetParent(go.transform, true);
        }
        foreach (GameObject nucleotide in _nucleotidesB)
        {
            nucleotide.transform.SetParent(go.transform, true);
            //Strand strand = Utils.GetStrand(nucleotide);
            //strand?.Cone.transform.SetParent(go.transform, true);
        }
        foreach (GameObject nucleotide in _backbonesA)
        {
            nucleotide.transform.SetParent(go.transform, true);
        }
        foreach (GameObject nucleotide in _backbonesB)
        {
            nucleotide.transform.SetParent(go.transform, true);
        }
    }

    /// <summary>
    /// Resets parents of all Helix gameobjects to null. This is used in TranslateHandle.cs
    /// </summary>
    public void ResetParent()
    {
        foreach (GameObject nucleotide in _nucleotidesA)
        {
            nucleotide.transform.SetParent(null);
            //Strand strand = Utils.GetStrand(nucleotide);
            //strand?.Cone.transform.SetParent(null);
        }
        foreach (GameObject nucleotide in _nucleotidesB)
        {
            nucleotide.transform.SetParent(null);
            //Strand strand = Utils.GetStrand(nucleotide);
            //fstrand?.Cone.transform.SetParent(null);
        }
        foreach (GameObject nucleotide in _backbonesA)
        {
            nucleotide.transform.SetParent(null);
        }
        foreach (GameObject nucleotide in _backbonesB)
        {
            nucleotide.transform.SetParent(null);
        }
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
}

