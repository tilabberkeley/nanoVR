/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

[System.Serializable]
public class NucleotideData
{
    private int id;
    private int helixId;
    private int strandId = -1;
    private int direction;
    private string sequence = "";
    private Color color;
    private int insertion = 0;
    private bool deletion = false;

    public int Id 

    public NucleotideData(int id, int helixId, int direction)
    {
        Id = id;
        HelixId = helixId;
        Direction = direction;
    }
}
