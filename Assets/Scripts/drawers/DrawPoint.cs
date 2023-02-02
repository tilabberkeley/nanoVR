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
  public static GameObject MakeNucleotide(Vector3 position, string name)
  {
    // make sphere
    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    sphere.name = name;
    sphere.transform.position = position;
    sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);


    sphere.AddComponent<NucleotideComponent>();
    var sphereNucleotide = sphere.GetComponent<NucleotideComponent>();
    sphereNucleotide.SetId(Int32.Parse(name.Substring(11)));

    sphere.AddComponent<XRSimpleInteractable>();
    //var sphereXRGrab = sphere.GetComponent<XRGrabInteractable>();
    // sphereXRGrab.throwOnDetach = false;

    sphere.AddComponent<Rigidbody>();
    var sphereRigidbody = sphere.GetComponent<Rigidbody>();
    sphereRigidbody.useGravity = false;
    sphereRigidbody.isKinematic = true;

    // TEST THIS
    // Material mat = new Material(Shader.Find("Standard"));
    // mat.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 0.5f));
    // mat.SetFloat("_Mode", 3);
    // mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    // mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    // mat.SetInt("_ZWrite", 0);
    // mat.EnableKeyword("_ALPHABLEND_ON");
    // mat.renderQueue = 3000;
    sphere.AddComponent<Renderer>();
    var sphereRenderer = sphere.GetComponent<Renderer>();
    sphereRenderer.material.SetColor("_Color", Color.gray);
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
