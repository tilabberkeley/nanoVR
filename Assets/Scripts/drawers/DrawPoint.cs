/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.Object;
using static GlobalVariables;
using SplineMesh;

/// <summary>
/// Creates needed gameobjects like nucleotides, backbones, cones, Xovers, spheres, and grids.
/// </summary>
public static class DrawPoint
{
    private const int SPLINE_RESOLUTION = 1;
    private const float TUBE_SIZE = 0.01f;
    // private const float LOOPOUT_SIZE = 0.005f;
    // Factor determines how much loopout "bends." Higher factor, more bending.
    private const float LOOPOUT_BEND_FACTOR = 0.25f;

    /// <summary>
    /// Creates nucleotide at given position.
    /// </summary>
    /// <param name="position">Position of nucleotide.</param>
    /// <param name="id">Id of nuecleotide.</param>
    /// <param name="helixId">Id of helix nucleotide is in.</param>
    /// <param name="ssDirection">Direction of nucleotide.</param>
    /// <returns>GameObject of Nucleotide.</returns>
    public static GameObject MakeNucleotide(Vector3 position, int id, int helixId, int direction, bool hideNucleotide = false)
    {
        GameObject sphere =
                    Instantiate(GlobalVariables.Nucleotide,
                    position,
                    Quaternion.identity) as GameObject;
        sphere.name = "nucleotide" + id;

        NucleotideComponent ntc = sphere.GetComponent<NucleotideComponent>();
        //SequenceComponent seqComp = sphere.GetComponent<SequenceComponent>();
        ntc.Id = id;
        ntc.HelixId = helixId;
        ntc.Direction = direction;
        ntc.IsBackbone = false;
        //seqComp.HasComplement = true;
        SaveGameObject(sphere);
        sphere.isStatic = true;
        sphere.SetActive(!hideNucleotide);
        return sphere;
    }

    public static void SetNucleotide(GameObject sphere, Vector3 position, int id, int helixId, int direction, bool hideNucleotide = false, bool isOxview = false)
    {
        sphere.transform.position = position;
        sphere.name = "nucleotide" + id;

        NucleotideComponent ntc = sphere.GetComponent<NucleotideComponent>();
        //SequenceComponent seqComp = sphere.GetComponent<SequenceComponent>();
        ntc.Id = id;
        ntc.HelixId = helixId;
        ntc.Direction = direction;
        ntc.IsOxview = isOxview;
        ntc.IsBackbone = false;
        //seqComp.HasComplement = true;
        //sphere.transform.SetParent(null);
        SaveGameObject(sphere);
        sphere.isStatic = true;
        //sphere.SetActive(!hideNucleotide);
        sphere.SetActive(!hideNucleotide);
    }


    public static List<GameObject> MakeNucleotides(NucleotideSize size, bool hide)
    {
        List<GameObject> children = new List<GameObject>();

        switch (size)
        {
            case NucleotideSize.LENGTH_64:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Nucleotide64,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 64; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case NucleotideSize.LENGTH_32:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Nucleotide32,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 32; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case NucleotideSize.LENGTH_16:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Nucleotide16,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 16; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case NucleotideSize.LENGTH_8:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Nucleotide8,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 8; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case NucleotideSize.LENGTH_4:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Nucleotide4,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 4; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case NucleotideSize.LENGTH_2:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Nucleotide2,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 2; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case NucleotideSize.LENGTH_1:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Nucleotide,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    children.Add(spheres);
                    break;
                }
        }
        
