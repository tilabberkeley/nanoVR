/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using UnityEngine;
using UnityEngine.UI;

public class ObjectListManager : MonoBehaviour
{

    public GameObject content;

    /// <summary>
    /// Creates button in Strand List UI whenever new strand is created. Button and Strand
    /// are linked by the strand Id.
    /// </summary>
    /// <param name="strandId">Id of strand which is also id of corresponding button.</param>
    public void CreateButton(int strandId)
    {
        GameObject button = Instantiate(Resources.Load("Button")) as GameObject;
        //button.transform.SetParent(content.transform, false);
        GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        button.GetComponentInChildren<Text>().text = "Strand " + strandId;
        button.name = "StrandButton" + strandId;
        button.GetComponent<Button>().onClick.AddListener(() => SelectStrand(Int32.Parse(button.name.Substring(12))));
        //cyl.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }

    // TEST
    public static void SelectStrand(int strandId)
    {
        SelectStrand ss = new SelectStrand();
        ss.HighlightStrand(strandId);
    }

    // TEST
    public void DeleteButton(int strandId)
    {
        string name = "StrandButton" + strandId;
        GameObject button = GameObject.Find(name);
        Destroy(button);
    }

}
