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

    public static void CreateGridButton(string gridId)
    {
        GameObject button = Instantiate(Resources.Load("Button")) as GameObject;
        button.transform.SetParent(GameObject.FindWithTag("GridList").transform, false);
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Grid " + gridId;
        button.name = "GridButton" + gridId;
        button.GetComponent<Button>().onClick.AddListener(() => SelGrid(gridId));
        button.transform.SetSiblingIndex(s_numGrids);
    }

    public static void CreateSubGridButton(int subGridId)
    {
        GameObject button = Instantiate(Resources.Load("Button")) as GameObject;
        button.transform.SetParent(GameObject.FindWithTag("SubGridList").transform, false);
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "SubGrid " + subGridId;
        button.name = "SubGridButton" + subGridId;
        button.GetComponent<Button>().onClick.AddListener(() => SelSubGrid(subGridId));
        button.transform.SetSiblingIndex(subGridId);
    }

    /*public static void SelectAll()
    {
        foreach (int i in s_strandDict.Keys)
        {
            SelStrand(i);
        }
    }*/

    public static void SelStrand(int strandId)
    {
        SelectStrand.HighlightStrand(strandId);
        SelectStrand.AddStrand(strandId);
    }

    public static void SelGrid(string gridId)
    {
        //SelectGrid.HighlightGrid(gridId);
        SelectGrid.ShowGridCircles(gridId);
    }

    public static void SelSubGrid(int subGridId)
    {
    }

    public static void DeleteStrandButton(int strandId)
    {
        string name = "StrandButton" + strandId;
        GameObject button = GameObject.Find(name);
        Destroy(button);
    }

}
