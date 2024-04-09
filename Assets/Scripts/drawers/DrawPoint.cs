/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.Object;
using static GlobalVariables;

/// <summary>
/// Creates needed gameobjects like nucleotides, backbones, cones, Xovers, spheres, and grids.
/// </summary>
public static class DrawPoint
{
    private const int SPLINE_RESOLUTION = 16;
    private const float TUBE_SIZE = 0.01f;
    private const float LOOPOUT_SIZE = 0.005f;
    // Ratio determines how much loopout "bends." Higher ratio, more bending.
    private const float LOOPOUT_BEND_RATIO = 0.15f;

    /// <summary>
    /// Creates nucleotide at given position.
    /// </summary>
    /// <param name="position">Position of nucleotide.</param>
    /// <param name="id">Id of nuecleotide.</param>
    /// <param name="helixId">Id of helix nucleotide is in.</param>
    /// <param name="ssDirection">Direction of nucleotide.</param>
    /// <returns>GameObject of Nucleotide.</returns>
    public static GameObject MakeNucleotide(Vector3 position, int id, int helixId, int direction)
    {
        GameObject sphere =
                    Instantiate(Resources.Load("Nucleotide"),
                    position,
                    Quaternion.identity) as GameObject;
        sphere.name = "nucleotide" + id;

        var ntc = sphere.GetComponent<NucleotideComponent>();
        ntc.Id = id;
        ntc.HelixId = helixId;
        ntc.Direction = direction;
        ntc.IsBackbone = false;
        return sphere;
    }

    public static GameObject MakeCone()
    {
        GameObject cone = Instantiate(Resources.Load("Cone")) as GameObject;
        return cone;
    }

    /// <summary>
    /// Creates a backbone between given two nucleotides.
    /// </summary>
    /// <param name="id">id of backbone.</param>
    /// <param name="start">Position of start nucleotide.</param>
    /// <param name="end">Position of end nucleotide.</param>
    /// <returns>GameObject of backbone.</returns>
    public static GameObject MakeBackbone(int id, int helixId, int direction, Vector3 start, Vector3 end)
    {
        GameObject cylinder = Instantiate(Resources.Load("Backbone"),
                                    Vector3.zero,
                                    Quaternion.identity) as GameObject;
        cylinder.name = "Backbone" + id;
        BackBoneComponent backBoneComponent = cylinder.GetComponent<BackBoneComponent>();
        backBoneComponent.Id = id;
        backBoneComponent.HelixId = helixId;
        backBoneComponent.Direction = direction;
        backBoneComponent.IsBackbone = true;

        Vector3 cylinderDefaultOrientation = new Vector3(0, 1, 0);

        // Position
        cylinder.transform.position = (end + start) / 2.0F;

        // Rotation
        Vector3 dirV = Vector3.Normalize(start - end);
        Vector3 rotAxisV = dirV + cylinderDefaultOrientation;
        rotAxisV = Vector3.Normalize(rotAxisV);
        cylinder.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);

