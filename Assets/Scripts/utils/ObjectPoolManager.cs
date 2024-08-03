/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> nucleotidePool = new List<GameObject>();
    //[SerializeField] private List<GameObject> backbonePool = new List<GameObject>();

    public static ObjectPoolManager Instance;
    private const int NUCLEOTIDE_TOTAL = 16384; // 2^14 = 16384
    private int numNucleotidesUsed = 0;
    private int numBackbonesUsed = 0;

    void Awake()
    {
        Instance = this;
    }

    public bool CanGetNucleotides(int length)
    {
        return NUCLEOTIDE_TOTAL - numNucleotidesUsed >= length;
    }

    public bool CanGetBackbones(int length) {
        return NUCLEOTIDE_TOTAL - numBackbonesUsed >= length;
    }


    public List<GameObject> GetNucleotides(int length)
    {
        if (CanGetNucleotides(length))
        {
            List<GameObject> gos = nucleotidePool.GetRange(numNucleotidesUsed, length);
            List<GameObject> nucleotides = new List<GameObject>();
            foreach (GameObject go in gos)
            {
                GameObject nucleotide = go.transform.GetChild(0).gameObject;
                nucleotides.Add(nucleotide);
            }
            numNucleotidesUsed += length;
            return nucleotides;
        }
        return null;
    }

    public List<GameObject> GetBackbones(int length)
    {
        if (CanGetBackbones(length))
        {
            List<GameObject> gos = nucleotidePool.GetRange(numBackbonesUsed, length);
            List<GameObject> backbones = new List<GameObject>();
            foreach (GameObject go in gos)
            {
                Transform transform = go.transform;
                int childCount = transform.childCount;

                // If there's only 1 child left, it must be the backbone at index 0.
                // Else, the nucleotide will be at index 0 and the backbone will be at index 1.
                if (childCount == 1)
                {
                    backbones.Add(transform.GetChild(0).gameObject);
                }
                else
                {
                    backbones.Add(transform.GetChild(1).gameObject);
                }
            }
            numBackbonesUsed += length;
            return backbones;
        }
        return null;
    }
}
