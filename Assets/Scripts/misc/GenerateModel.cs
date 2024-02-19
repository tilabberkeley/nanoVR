using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateModel : MonoBehaviour
{
    public GameObject MenuCanvas;  

    public void Generate() 
    {
        CloseMenu();
        //RenderModel(GlobalVariables.s_sequence);
    }

    void CloseMenu() 
    {
        if (MenuCanvas != null) 
        {  
            bool isActive = MenuCanvas.activeSelf;  
            MenuCanvas.SetActive(!isActive);  
        }
    }

    void RenderModel(string s) 
    {
        for (int i = 0; i < s.Length; i++) {
            if (s[i] == 'A') {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(i, 5, 0);
                var sphereRenderer = sphere.GetComponent<Renderer>();
                sphereRenderer.material.SetColor("_Color", Color.red);
            }
            else if (s[i] == 'T') {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(i, 5, 0);
                var sphereRenderer = sphere.GetComponent<Renderer>();
                sphereRenderer.material.SetColor("_Color", Color.blue);
            }
            else if (s[i] == 'G') {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(i, 5, 0);
                var sphereRenderer = sphere.GetComponent<Renderer>();
                sphereRenderer.material.SetColor("_Color", Color.green);
            }
            else if (s[i] == 'C') {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(i, 5, 0);
                var sphereRenderer = sphere.GetComponent<Renderer>();
                sphereRenderer.material.SetColor("_Color", Color.yellow);
            }
        }
    }
}