        return children;
    }

    /// <summary>
    /// Creates a cone gameobjects, used to display the direction of a strand.
    /// </summary>
    public static GameObject MakeCone()
    {
        GameObject cone = Instantiate(Cone) as GameObject;
        SaveGameObject(cone);
        return cone;
    }

    /// <summary>
    /// Creates a backbone between two nucleotides.
    /// </summary>
    /// <param name="id">id of backbone.</param>
    /// <param name="start">Position of start nucleotide.</param>
    /// <param name="end">Position of end nucleotide.</param>
    /// <returns>GameObject of backbone.</returns>
    public static GameObject MakeBackbone(int id, int helixId, int direction, Vector3 start, Vector3 end, bool hideBackbone = false)
    {
        GameObject cylinder = Instantiate(Backbone,
                                    Vector3.zero,
                                    Quaternion.identity) as GameObject;
        cylinder.name = "Backbone" + id;

        BackBoneComponent backBoneComponent = cylinder.GetComponent<BackBoneComponent>();
        backBoneComponent.Id = id;
        backBoneComponent.HelixId = helixId;
        backBoneComponent.Direction = direction;
        backBoneComponent.IsBackbone = true;     

        // Scale        
        float dist = Vector3.Distance(end, start);
        //cylinder.transform.localScale = new Vector3(0.005f, dist / 2, 0.005f); // For "Backbone" prefab
        cylinder.transform.localScale = new Vector3(0.25f, dist / 2, 0.25f);   // For "Cylinder" (Probuilder) prefab

        // Position
        cylinder.transform.position = (end + start) / 2.0f;

        // Rotation
        cylinder.transform.up = end - start;

        SaveGameObject(cylinder);
        cylinder.isStatic = true;
        cylinder.SetActive(false);
        return cylinder;
    }

    public static List<GameObject> MakeBackbones(BackboneSize size, bool hide)
    {
        List<GameObject> children = new List<GameObject>();

        switch (size)
        {
            case BackboneSize.LENGTH_63:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Backbone63,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 63; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case BackboneSize.LENGTH_31:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Backbone31,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 31; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case BackboneSize.LENGTH_15:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Backbone15,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 15; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case BackboneSize.LENGTH_7:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Backbone7,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 7; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case BackboneSize.LENGTH_3:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Backbone3,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    for (int j = 0; j < 3; j++)
                    {
                        GameObject sphere = spheres.transform.GetChild(j).gameObject;
                        //sphere.transform.SetParent(null);
                        children.Add(sphere);
                    }
                    break;
                }
            case BackboneSize.LENGTH_1:
                {
                    GameObject spheres =
                       Instantiate(GlobalVariables.Backbone,
                       Vector3.zero,
                       Quaternion.identity) as GameObject;
                    children.Add(spheres);
                    break;
                }
        }

        return children;
    }

    public static void SetBackbone(GameObject cylinder, int id, int helixId, int direction, Vector3 start, Vector3 end, bool hideBackbone = false, bool isOxview = false)
    {
        cylinder.name = "Backbone" + id;

        BackBoneComponent backBoneComponent = cylinder.GetComponent<BackBoneComponent>();
        backBoneComponent.Id = id;
        backBoneComponent.HelixId = helixId;
        backBoneComponent.Direction = direction;
        backBoneComponent.IsOxview = isOxview;
        backBoneComponent.IsBackbone = true;

        // Scale        
        float dist = Vector3.Distance(end, start);
        cylinder.transform.localScale = new Vector3(0.25f, dist, 0.25f);   // For "Cylinder" (Probuilder) prefab

        // Position
        cylinder.transform.position = (end + start) / 2.0f;

        // Rotation
        cylinder.transform.up = end - start;
        //cylinder.transform.SetParent(null);
        SaveGameObject(cylinder);
        cylinder.isStatic = true;
        cylinder.SetActive(!hideBackbone);
    }

    /// <summary>
    /// Creates a crossover suggestion at the given nucleotides gameobjects.
    /// </summary>
    public static GameObject MakeXoverSuggestion(GameObject prevGO, GameObject nextGO)
    {
        GameObject xoverSuggestion =
                 Instantiate(XoverSuggestion,
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

    /// <summary>
    /// Creates grid circle game object at specified location and rotates it depending on plane direction.
    /// </summary>
    /// <param name="startPosition">Start position of grid object.</param>
    /// <param name="startGridCircle">Grid circle associated with original start position</param>
    /// <param name="xOffset">2D x direction offset of grid circle in grid object.</param>
    /// <param name="yOffset">2D y direction offset of grid circle in grid object.</param>
    /// <param name="plane">Plane direction of grid object.</param>
    /// <returns></returns>
    public static GameObject MakeGridCircleGO(Vector3 startPosition, GameObject startGridCircle, float xOffset, float yOffset, string plane)
    {
        // Calculate position.
        Vector3 position;
        if (startGridCircle != null)
        {
            position = startPosition + xOffset * startGridCircle.transform.right + yOffset * startGridCircle.transform.up;
        }
        else
        {
            if (plane.Equals("XY"))
            {
                position = new Vector3(startPosition.x + xOffset, startPosition.y + yOffset, startPosition.z);
            }
            else if (plane.Equals("YZ"))
            {
                position = new Vector3(startPosition.x, startPosition.y + xOffset, startPosition.z + yOffset);
            }
            else
            {
                position = new Vector3(startPosition.x + xOffset, startPosition.y, startPosition.z + yOffset);
            }
        }


        GameObject gridCircle = Instantiate(GridCircle,
                   position,
                   Quaternion.identity) as GameObject;

        // Calculate rotation
        if (startGridCircle != null)
        {
            gridCircle.transform.rotation = startGridCircle.transform.rotation;
        }
        else
        {
            if (plane.Equals("XZ"))
            {
                gridCircle.transform.Rotate(0f, 0f, 90f, 0);
            }
            else if (plane.Equals("YZ"))
            {
                gridCircle.transform.Rotate(0f, 90f, 0f, 0);
            }
        }

        gridCircle.name = "gridPoint";
        GridComponent gridComponent = gridCircle.GetComponent<GridComponent>();
        gridComponent.Position = position;
        SaveGameObject(gridCircle);
        gridCircle.isStatic = true;
        return gridCircle;
    }

    /// <summary>
    /// Creates a simpler bezier representation gameobject of a domain. 
    /// </summary>
    public static GameObject MakeDomainBezier(List<DNAComponent> dnaComponents, Color32 color, out Vector3 bezierStartPoint, out Vector3 bezierEndPoint)
    {
        GameObject tube = new GameObject("tube");
        MeshRenderer meshRend = tube.AddComponent<MeshRenderer>();
        TubeRenderer tubeRend = tube.AddComponent<TubeRenderer>();
        tubeRend.radius = TUBE_SIZE;
        meshRend.material.SetColor("_Color", color);
        
        Vector3[] anchorPoints = SplineInterpolation.GenerateBezierSpline(dnaComponents, SPLINE_RESOLUTION);

        tubeRend.points = anchorPoints;

        bezierStartPoint = anchorPoints[0];
        bezierEndPoint = anchorPoints[anchorPoints.Length - 1];

        GameObject endpoint = MakeBezierEndpoint(bezierStartPoint, color);
        endpoint.transform.SetParent(tube.transform);
        endpoint = MakeBezierEndpoint(bezierEndPoint, color);
        endpoint.transform.SetParent(tube.transform);

        return tube;
    }

    /// <summary>
    /// Creates a simpler bezier represetation gameobject of a xover.
    /// </summary>
    public static GameObject MakeXoverBezier(XoverComponent xoverComponent, Color32 color)
    {
        GameObject tube = new GameObject("tube");
        MeshRenderer meshRend = tube.AddComponent<MeshRenderer>();
        TubeRenderer tubeRend = tube.AddComponent<TubeRenderer>();
        tubeRend.radius = TUBE_SIZE;
        meshRend.material.SetColor("_Color", color);

        Vector3[] anchorPoints = new Vector3[2];

        // Set nucleotides to be start and end point of bezier xover.
        anchorPoints[0] = xoverComponent.PrevGO.GetComponent<DNAComponent>().Domain.BezierStartPoint;
        anchorPoints[1] = xoverComponent.NextGO.GetComponent<DNAComponent>().Domain.BezierEndPoint;

        tubeRend.points = anchorPoints;

        // Set endpoints to parent, so they are destroyed when tube is destroyed
        GameObject endpoint = MakeBezierEndpoint(anchorPoints[0], color);
        endpoint.transform.SetParent(tube.transform);
        endpoint = MakeBezierEndpoint(anchorPoints[1], color);
        endpoint.transform.SetParent(tube.transform);

        return tube;
    }

    /// <summary>
    /// Instantiates a bezier endpoint at the given location
    /// </summary>
    private static GameObject MakeBezierEndpoint(Vector3 position, Color32 color)
    {
        GameObject bezierEndpoint = Instantiate(
            BezierEndpoint,
            position,
            Quaternion.identity);

        bezierEndpoint.name = "Bezier Endpoint";
        bezierEndpoint.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        // Set sphere's radius
        bezierEndpoint.transform.localScale = new Vector3(TUBE_SIZE * 2, TUBE_SIZE * 2, TUBE_SIZE * 2);

        return bezierEndpoint;
    }

    private static void SaveGameObject(GameObject go)
    {
        if (s_visualMode)
        {
            allVisGameObjects.Add(go);
        }
        else
        {
            allGameObjects.Add(go);
        }
    }

    /// <summary>
    /// Creates a crossover at the given nucleotide gameobjects.
    /// </summary>
    public static GameObject MakeXover(GameObject prevGO, GameObject nextGO, int strandId, int prevStrandId)
    {
        GameObject xover =
                   Instantiate(Xover,
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

        NucleotideComponent prevNucleotideComponent = prevGO.GetComponent<NucleotideComponent>();
        NucleotideComponent nextNucleotideComponent = nextGO.GetComponent<NucleotideComponent>();

        var xoverComp = xover.GetComponent<XoverComponent>();
        xoverComp.StrandId = strandId;
        xoverComp.PrevStrandId = prevStrandId;
        xoverComp.Length = dist;
        xoverComp.Color = prevNucleotideComponent.Color;
        xoverComp.SavedColor = nextNucleotideComponent.Color;
        prevNucleotideComponent.Xover = xover;
        nextNucleotideComponent.Xover = xover;
        xoverComp.PrevGO = prevGO;
        xoverComp.NextGO = nextGO;
        SaveGameObject(xover);
        return xover;
    }

    /// <summary>
    /// Creates a loopout between the given two nucleotides.
    /// </summary>
    /// <param name="sequenceLengt">Sequence length of the loopout.</param>
    /// <param name="prevNucleotide">Nucleotide that loopout begins on.</param>
    /// <param name="nextNucleotide">Nucleotide that loopout ends on.</param>
    /// <param name="color">Color of the loopout.</param>
    /// <returns>Loopout component of the created loopout in scene.</returns>
    public static GameObject MakeLoopout(GameObject prevNucleotide, GameObject nextNucleotide, int strandId, int prevStrandId, int sequenceLength)
    {
        // Transform scale of the loopout gamebobject - needed for relative node location.
        float scale = 0.005f;

        /* For this spline, the location of the gameobject is placed between the two nucleotides.
         * The locations of the nodes are relative to the center of the gameobject, so the center
         * of the gameobject is the origin of the node coordinate system. So to find the location
         * of the nodes, you have to find the direction from the center of the gameobject
         * (in this case the midpoint) to where you want to node in the actual world space. This also 
         * needs to be scaled by the scale of the gameobject, which is in the transform component. 
         * You can see this in the loopout prefab. For a nice bend, I just patterned matched. You
         * just have to make the direction the location of the node +/- an orthogonal vector. 
         * Again, I just patterned matched to figure this out, not exactly sure how it works. */
        Vector3 prevLocation = prevNucleotide.transform.position;
        Vector3 nextLocation = nextNucleotide.transform.position;
        Vector3 midpoint = (prevLocation + nextLocation) / 2;

        Vector3 midPointToPrevScaled = (prevLocation - midpoint) / scale;
        Vector3 midPointToNextScaled = (nextLocation - midpoint) / scale;

        GameObject loopout = Instantiate(Loopout, midpoint, Quaternion.identity);

        // Create spline
        Spline spline = loopout.GetComponent<Spline>();
        SplineMeshTiling splineMeshTiling = loopout.GetComponent<SplineMeshTiling>();
        
        Vector3 prevToNext = nextLocation - prevLocation;
        float distance = prevToNext.magnitude;

        float a = prevToNext.x;
        float b = prevToNext.y;
        float c = prevToNext.z;
        // Also have to scale orthogonal vector? I didn't try without it. Either way just adjust bend factor.
        Vector3 orthogonalVector = new Vector3(b + c, c - a, -a - b).normalized / scale;

        Vector3 prevDirection = midPointToPrevScaled + (orthogonalVector * distance * LOOPOUT_BEND_FACTOR);
        Vector3 nextDirection = midPointToNextScaled - (orthogonalVector * distance * LOOPOUT_BEND_FACTOR);

        SplineNode prevNode = new SplineNode(midPointToPrevScaled, prevDirection);
        SplineNode nextNode = new SplineNode(midPointToNextScaled, nextDirection);

        // Remove default nodes from spline
        SplineNode toRemove0 = spline.nodes[0];
        SplineNode toRemove1 = spline.nodes[1];

        // Update spline, and create mesh
        spline.AddNode(prevNode);
        spline.AddNode(nextNode);
        spline.RemoveNode(toRemove0);
        spline.RemoveNode(toRemove1);
        splineMeshTiling.CreateMeshes();

        // SplineMeshTiling script adds a gameobject as a grandchild of the loopout, which is where the mesh is attached.
        GameObject meshGO = loopout.transform.GetChild(0).GetChild(0).gameObject;

        // Add xr interactable to mesh gameobject
        meshGO.AddComponent<XRSimpleInteractable>();

        // Add outline component
        Outline outline = meshGO.AddComponent<Outline>();
        outline.enabled = false;
        outline.OutlineWidth = 3;

        // Set loopout component properties ON meshGO
        NucleotideComponent prevNucleotideComponent = prevNucleotide.GetComponent<NucleotideComponent>();
        NucleotideComponent nextNucleotideComponent = nextNucleotide.GetComponent<NucleotideComponent>();

        LoopoutComponent loopoutComponent = meshGO.AddComponent<LoopoutComponent>();
        loopoutComponent.SequenceLength = sequenceLength;
        loopoutComponent.PrevGO = prevNucleotide;
        loopoutComponent.NextGO = nextNucleotide;
        loopoutComponent.StrandId = strandId;
        loopoutComponent.PrevStrandId = prevStrandId;
        loopoutComponent.Color = prevNucleotideComponent.Color;
        loopoutComponent.SavedColor = nextNucleotideComponent.Color;
        loopoutComponent.IsXover = false;

        // Assign to meshGO that has the loopout component.
        prevNucleotideComponent.Xover = meshGO;
        nextNucleotideComponent.Xover = meshGO;
        SaveGameObject(meshGO);

        return meshGO;
    }

    /// <summary>
    /// Creates a domain given a list of DNA components.
    /// </summary>
    public static DomainComponent MakeDomain(List<DNAComponent> dnaList, Strand strand)
    {
        if (dnaList.Count == 0)
        {
            throw new ArgumentException("dnaList must be non empty.");
        }

        GameObject domain = Instantiate(Domain,
                   Vector3.zero,
                   Quaternion.identity);

        DomainComponent domainComponent = domain.GetComponent<DomainComponent>();
        domainComponent.Strand = strand;

        domainComponent.Configure(dnaList);

        return domainComponent;
    }
}

