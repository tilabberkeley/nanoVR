/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Helix object keeps track of nucleotides in the helix.
/// </summary>
public class Helix
{
    //constants
    private const float OFFSET = 0.034f;
    private const float RISE = 0.034f;
    private const int LENGTH = 64;

    // Helix id.
    public int Id { get; set; }
    public Vector3 StartPoint { get; set; }
    public Vector3 EndPoint { get; set; }

    // Number of nucleotides in helix.
    public int Length { get; private set; }
    public string Orientation { get; set; }

    // Grid Component that helix is on.
    public GridComponent GridComponent { get; private set; }

    // List containing all nucleotides in spiral going in direction 1.
    public List<GameObject> NucleotidesA { get; private set; }

    // List containing all backbones in spiral going in direction 1.
    private List<GameObject> BackbonesA { get; set; }

    // List containing all nucleotides in spiral going in direction 0.
    public List<GameObject> NucleotidesB { get; private set; }

    // List containing all backbones in spiral going in direction 0.
    private List<GameObject> BackbonesB { get; set; }

    // List of strand ids created on helix.
    public List<int> StrandIds { get; set; }

    // Positions of last nucleotides in helix
    private Vector3 LastPositionA { get; set; }
    private Vector3 LastPositionB { get; set; }

    // Helix constructor.
    public Helix(int id, Vector3 startPoint, Vector3 endPoint, string orientation, int length, GridComponent gridComponent)
    {
        Id = id;
        StartPoint = startPoint;
        EndPoint = endPoint;
        Length = 0;
        Orientation = orientation;
        GridComponent = gridComponent;
        NucleotidesA = new List<GameObject>();
        BackbonesA = new List<GameObject>();
        NucleotidesB = new List<GameObject>();
        BackbonesB = new List<GameObject>();
        StrandIds = new List<int>();
        LastPositionA = GridComponent.Position - new Vector3(0, OFFSET, 0);
        LastPositionB = GridComponent.Position + new Vector3(0, OFFSET, 0);
        Extend(length);
        ChangeRendering();
    }

    /// <summary>
    /// Draws the nucleotides of the helix.
    /// </summary>
    public void Extend(int length)
    {
        int prevLength = Length;
        Length += length;
        for (int i = prevLength; i < Length; i++)
        {
            GameObject sphereA = DrawPoint.MakeNucleotide(LastPositionA, i, Id, 1);
            //sphereA.SetActive(false);
            NucleotidesA.Add(sphereA);

            GameObject sphereB = DrawPoint.MakeNucleotide(LastPositionB, i, Id, 0);
            //sphereB.SetActive(false);
            NucleotidesB.Add(sphereB);

            float angleA = i * (2 * (float)(Math.PI) / 10); // rotation per bp in radians
            float angleB = (float)(((float)(i) + 5.5) * (2 * (float)(Math.PI) / 10));
            float axisOneChangeA = Mathf.Cos(angleA) * 0.02f;
            float axisTwoChangeA = Mathf.Sin(angleA) * 0.02f;
            float axisOneChangeB = Mathf.Cos(angleB) * 0.02f;
            float axisTwoChangeB = Mathf.Sin(angleB) * 0.02f;

            LastPositionA += new Vector3(axisOneChangeA, axisTwoChangeA, -RISE);
            LastPositionB += new Vector3(axisOneChangeB, axisTwoChangeB, -RISE);
        }

        if (prevLength == 0) 
        { 
            DrawBackbones(prevLength + 1); 
        }
        else
        {
            // Needs to add backbone to connect previous set of nucleotides
            DrawBackbones(prevLength);
        }
    }

