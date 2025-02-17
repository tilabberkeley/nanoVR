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

    public int Id { get => id; }
    public int HelixId { get => helixId; }
    public int StrandId { get => strandId; set => strandId = value; }
    public int Direction { get => direction; }
    public string Sequence { get => sequence; set => sequence = value; }
    public Color Color { get => color; set => color = value; }
    public int Insertion { get => insertion; set => insertion = value; }
    public bool Deletion { get => deletion; set => deletion = value; }


    public NucleotideData(int id, int helixId, int direction)
    {
        this.id = id;
        this.helixId = helixId;
        this.direction = direction;
    }
}
