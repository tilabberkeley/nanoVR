/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.Object;
using static GlobalVariables;
using UnityEngine.ProBuilder;
using SplineMesh;
using UnityEngine.UIElements;

/// <summary>
/// Creates needed gameobjects like nucleotides, backbones, cones, Xovers, spheres, and grids.
/// </summary>
public static class DrawPoint
{
    private const int SPLINE_RESOLUTION = 16;
    private const float TUBE_SIZE = 0.01f;
    // private const float LOOPOUT_SIZE = 0.005f;
    // Ratio determines how much loopout "bends." Higher ratio, more bending.
    private const float LOOPOUT_BEND_RATIO = 3f;

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
                    Instantiate(GlobalVariables.Nucleotide,
                    position,
                    Quaternion.identity) as GameObject;
        sphere.name = "nucleotide" + id;

        var ntc = sphere.GetComponent<NucleotideComponent>();
        ntc.Id = id;
        ntc.HelixId = helixId;
        ntc.Direction = direction;
        ntc.IsBackbone = false;
        SaveGameObject(sphere);
        return sphere;
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
    /// Creates a backbone between given two nucleotides.
    /// </summary>
    /// <param name="id">id of backbone.</param>
    /// <param name="start">Position of start nucleotide.</param>
    /// <param name="end">Position of end nucleotide.</param>
    /// <returns>GameObject of backbone.</returns>
    public static GameObject MakeBackbone(int id, int helixId, int direction, Vector3 start, Vector3 end)
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
        cylinder.transform.localScale = new Vector3(0.005f, dist / 2, 0.005f);

        // Position
        cylinder.transform.position = (end + start) / 2.0F;

        // Rotation
        cylinder.transform.up = end - start;

        SaveGameObject(cylinder);

        return cylinder;
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

        var xoverComp = xover.GetComponent<XoverComponent>();
        xoverComp.StrandId = strandId;
        xoverComp.PrevStrandId = prevStrandId;
        xoverComp.Length = dist;
        xoverComp.Color = prevGO.GetComponent<NucleotideComponent>().Color;
        prevGO.GetComponent<NucleotideComponent>().Xover = xover;
        nextGO.GetComponent<NucleotideComponent>().Xover = xover;
        xoverComp.PrevGO = prevGO;
        xoverComp.NextGO = nextGO;
        SaveGameObject(xover);
        return xover;
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
        return gridCircle;
    }

    public static GameObject MakeBezier(List<GameObject> nucleotides, Color32 color)
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
    /// Creates a loopout between the given two nucleotides.
    /// </summary>
    /// <param name="length">Sequence length of the loopout.</param>
    /// <param name="prevNucleotide">Nucleotide that loopout begins on.</param>
    /// <param name="nextNucleotide">Nucleotide that loopout ends on.</param>
    /// <param name="color">Color of the loopout.</param>
    /// <returns>Loopout component of the created loopout in scene.</returns>
    public static LoopoutComponent MakeLoopout(int length, NucleotideComponent prevNucleotide, NucleotideComponent nextNucleotide, int strandId, int prevStrandId)
    {
        GameObject loopout = Instantiate(Loopout,
                   Vector3.zero,
                   Quaternion.identity) as GameObject;

        // Create spline
        Spline spline = loopout.GetComponent<Spline>();
        SplineMeshTiling splineMeshTiling = loopout.GetComponent<SplineMeshTiling>();

        Vector3 prevLocation = prevNucleotide.transform.position;
        Vector3 nextLocation = nextNucleotide.transform.position;
        Vector3 prevToNext = nextLocation - prevLocation;
        float distance = prevToNext.magnitude;

        float a = prevToNext.x;
        float b = prevToNext.y;
        float c = prevToNext.z;
        Vector3 orthogonalVector = new Vector3(b + c, c - a, -a - b).normalized;

        Vector3 prevDirection = prevLocation + (orthogonalVector * distance * LOOPOUT_BEND_RATIO);
        Vector3 nextDirection = nextLocation - (orthogonalVector * distance * LOOPOUT_BEND_RATIO);

        SplineNode prevNode = new SplineNode(prevLocation, prevDirection);
        SplineNode nextNode = new SplineNode(nextLocation, nextDirection);

        // Remove default nodes from spline
        SplineNode toRemove0 = spline.nodes[0];
        SplineNode toRemove1 = spline.nodes[1];

        // Update spline, and create mesh
        spline.AddNode(prevNode);
        spline.AddNode(nextNode);
        spline.RemoveNode(toRemove0);
        spline.RemoveNode(toRemove1);
        splineMeshTiling.CreateMeshes();

        // splineMeshTiling.material.SetColor("_Color", prevNucleotide.Color);
        GameObject meshGO = loopout.transform.GetChild(0).GetChild(0).gameObject;
        Debug.Log(meshGO == null);
        meshGO.GetComponent<Material>().SetColor("_Color", prevNucleotide.Color);

        // Add outline component
        Outline outline = loopout.AddComponent<Outline>();
        outline.enabled = false;
        outline.OutlineWidth = 3;

        // Add loopout component
        LoopoutComponent loopoutComponent = loopout.AddComponent<LoopoutComponent>();
        loopoutComponent.SequenceLength = length;
        loopoutComponent.PrevGO = prevNucleotide.gameObject;
        loopoutComponent.NextGO = nextNucleotide.gameObject;
        loopoutComponent.StrandId = strandId;
        loopoutComponent.PrevStrandId = prevStrandId;
        loopoutComponent.Color = prevNucleotide.Color;

        prevNucleotide.Xover = loopout;
        nextNucleotide.Xover = loopout;

        // Create interactable
        //GameObject loopoutInteractable =
        //         Instantiate(Resources.Load("LoopoutInteractable"),
        //         Vector3.zero,
        //         Quaternion.identity) as GameObject;
        //loopoutInteractable.transform.position = bendPoint;
        //loopoutInteractable.GetComponent<MeshRenderer>().material.SetColor("_Color", prevNucleotide.Color);
        //LoopoutInteractableComponent loopoutInteractableComponent = loopoutInteractable.GetComponent<LoopoutInteractableComponent>();
        //loopoutInteractableComponent.Loopout = loopoutComponent;
        //loopoutComponent.Interactable = loopoutInteractableComponent;
        //loopoutInteractable.transform.parent = loopout.transform;

        return loopoutComponent;
    }
}

