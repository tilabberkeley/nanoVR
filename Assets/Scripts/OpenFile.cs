using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class OpenFile : MonoBehaviour
{
    string path;
    string sequence;

    public void OpenFileExplorer() {
        path = EditorUtility.OpenFilePanel("Show all text files (.txt) ", "", "txt");
        sequence = GetText();
        RenderModel(sequence);
        
    }

    public string GetText() {
        if (path != null) {
            WWW www = new WWW("file:///" + path);
            return www.text;
        }
        else {
            return null;
        }
    }

    void RenderModel(string s) {
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
            else if (s[i] == 'C') {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(i, 5, 0);
                var sphereRenderer = sphere.GetComponent<Renderer>();
                sphereRenderer.material.SetColor("_Color", Color.green);
            }
            else if (s[i] == 'G') {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(i, 5, 0);
                var sphereRenderer = sphere.GetComponent<Renderer>();
                sphereRenderer.material.SetColor("_Color", Color.yellow);
            }
        }
    }
   
}
