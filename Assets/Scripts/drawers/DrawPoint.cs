/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Creates needed gameobjects like nucleotides, grid circles, etc.
/// </summary>
public class DrawPoint : MonoBehaviour
{
    public GameObject nucleotidePrefab;
    public GameObject conePrefab;
   
    public GameObject MakeNucleotide(Vector3 position, int id, int helixId, int ssDirection)
    {
        // make sphere
        GameObject sphere =
                    Instantiate(Resources.Load("Nucleotide"),
                    position,
                    Quaternion.identity) as GameObject;        
        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.AddComponent<NucleotideComponent>();
        //sphere.AddComponent<XRSimpleInteractable>();
        //sphere.transform.position = position;
        //sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        sphere.name = "nucleotide" + id;

        var ntc = sphere.GetComponent<NucleotideComponent>();
        ntc.SetId(id);
        ntc.SetHelixId(helixId);
        ntc.SetDirection(ssDirection);
        // TEST THIS
        // Material mat = new Material(Shader.Find("Standard"));
        // mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        // mat.SetFloat("_Mode", 3);
        // mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        // mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // mat.SetInt("_ZWrite", 0);
        // mat.EnableKeyword("_ALPHABLEND_ON");
        // mat.renderQueue = 3000;
        return sphere;
    }

    public GameObject MakeCone(Vector3 position, int direction)
    {
        GameObject cone = Instantiate(Resources.Load("Cone"),
                                    position + new Vector3(0.015f, 0, 0),
                                    Quaternion.identity) as GameObject;
        if (direction == 0)
        {
            cone.transform.Rotate(90f, 0, 0, 0);
        }
        else
        {
            cone.transform.Rotate(-90f, 0, 0, 0);
        }
        return cone;
    }

    public static GameObject MakeXover(Vector3 startPos, Vector3 endPos)
    {
        GameObject xover = GameObject.CreatePrimitive(PrimitiveType.Cylinder);


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
        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        circle.name = name;
        circle.transform.position = position;
        circle.transform.localScale = new Vector3(0.1f, 0.0001f, 0.1f);
        circle.transform.Rotate(90f, 0f, 0f, 0);

        circle.AddComponent<XRSimpleInteractable>();

        circle.AddComponent<Rigidbody>();
        var sphereRigidbody = circle.GetComponent<Rigidbody>();
        sphereRigidbody.useGravity = false;
        sphereRigidbody.isKinematic = true;

        circle.AddComponent<Renderer>();
        var sphereRenderer = circle.GetComponent<Renderer>();
        sphereRenderer.material.SetColor("_Color", Color.gray);

        return circle;
    }
}

