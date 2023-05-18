/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using UnityEngine.UI;

public class ObjectListManager : MonoBehaviour
{

    public GameObject content;
    public void CreateButton(int strandId)
    {
        GameObject button = Instantiate(Resources.Load("Button")) as GameObject;
        button.transform.SetParent(content.transform); 
        GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        button.GetComponentInChildren<Text>().text = "Strand " + strandId;
        cyl.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }

}