        // Scale        
        float dist = Vector3.Distance(end, start);
        cylinder.transform.localScale = new Vector3(0.005f, dist / 2, 0.005f);
        return cylinder;
    }

    public static GameObject MakeXover(GameObject prevGO, GameObject nextGO, int strandId)
    {
        GameObject xover =
                   Instantiate(Resources.Load("Xover"),
                   Vector3.zero,
                   Quaternion.identity) as GameObject;
        xover.name = "xover";
        Vector3 cylDefaultOrientation = new Vector3(0, 1, 0);

        // Position
        xover.transform.position = (nextGO.transform.position + prevGO.transform.position) / 2.0F;

        // Rotation
        Vector3 dirV = Vector3.Normalize(nextGO.transform.position - prevGO.transform.position);
        Vector3 rotAxisV = dirV + cylDefaultOrientation;
        rotAxisV = Vector3.Normalize(rotAxisV);
        xover.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);

        // Scale        
        float dist = Vector3.Distance(nextGO.transform.position, prevGO.transform.position);
        xover.transform.localScale = new Vector3(0.005f, dist / 2, 0.005f);

        var xoverComp = xover.GetComponent<XoverComponent>();
        xoverComp.StrandId = strandId;
        xoverComp.Length = dist;
        xoverComp.Color = prevGO.GetComponent<NucleotideComponent>().Color;
        prevGO.GetComponent<NucleotideComponent>().Xover = xover;
        nextGO.GetComponent<NucleotideComponent>().Xover = xover;
        xoverComp.PrevGO = prevGO;
        xoverComp.NextGO = nextGO;

        return xover;
    }

    public static GameObject MakeXoverSuggestion(GameObject prevGO, GameObject nextGO)
    {
        GameObject xoverSuggestion =
                 Instantiate(Resources.Load("XoverSuggestion"),
                 Vector3.zero,
                 Quaternion.identity) as GameObject;
        xoverSuggestion.name = "xoverSuggestion";
        XoverSuggestionComponent xoverSuggestionComponent = xoverSuggestion.GetComponent<XoverSuggestionComponent>();
        xoverSuggestionComponent.NucleotideComponent0 = prevGO.GetComponent<NucleotideComponent>();
        xoverSuggestionComponent.NucleotideComponent1 = nextGO.GetComponent<NucleotideComponent>();

        Vector3 cylDefaultOrientation = new Vector3(0, 1, 0);

        // Position
        xoverSuggestion.transform.position = (nextGO.transform.position + prevGO.transform.position) / 2.0F;

        // Rotation
        Vector3 dirV = Vector3.Normalize(nextGO.transform.position - prevGO.transform.position);
        Vector3 rotAxisV = dirV + cylDefaultOrientation;
        rotAxisV = Vector3.Normalize(rotAxisV);
        xoverSuggestion.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);

        // Scale        
        float dist = Vector3.Distance(nextGO.transform.position, prevGO.transform.position);
        xoverSuggestion.transform.localScale = new Vector3(0.005f, dist / 2, 0.005f);
        //xoverComp.SetLength(dist);

        s_xoverSuggestions.Add(xoverSuggestion.GetComponent<XoverSuggestionComponent>());

        return xoverSuggestion;
    }

    public static GameObject MakeSphere(Vector3 position, string name)
    {
        // make sphere
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.position = position;
        sphere.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);

        sphere.AddComponent<XRGrabInteractable>();

        var sphereRigidbody = sphere.GetComponent<Rigidbody>();
        sphereRigidbody.useGravity = false;
        sphereRigidbody.isKinematic = true;

        var sphereRenderer = sphere.GetComponent<Renderer>();
        sphereRenderer.material.SetColor("_Color", Color.gray);

        return sphere;
    }

    /// <summary>
    /// Creates grid circle game object at specified location and rotates it depending on plane direction.
    /// </summary>
    /// <param name="startPosition">Start position of grid object.</param>
    /// <param name="xOffset">2D x direction offset of grid circle in grid object.</param>
    /// <param name="yOffset">2D y direction offset of grid circle in grid object.</param>
    /// <param name="plane">Plane direction of grid object.</param>
    /// <returns></returns>
    public static GameObject MakeGridCircleGO(Vector3 startPosition, float xOffset, float yOffset, string plane)
    {
        // Calculate game position.
        Vector3 gamePosition;
        if (plane.Equals("XY"))
        {
            gamePosition = new Vector3(startPosition.x + xOffset, startPosition.y + yOffset, startPosition.z);
        }
        else if (plane.Equals("YZ"))
        {
            gamePosition = new Vector3(startPosition.x, startPosition.y + xOffset, startPosition.z + yOffset);
        }
        else
        {
            gamePosition = new Vector3(startPosition.x + xOffset, startPosition.y, startPosition.z + yOffset);
        }

        GameObject gridCircle = Instantiate(Resources.Load("GridCircle"),
                   gamePosition,
                   Quaternion.identity) as GameObject;

        // Calculate rotation
        if (plane.Equals("XY"))
        {
            gridCircle.transform.Rotate(90f, 0f, 0f, 0);
        }
        else if (plane.Equals("YZ"))
        {
            gridCircle.transform.Rotate(90f, 90f, 0f, 0);
        }
       
        gridCircle.name = "gridPoint";
        GridComponent gridComponent = gridCircle.GetComponent<GridComponent>();
        gridComponent.Position = gamePosition;
        return gridCircle;
    }

    public static GameObject MakeStrandCylinder(List<GameObject> nucleotides, Color32 color)
    {
        GameObject tube = new GameObject("tube");
        var tubeRend = tube.AddComponent<TubeRenderer>();
        var meshRend = tube.GetComponent<MeshRenderer>();
        tubeRend.radius = TUBE_SIZE;
        meshRend.material.SetColor("_Color", color);

        // TODO: figure out how to make this smoother. Increase resolution? 
        SplineMaker splineMaker = tube.AddComponent<SplineMaker>();
        splineMaker.onUpdated.AddListener((points) => tubeRend.points = points); // updates tube renderer points when anchorPoints is changed.
        splineMaker.pointsPerSegment = SPLINE_RESOLUTION;
        Vector3[] anchorPoints = new Vector3[nucleotides.Count];

        for (int i = 0; i < nucleotides.Count; i += 1)
        {
            anchorPoints[i] = nucleotides[i].transform.position;
        }

        splineMaker.anchorPoints = anchorPoints;
        
        return tube;
    }

    /// <summary>
    /// Creates a loopout between the given two nucleotides.
    /// </summary>
    /// <param name="length">Sequence length of the loopout.</param>
    /// <param name="nucleotide0">Nucleotide that loopout begins on.</param>
    /// <param name="nucleotide1">Nucleotide that loopout ends on.</param>
    /// <param name="color">Color of the loopout.</param>
    /// <returns>Loopout component of the created loopout in scene.</returns>
    public static LoopoutComponent MakeLoopout(int length, NucleotideComponent nucleotide0, NucleotideComponent nucleotide1, int strandId)
    {
        GameObject loopout = new GameObject("loopout");

        TubeRenderer tubeRenderer = loopout.AddComponent<TubeRenderer>();
        tubeRenderer.radius = LOOPOUT_SIZE;

        SplineMaker splineMaker = loopout.AddComponent<SplineMaker>();
        splineMaker.onUpdated.AddListener((points) => tubeRenderer.points = points); // updates tube renderer points when anchorPoints is changed.
        splineMaker.pointsPerSegment = SPLINE_RESOLUTION;
        Vector3[] anchorPoints = new Vector3[3];

        anchorPoints[0] = nucleotide0.transform.position;
        // Calculate middle point that determines bend
        float distance = (nucleotide0.transform.position - nucleotide1.transform.position).magnitude;
        Vector3 midpoint = (nucleotide0.transform.position + nucleotide1.transform.position) / 2;
        Vector3 midpointToNucleotide1 = nucleotide1.transform.position - midpoint;
        float a = midpointToNucleotide1.x;
        float b = midpointToNucleotide1.y;
        float c = midpointToNucleotide1.z;
        Vector3 orthogonalVector = new Vector3(b + c, c - a, -a - b).normalized;
        Vector3 bendPoint = midpoint + orthogonalVector * distance * LOOPOUT_BEND_RATIO;
        anchorPoints[1] = bendPoint;
        anchorPoints[2] = nucleotide1.transform.position;

        splineMaker.anchorPoints = anchorPoints;

        MeshRenderer meshRenderer = loopout.GetComponent<MeshRenderer>();
        meshRenderer.material.SetColor("_Color", nucleotide0.Color);

        LoopoutComponent loopoutComponent = loopout.AddComponent<LoopoutComponent>();
        loopoutComponent.SequenceLength = length;
        loopoutComponent.PrevGO = nucleotide0.gameObject;
        loopoutComponent.NextGO = nucleotide1.gameObject;
        loopoutComponent.StrandId = strandId;
        loopoutComponent.Color = nucleotide0.Color;

        nucleotide0.Xover = loopout;
        nucleotide1.Xover = loopout;

        // Create interactable
        GameObject loopoutInteractable =
                 Instantiate(Resources.Load("LoopoutInteractable"),
                 Vector3.zero,
                 Quaternion.identity) as GameObject;
        loopoutInteractable.transform.position = bendPoint;
        loopoutInteractable.GetComponent<MeshRenderer>().material.SetColor("_Color", nucleotide0.Color);
        LoopoutInteractableComponent loopoutInteractableComponent = loopoutInteractable.GetComponent<LoopoutInteractableComponent>();
        loopoutInteractableComponent.Loopout = loopoutComponent;
        loopoutComponent.Interactable = loopoutInteractableComponent;
        loopoutInteractable.transform.parent = loopout.transform;

        return loopoutComponent;
    }
}

