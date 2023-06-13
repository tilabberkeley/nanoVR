/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using UnityEngine;
using UnityEngine.UI;

public class ObjectListManager : MonoBehaviour
{

    // public GameObject content;

    /// <summary>
    /// Creates button in Strand List UI whenever new strand is created. Button and Strand
    /// are linked by the strand Id.
    /// </summary>
    /// <param name="strandId">Id of strand which is also id of corresponding button.</param>
    public static void CreateButton(int strandId)
    {
        GameObject button = Instantiate(Resources.Load("Button")) as GameObject;
        button.transform.SetParent(GameObject.FindWithTag("StrandList").transform, false);
        button.GetComponentInChildren<Text>().text = "Strand " + strandId;
        button.name = "StrandButton" + strandId;
        button.GetComponent<Button>().onClick.AddListener(() => SelStrand(strandId));
    }

    // TEST
    public static void SelStrand(int strandId)
    {
        SelectStrand.HighlightStrand(strandId);
    }

    // TEST
    public static void DeleteButton(int strandId)
    {
        string name = "StrandButton" + strandId;
        GameObject button = GameObject.Find(name);
        Destroy(button);
    }

}
