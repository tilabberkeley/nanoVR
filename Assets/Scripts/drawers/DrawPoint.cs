/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.Object;

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
    public static GameObject MakeNucleotide(Vector3 position, int id, int helixId, int ssDirection)
    {
        GameObject sphere =
                    Instantiate(Resources.Load("Nucleotide"),
                    position,
                    Quaternion.identity) as GameObject;        
        sphere.name = "nucleotide" + id;

        var ntc = sphere.GetComponent<NucleotideComponent>();
        ntc.Id = id;
        ntc.HelixId = helixId;
        ntc.Direction = ssDirection;
        return sphere;
    }

    public static GameObject MakeCone(Vector3 position)
    {
        GameObject cone = Instantiate(Resources.Load("Cone"),
                                    position + new Vector3(0.015f, 0, 0),
                                    Quaternion.identity) as GameObject;
        return cone;
    }

    /// <summary>
    /// Creates a backbone between given two nucleotides.
    /// </summary>
    /// <param name="id">id of backbone.</param>
    /// <param name="start">Position of start nucleotide.</param>
    /// <param name="end">Position of end nucleotide.</param>
    /// <returns>GameObject of backbone.</returns>
    public static GameObject MakeBackbone(int id, Vector3 start, Vector3 end)
    {
        GameObject cylinder = Instantiate(Resources.Load("Backbone"),
                                    Vector3.zero,
                                    Quaternion.identity) as GameObject;
        cylinder.name = "Backbone" + id;
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
        var xoverComp = xover.GetComponent<XoverComponent>();
        xoverComp.SetStrandId(strandId);
        

        
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
        xoverComp.SetLength(dist);
        
        return xover;
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

    public static GameObject MakeGrid(Vector3 position, string name)
    {
        GameObject gridCircle = Instantiate(Resources.Load("GridCircle"),
                   position,
                   Quaternion.identity) as GameObject;
        gridCircle.name = name;
        gridCircle.transform.Rotate(90f, 0f, 0f, 0);


        //var collider = circle.GetComponent<SphereCollider>();
        //collider.radius = 0.3f;

        return gridCircle;
    }
}

