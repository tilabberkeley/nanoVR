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
                    Instantiate(nucleotide,
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
        SaveGameObject(cylinder);

        return cylinder;
    }

    public static GameObject MakeXover(GameObject prevGO, GameObject nextGO, int strandId)
    {
        GameObject xover =
                   Instantiate(Xover,
                   Vector3.zero,
                   Quaternion.identity) as GameObject;
        xover.name = "xover";
        var xoverComp = xover.GetComponent<XoverComponent>();
        xoverComp.StrandId = strandId;
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
        xoverComp.Length = dist;
        xoverComp.Color = prevGO.GetComponent<NucleotideComponent>().Color;
        prevGO.GetComponent<NucleotideComponent>().Xover = xover;
        nextGO.GetComponent<NucleotideComponent>().Xover = xover;
        xoverComp.PrevGO = prevGO;
        xoverComp.NextGO = nextGO;
        SaveGameObject(xover);

        return xover;
    }

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

        GameObject gridCircle = Instantiate(GridCircle,
                   gamePosition,
                   Quaternion.identity) as GameObject;

        // Calculate rotation
        if (plane.Equals("XZ"))
        {
            gridCircle.transform.Rotate(0f, 0f, 90f, 0);
        }
        else if (plane.Equals("YZ"))
        {
            gridCircle.transform.Rotate(0f, 90f, 0f, 0);
        }
       
        gridCircle.name = "gridPoint";
        GridComponent gridComponent = gridCircle.GetComponent<GridComponent>();
        gridComponent.Position = gamePosition;
        SaveGameObject(gridCircle);

        return gridCircle;
    }

    public static GameObject MakeBezier(List<GameObject> nucleotides, Color32 color)
    {
        GameObject tube = new GameObject("tube");
        var tubeRend = tube.AddComponent<TubeRenderer>();
        var meshRend = tube.GetComponent<MeshRenderer>();
        tubeRend.radius = 0.01f;
        tubeRend.points = new Vector3[nucleotides.Count];
        meshRend.material.SetColor("_Color", color);
        for (int i = 0; i < nucleotides.Count; i += 1)
        {
            tubeRend.points[i] = nucleotides[i].transform.position;
        }
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
}

