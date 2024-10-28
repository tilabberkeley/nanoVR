/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> nucl64Pool = new List<GameObject>();
    [SerializeField] private List<GameObject> back63Pool = new List<GameObject>();
   

    public static ObjectPoolManager Instance;

    private const int PREFAB_TOTAL = 1024;

    private const int NUCLEOTIDE_PREFAB_SIZE = 32;
    private const int BACKBONE_PREFAB_SIZE = 31;
    private const int NUCLEOTIDE_TOTAL = PREFAB_TOTAL * NUCLEOTIDE_PREFAB_SIZE;
    private const int BACKBONE_TOTAL = PREFAB_TOTAL * BACKBONE_PREFAB_SIZE;
    private int numNuclPrefabsUsed = 0;
    private int numNuclChildrenUsed = 0;
    private int numBackPrefabsUsed = 0;
    private int numBackChildrenUsed = 0;

    void Awake()
    {
        Instance = this;
    }

    public bool CanGetNucleotides(int length)
    {
        return NUCLEOTIDE_TOTAL - numNuclPrefabsUsed * NUCLEOTIDE_PREFAB_SIZE - numNuclChildrenUsed >= length; 
    }

    public bool CanGetBackbones(int length)
    {
        return BACKBONE_TOTAL - numBackPrefabsUsed * BACKBONE_PREFAB_SIZE - numNuclChildrenUsed >= length;
    }

    public List<GameObject> GetNucleotides(int length)
    {
        List<GameObject> nucls = new List<GameObject>();
        if (numNuclChildrenUsed > 0)
        {
            Transform transform = nucl64Pool[numNuclPrefabsUsed].transform;
            int limit = Mathf.Min(transform.childCount, length);
            for (int i = 0; i < limit; i++)
            {
                GameObject nucl = transform.GetChild(0).gameObject; // Always get 0th child since we are removing children in the following line of code.
                nucl.transform.SetParent(null);
                nucls.Add(nucl);
            }
            length -= limit;
            numNuclChildrenUsed = 0;
            numNuclPrefabsUsed += 1;
        }
        int numPrefabsNeeded = length / NUCLEOTIDE_PREFAB_SIZE;
        int numChildrenNeeded = length % NUCLEOTIDE_PREFAB_SIZE;
      
        List<GameObject> gos = nucl64Pool.GetRange(numNuclPrefabsUsed, numPrefabsNeeded);
        foreach (GameObject go in gos)
        {
            Transform transform = go.transform;
            for (int i = 0; i < NUCLEOTIDE_PREFAB_SIZE; i++)
            {
                GameObject nucl = transform.GetChild(0).gameObject;
                nucl.transform.SetParent(null);
                nucls.Add(nucl);
            }
        }
        numNuclPrefabsUsed += numPrefabsNeeded;
        if (numChildrenNeeded > 0)
        {
            Transform transform = nucl64Pool[numNuclPrefabsUsed].transform;
            for (int i = 0; i < numChildrenNeeded; i++)
            {
                GameObject nucl = transform.GetChild(0).gameObject;
                nucl.transform.SetParent(null);
                nucls.Add(nucl);
            }
            numNuclChildrenUsed += numChildrenNeeded;
        }
        return nucls;  
    }

    public List<GameObject> GetBackbones(int length)
    {
        List<GameObject> backbones = new List<GameObject>();
        if (numBackChildrenUsed > 0)
        {
            Transform transform = back63Pool[numBackPrefabsUsed].transform;
            int limit = Mathf.Min(transform.childCount, length);

            for (int i = 0; i < limit; i++)
            {
                GameObject backbone = transform.GetChild(0).gameObject; // Always get 0th child since we are removing children in the following line of code.
                backbone.transform.SetParent(null);
                backbones.Add(backbone);
            }
            length -= limit;
            numBackChildrenUsed = 0;
            numBackPrefabsUsed += 1;
        }
        int numPrefabsNeeded = length / BACKBONE_PREFAB_SIZE;
        int numChildrenNeeded = length % BACKBONE_PREFAB_SIZE;
     
        List<GameObject> gos = back63Pool.GetRange(numBackPrefabsUsed, numPrefabsNeeded);
        foreach (GameObject go in gos)
        {
            Transform transform = go.transform;
            for (int i = 0; i < BACKBONE_PREFAB_SIZE; i++)
            {
                GameObject backbone = transform.GetChild(0).gameObject;
                backbone.transform.SetParent(null);
                backbones.Add(backbone);
            }
        }
        numBackPrefabsUsed += numPrefabsNeeded;
        if (numChildrenNeeded > 0)
        {
            Transform transform = back63Pool[numBackPrefabsUsed].transform;
            for (int i = 0; i < numChildrenNeeded; i++)
            {
                GameObject backbone = transform.GetChild(0).gameObject;
                backbone.transform.SetParent(null);
                backbones.Add(backbone);
            }
            numBackChildrenUsed += numChildrenNeeded;
        }
        return backbones;
    }
}
