/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using UnityEngine.UI;
using static GlobalVariables;

public class ObjectListManager : MonoBehaviour
{

    // public GameObject content;

    /// <summary>
    /// Creates button in Strand List UI whenever new strand is created. Button and Strand
    /// are linked by the strand Id.
    /// </summary>
    /// <param name="strandId">Id of strand which is also id of corresponding button.</param>
    public static void CreateStrandButton(int strandId)
    {
        GameObject button = Instantiate(Resources.Load("Button")) as GameObject;
        button.transform.SetParent(GameObject.FindWithTag("StrandList").transform, false);
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Strand " + strandId;
        button.name = "StrandButton" + strandId;
        button.GetComponent<Button>().onClick.AddListener(() => SelStrand(strandId));
        button.transform.SetSiblingIndex(strandId);
    }

    public static void CreateGridButton(int gridId)
    {
        GameObject button = Instantiate(Resources.Load("Button")) as GameObject;
        button.transform.SetParent(GameObject.FindWithTag("GridList").transform, false);
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Grid " + gridId;
        button.name = "GridButton" + gridId;
        button.GetComponent<Button>().onClick.AddListener(() => SelGrid(gridId));
        button.transform.SetSiblingIndex(gridId);
    }

    public static void SelectAll()
    {
        foreach (int i in s_strandDict.Keys)
        {
            SelStrand(i);
        }
    }

    public static void SelStrand(int strandId)
    {
        SelectStrand.HighlightStrand(strandId);
    }

    public static void SelGrid(int gridId)
    {
        SelectGrid.HighlightGrid(gridId);
    }

    public static void DeleteStrandButton(int strandId)
    {
        string name = "StrandButton" + strandId;
        GameObject button = GameObject.Find(name);
        Destroy(button);
    }

}
