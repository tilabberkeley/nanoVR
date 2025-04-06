/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

[System.Serializable]
public class NucleotideData
{
    private readonly int id;
    private readonly int helixId;
    private int strandId = -1;
    private readonly int direction;
    private string sequence = "";
    private Color color = Color.white;
    private Color highlight = Color.white;
    private int insertion = 0;
    private bool isDeletion = false;

    private XoverComponent xover = null; // Gameobject of xover or loopout attached to this nucleotide. Null if there isn't a xover or loopout.
    private int domainIdx = -1;          // Index of the domain within Strand's domain list
    private bool isHighlighted = false;

    public int Id { get => id; }
    public int HelixId { get => helixId; }
    public int StrandId { get => strandId; set => strandId = value; }
    public int Direction { get => direction; }
    public string Sequence { get => sequence; set => sequence = value; }
    public Color Color { get => color; set => color = value; }
    public int Insertion { get => insertion; set => insertion = value; }
    public bool IsInsertion { get => insertion > 0; }
    public bool IsDeletion { get => isDeletion; set => isDeletion = value; }
    public XoverComponent Xover { get => xover; set => xover = value; }
    public bool HasXover { get => xover != null; }
    public int DomainIdx { get => domainIdx; set => domainIdx = value; }

    public bool IsHighlighted { get => isHighlighted; set => isHighlighted = value; }
    public Color Highlight
    {
        get
        {
            if (isHighlighted)
                return highlight;
            else
                return color; // default to color if not highlighted so that result is consistent
        }
        set => highlight = value;
    }


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
        Matrix4x4 worldMat = GetHelix().TransformOffset * GetMatrix();
        return worldMat.GetColumn(3);
    }

    public Helix GetHelix()
    {
        return GlobalVariables.s_helixDict[helixId];
    }

    public Domain GetDomain()
    {
        if (strandId == -1 || domainIdx == -1)
        {
            return null;
        }

        Strand strand = GlobalVariables.s_strandDict[strandId];
        return strand.GetDomain(domainIdx);
    }

    public Strand GetStrand()
    {
        if (strandId == -1)
        {
            return null;
        }

        return GlobalVariables.s_strandDict[strandId];
    }

    public bool IsSelected()
    {
        return strandId != -1;
    }

    public bool IsHelixEnd()
    {
        Helix helix = GetHelix();
        return id == helix.Length - 1;
    }

    public override string ToString()
    {
        return string.Format("NucleotideData: id={0}, helixId={1}, strandId={2}, direction={3}",
            id, helixId, strandId, direction);
    }
}