    /// <summary>
    /// Draws the backbones between the nucleotides in the helix.
    /// </summary>
    /// <param name="start">Start index of nucleotide to begin drawing backbones.</param>
    private void DrawBackbones(int start)
    {
        // Backbones for A nucleotides
        for (int i = start; i < NucleotidesA.Count; i++)
        {
            GameObject cylinder = DrawPoint.MakeBackbone(i - 1, Id, 1, NucleotidesA[i].transform.position, NucleotidesA[i - 1].transform.position);
            //cylinder.SetActive(false);
            BackbonesA.Add(cylinder);
        }

        // Backbones for B nucleotides
        for (int i = start; i < NucleotidesB.Count; i++)
        {
            GameObject cylinder = DrawPoint.MakeBackbone(i - 1, Id, 0, NucleotidesB[i].transform.position, NucleotidesB[i - 1].transform.position);
            //cylinder.SetActive(false);
            BackbonesB.Add(cylinder);
        }
    }

    /// <summary>
    /// Returns sublist of nucleotides and backbones from helix spiral.
    /// </summary>
    /// <param name="sIndex">Start index of sublist.</param>
    /// <param name="eIndex">End index of sublist.</param>
    /// <param name="direction">Spiral direction (determines nucleotidesA or nucleotidesB).</param>
    /// <returns>Returns sublist of nucleotides from helix spiral.</returns>
    public List<GameObject> GetHelixSub(int sIndex, int eIndex, int direction)
    {
        List<GameObject> temp = new List<GameObject>();
        if (direction == 0)
        {
            for (int i = sIndex; i < eIndex; i++)
            {
                temp.Add(NucleotidesB[i]);
                temp.Add(BackbonesB[i]);
            }
            temp.Add(NucleotidesB[eIndex]);
            return temp;
        }
        else 
        {
            for (int i = sIndex; i < eIndex; i++)
            {
                temp.Add(NucleotidesA[i]);
                temp.Add(BackbonesA[i]);
            }
            temp.Add(NucleotidesA[eIndex]);
            temp.Reverse();
            return temp;
        }
    }

    public GameObject GetNucleotide(int id, int direction)
    {
        if (direction == 0)
        {
            return NucleotidesB[id];
        }
        else
        {
            return NucleotidesA[id];
        }
    }

