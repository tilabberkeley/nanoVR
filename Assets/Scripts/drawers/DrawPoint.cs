using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

 
public class DrawPoint : MonoBehaviour
{

    [SerializeField] GameObject nucleotidePrefab;
    public GameObject MakeNucleotide(Vector3 position, int id, int helixId, int ssDirection)
    {
        // make sphere
        GameObject sphere = Instantiate(nucleotidePrefab, position, Quaternion.identity);
        sphere.name = "nucleotide" + id;
       

        var ntc = sphere.GetComponent<NucleotideComponent>();
        var helixC = sphere.GetComponent<HelixComponent>();
        ntc.SetId(id);
        helixC.SetHelixId(helixId);
        helixC.SetSSId(ssDirection);
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

    public static GameObject MakeSphere(Vector3 position, string name)
    {
        // make sphere
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.position = position;
        sphere.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);

        sphere.AddComponent<XRGrabInteractable>();

        sphere.AddComponent<Rigidbody>();
        var sphereRigidbody = sphere.GetComponent<Rigidbody>();
        sphereRigidbody.useGravity = false;
        sphereRigidbody.isKinematic = true;


        sphere.AddComponent<Renderer>();
        var sphereRenderer = sphere.GetComponent<Renderer>();
        sphereRenderer.material.SetColor("_Color", Color.gray);

        return sphere;
    }

    public static GameObject MakeGrid(Vector3 position, string name)
    {

        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        circle.name = name;
        circle.transform.position = position;
        circle.transform.localScale = new Vector3(0.05f, 0.0001f, 0.05f);
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

