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
    private Color color = Color.white;
    private int insertion = 0;
    private bool isDeletion = false;

    private XoverComponent xover = null; // Gameobject of xover or loopout attached to this nucleotide. Null if there isn't a xover or loopout.
    private int domainIdx = -1;          // Index of the domain within Strand's domain list

    public int Id { get => id; }
    public int HelixId { get => helixId; }
    public int StrandId { get => strandId; set => strandId = value; }
    public int Direction { get => direction; }
    public string Sequence { get => sequence; set => sequence = value; }
    public Color Color { get => color; set => color = value; }
    public int Insertion { get => insertion; set => insertion = value; }
    public bool IsDeletion { get => isDeletion; set => isDeletion = value; }
    public XoverComponent Xover { get => xover; set => xover = value; }
    public bool HasXover { get => xover != null; }
    public int DomainIdx { get => domainIdx; set => domainIdx = value; }


    public NucleotideData(int id, int helixId, int direction)
    {
        this.id = id;
        this.helixId = helixId;
        this.direction = direction;
    }

    public Matrix4x4 GetMatrix()
    {
        Helix helix = GlobalVariables.s_helixDict[helixId];
        return helix.GetNucleotideMesh(id, direction);
    }

    public Vector3 GetPosition()
    {   
        return GetMatrix().GetColumn(3);
    }
}
