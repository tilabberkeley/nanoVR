/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using SplineMesh;
using UnityEngine;

/// <summary>
/// Component for loopout game objects. Treating a loopout as a crossover, except that it has a sequence associated with it. 
/// </summary>
public class LoopoutComponent : XoverComponent
{
    // Sequence length of this loopout.
    private int _sequenceLength;
    public int SequenceLength { get => _sequenceLength; set => _sequenceLength = value; }

    // Sequence of this loopout.
    private string _sequence = "";
    public string Sequence { get => _sequence; set => _sequence = value; }

    private const float SCALE = 0.005f;
    private const float LOOPOUT_BEND_FACTOR = 0.25f;

    // Cached references to the Spline and SplineMeshTiling components.
    private Spline spline;
    private SplineMeshTiling splineMeshTiling;

    // Optionally, you can cache the loopout GameObject reference if needed.
    private GameObject loopoutObject;


    // Changing color of loopout done through mesh renderer
    public override Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            GetComponent<MeshRenderer>().material.SetColor("_Color", value);
        }
    }

    /*void Awake()
    {
        // Assuming LoopoutComponent is on the meshGO, which is a grandchild of the actual loopout.
        loopoutObject = transform.parent.parent.gameObject;

        // Get the Spline and SplineMeshTiling components from the loopout object.
        spline = loopoutObject.GetComponent<Spline>();
        splineMeshTiling = loopoutObject.GetComponent<SplineMeshTiling>();

        //if (spline == null || splineMeshTiling == null)
        //{
        //    Debug.LogError("Spline and/or SplineMeshTiling component not found on the loopout object.");
        //}
    }*/

    public override void UpdateXover()
    {
        // Get nucleotide positions from your domains.
        /*Vector3 prevPosition = prevNucl.GetPosition();
        Vector3 nextPosition = nextNucl.GetPosition();

        Vector3 midpoint = (prevPosition + nextPosition) / 2;

        // Update the loopout object position so that the spline remains in its local coordinate system.
        loopoutObject.transform.position = midpoint;

        // Calculate positions relative to the new midpoint applying the scale.
        Vector3 midPointToPrevScaled = (prevPosition - midpoint) / SCALE;
        Vector3 midPointToNextScaled = (nextPosition - midpoint) / SCALE;

        // Compute an orthogonal vector for creating a nice bend.
        Vector3 prevToNext = nextPosition - prevPosition;
        float distance = prevToNext.magnitude;
        float a = prevToNext.x;
        float b = prevToNext.y;
        float c = prevToNext.z;
        Vector3 orthogonalVector = new Vector3(b + c, c - a, -a - b).normalized / SCALE;

        Vector3 prevDirection = midPointToPrevScaled + (orthogonalVector * distance * LOOPOUT_BEND_FACTOR);
        Vector3 nextDirection = midPointToNextScaled - (orthogonalVector * distance * LOOPOUT_BEND_FACTOR);

        // Create new nodes for the spline.
        SplineNode prevNode = new SplineNode(midPointToPrevScaled, prevDirection);
        SplineNode nextNode = new SplineNode(midPointToNextScaled, nextDirection);

        // Clear existing nodes and add the updated ones.
        spline.nodes.Clear();
        spline.AddNode(prevNode);
        spline.AddNode(nextNode);

        // Regenerate the mesh with the updated spline.
        splineMeshTiling.CreateMeshes();*/
        Vector3 start = prevNucl.GetPosition();
        Vector3 end = nextNucl.GetPosition();

        // Scale        
        float dist = Vector3.Distance(end, start);
        transform.localScale = new Vector3(0.4f, dist, 0.4f);

        // Position
        transform.position = (end + start) / 2.0F;

        // Rotation
        transform.up = end - start;
    }
}
