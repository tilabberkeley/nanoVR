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

    /*public bool CanGetNucleotides(int count, NucleotideSize size)
    {
        switch (size)
        {
            case NucleotideSize.LENGTH_64:
                return NUCLEOTIDE_TOTAL - numNucl64Used >= count;
            case NucleotideSize.LENGTH_32:
                return NUCLEOTIDE_TOTAL - numNucl32Used >= count;
            case NucleotideSize.LENGTH_16:
                return NUCLEOTIDE_TOTAL - numNucl16Used >= count;
            case NucleotideSize.LENGTH_8:
                return NUCLEOTIDE_TOTAL - numNucl8Used >= count;
            case NucleotideSize.LENGTH_4:
                return NUCLEOTIDE_TOTAL - numNucl4Used >= count;
            case NucleotideSize.LENGTH_2:
                return NUCLEOTIDE_TOTAL - numNucl2Used >= count;
            case NucleotideSize.LENGTH_1:
                return NUCLEOTIDE_TOTAL - numNucl1Used >= count;
        }
        return false;
    }

    public List<GameObject> GetNucleotidesFromPool(int count, NucleotideSize size)
    { 
        switch (size)
        {
            case NucleotideSize.LENGTH_64:
                return nucl64Pool.GetRange(numNucl64Used, count);
            case NucleotideSize.LENGTH_32:
                return nucl32Pool.GetRange(numNucl32Used, count);
            case NucleotideSize.LENGTH_16:
                return nucl16Pool.GetRange(numNucl16Used, count);
            case NucleotideSize.LENGTH_8:
                return nucl8Pool.GetRange(numNucl8Used, count);
            case NucleotideSize.LENGTH_4:
                return nucl4Pool.GetRange(numNucl4Used, count);
            case NucleotideSize.LENGTH_2:
                return nucl2Pool.GetRange(numNucl2Used, count);
            case NucleotideSize.LENGTH_1:
                return nucl1Pool.GetRange(numNucl1Used, count);
        }
        return null;
    }

    private void UpdateNuclUsed(int count, NucleotideSize size) {
        switch (size)
        {
            case NucleotideSize.LENGTH_64:
                numNucl64Used += count;
                break;
            case NucleotideSize.LENGTH_32:
                numNucl32Used += count;
                break;
            case NucleotideSize.LENGTH_16:
                numNucl16Used += count;
                break;
            case NucleotideSize.LENGTH_8:
                numNucl8Used += count;
                break;
            case NucleotideSize.LENGTH_4:
                numNucl4Used += count;
                break;
            case NucleotideSize.LENGTH_2:
                numNucl2Used += count;
                break;
            case NucleotideSize.LENGTH_1:
                numNucl1Used += count;
                break;
        }
    }


    public bool CanGetBackbones(int length, BackboneSize size) {
        switch (size)
        {
            case BackboneSize.LENGTH_63:
                return NUCLEOTIDE_TOTAL - numBack63Used >= length;
            case BackboneSize.LENGTH_31:
                return NUCLEOTIDE_TOTAL - numBack31Used >= length;
            case BackboneSize.LENGTH_15:
                return NUCLEOTIDE_TOTAL - numBack15Used >= length;
            case BackboneSize.LENGTH_7:
                return NUCLEOTIDE_TOTAL - numBack7Used >= length;
            case BackboneSize.LENGTH_3:
                return NUCLEOTIDE_TOTAL - numBack3Used >= length;          
            case BackboneSize.LENGTH_1:
                return NUCLEOTIDE_TOTAL - numBack1Used >= length;
        }
        return false;
    }

    public List<GameObject> GetBackbonesFromPool(int count, BackboneSize size)
    {
        switch (size)
        {
            case BackboneSize.LENGTH_63:
                return back63Pool.GetRange(numBack63Used, count);
            case BackboneSize.LENGTH_31:
                return back31Pool.GetRange(numBack31Used, count);
            case BackboneSize.LENGTH_15:
                return back15Pool.GetRange(numBack15Used, count);
            case BackboneSize.LENGTH_7:
                return back7Pool.GetRange(numBack7Used, count);
            case BackboneSize.LENGTH_3:
                return back3Pool.GetRange(numBack3Used, count);
            case BackboneSize.LENGTH_1:
                return back1Pool.GetRange(numBack1Used, count);
        }
        return null;
    }

    private void UpdateBackUsed(int count, BackboneSize size)
    {
        switch (size)
        {
            case BackboneSize.LENGTH_63:
                numBack63Used += count;
                break;
            case BackboneSize.LENGTH_31:
                numBack31Used += count;
                break;
            case BackboneSize.LENGTH_15:
                numBack15Used += count;
                break;
            case BackboneSize.LENGTH_7:
                numBack7Used += count;
                break;
            case BackboneSize.LENGTH_3:
                numBack3Used += count;
                break;
            case BackboneSize.LENGTH_1:
                numBack1Used += count;
                break;
        }
    }


    public List<GameObject> GetNucleotides(int count, NucleotideSize size)
    {
        if (CanGetNucleotides(count, size))
        {
            List<GameObject> gos = GetNucleotidesFromPool(count, size);
            List<GameObject> nucleotides = new List<GameObject>();
            foreach (GameObject go in gos)
            {
                Transform transform = go.transform;
                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject nucleotide = transform.GetChild(i).gameObject;
                    nucleotides.Add(nucleotide);
                }
            }
            UpdateNuclUsed(count, size);
            return nucleotides;
        }
        return null;
    }

    public List<GameObject> GetBackbones(int count, BackboneSize size)
    {
        if (CanGetBackbones(count, size))
        {
            List<GameObject> gos = GetBackbonesFromPool(count, size);
            List<GameObject> backbones = new List<GameObject>();
            foreach (GameObject go in gos)
            {
                Transform transform = go.transform;
                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject backbone = transform.GetChild(i).gameObject;
                    backbones.Add(backbone);
                }
            }
            int numConnectingBackbones = count - 1;
            for (int i = 0; i < numConnectingBackbones; i++)
            {
                backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, false));
            }
            UpdateBackUsed(count, size);
            return backbones;
        }
        return null;
    }*/
}