    public GameObject GetBackbone(int id, int direction)
    {
        if (direction == 0)
        {
            return BackbonesB[id];
        }
        else
        {
            return BackbonesA[id];
        }
    }
    /// <summary>
    /// Returns nucleotide in front of head nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of the helix, 0 or 1.</param>
    /// <returns>Returns nucleotide in front of head nucleotide.</returns>
    public GameObject GetHeadNeighbor(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = NucleotidesB.IndexOf(go);
            return NucleotidesB[index - 1];
        }
        else
        {
            int index = NucleotidesA.IndexOf(go);
            return NucleotidesA[index + 1];
        }
    }

    /// <summary>
    /// Returns nucleotide behind tail nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of helix, 0 or 1.</param>
    /// <returns>Returns nucleotide behind tail nucleotide.</returns>
    public GameObject GetTailNeighbor(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = NucleotidesB.IndexOf(go);
            return NucleotidesB[index + 1];
        }
        else
        {
            int index = NucleotidesA.IndexOf(go);
            return NucleotidesA[index - 1];
        }
    }

    /// <summary>
    /// Returns backbone in front of head nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of helix, 0 or 1.</param>
    /// <returns>Returns backbone in front of head nucleotide.</returns>
    public GameObject GetHeadBackbone(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = NucleotidesB.IndexOf(go);
            return BackbonesB[index - 1];
        }
        else
        {
            int index = NucleotidesA.IndexOf(go);
            return BackbonesA[index];
        }
    }

    /// <summary>
    /// Returns backbone in behind tail nucleotide.
    /// </summary>
    /// <param name="go">GameObject to find neighbor of.</param>
    /// <param name="direction">Direction of helix, 0 or 1.</param>
    /// <returns>Returns backbone in behind tail nucleotide.</returns>
    public GameObject GetTailBackbone(GameObject go, int direction)
    {
        if (direction == 0)
        {
            int index = NucleotidesB.IndexOf(go);
            return BackbonesB[index];
        }
        else
        {
            int index = NucleotidesA.IndexOf(go);
            return BackbonesA[index - 1];
        }
    }

    // Adds strandId to strand id list.
    public void AddStrandId(int strandId)
    {
        StrandIds.Add(strandId);
    }

    // Removes strandId from strand id list.
    public void DeleteStrandId(int strandId)
    {
        StrandIds.Remove(strandId);
    }

    // Returns true if none of the helix's nucleotides are selected.
    // In other words, if there are no strands on the helix.
    public bool IsEmpty()
    {
        return IsEmpty(NucleotidesA) && IsEmpty(NucleotidesB) && IsEmpty(BackbonesA) && IsEmpty(BackbonesB);
    }

    // Helper method for IsEmpty().
    public bool IsEmpty(List<GameObject> lst)
    {
        foreach (GameObject nucleotide in lst)
        {
            var ntc = nucleotide.GetComponent<NucleotideComponent>();
            if (ntc.Selected)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns the length of the helix.
    /// </summary>
    /// <returns>Lenght of the helix</returns>
    public int GetLength()
    {
        return Length;
    }

    public void Highlight(Color color)
    {
        for (int i = 0; i < NucleotidesA.Count; i++)
        {
            NucleotidesA[i].GetComponent<NucleotideComponent>().Highlight(color);
            NucleotidesB[i].GetComponent<NucleotideComponent>().Highlight(color);
        }
        for (int i = 0; i < BackbonesA.Count; i++)
        {
            BackbonesA[i].GetComponent<NucleotideComponent>().Highlight(color);
            BackbonesB[i].GetComponent<NucleotideComponent>().Highlight(color);
        }
        /*
        for (int i = 0; i < StrandIds.Count; i++)
        {
            s_strandDict[StrandIds[i]].HighlightCone(color);
        } */
    }

    /// <summary>
    /// Changes rendering of helix and its components (cones).
    /// </summary>
    public void ChangeRendering()
    {
        for (int i = 0; i < BackbonesA.Count; i++)
        {
            NucleotidesA[i].SetActive(s_nucleotideView);
            BackbonesA[i].SetActive(s_nucleotideView);
            NucleotidesB[i].SetActive(s_nucleotideView);
            BackbonesB[i].SetActive(s_nucleotideView);
        }
        NucleotidesA[NucleotidesA.Count - 1].SetActive(s_nucleotideView);
        NucleotidesB[NucleotidesB.Count - 1].SetActive(s_nucleotideView);
        /*
        for (int i = 0; i < StrandIds.Count; i++)
        {
            Strand strand = s_strandDict[StrandIds[i]];
            strand.ShowHideCone(s_nucleotideView);
        }   
        */
    }

    /// <summary>
    /// Returns the helices that neighbor this helix.
    /// </summary>
    /// <returns>List of neighboring helices.</returns>
    public List<Helix> getNeighborHelices()
    {
        List<Helix> helices = new List<Helix>();
        foreach (GridComponent gridComponent in GridComponent.getNeighborGridComponents())
        {
            Helix helix = gridComponent.Helix;
            // helix != null if there is a helix on the grid component
            if (helix != null)
            {
                helices.Add(helix);
            }
        }
        return helices;
    }

    public void DeleteHelix()
    {
        GridComponent.Helix = null;
        GridComponent.Selected = false;
        foreach (GameObject nucleotide in NucleotidesA)
        {
            GameObject.Destroy(nucleotide);
        }
        foreach (GameObject nucleotide in NucleotidesB)
        {
            GameObject.Destroy(nucleotide);
        }
        foreach (GameObject nucleotide in BackbonesA)
        {
            GameObject.Destroy(nucleotide);
        }
        foreach (GameObject nucleotide in BackbonesB)
        {
            GameObject.Destroy(nucleotide);
        }
    }
}

