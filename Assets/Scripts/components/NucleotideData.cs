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

    private GameObject xover = null; // Gameobject of xover or loopout attached to this nucleotide. Null if there isn't a xover or loopout.

    public int Id { get => id; }
    public int HelixId { get => helixId; }
    public int StrandId { get => strandId; set => strandId = value; }
    public int Direction { get => direction; }
    public string Sequence { get => sequence; set => sequence = value; }
    public Color Color { get => color; set => color = value; }
    public int Insertion { get => insertion; set => insertion = value; }
    public bool Deletion { get => deletion; set => deletion = value; }
    public GameObject Xover { get => xover; set => xover = value; }
    public bool HasXover { get => xover != null; }


    public NucleotideData(int id, int helixId, int direction)
    {
        this.id = id;
        this.helixId = helixId;
        this.direction = direction;
    }
}
