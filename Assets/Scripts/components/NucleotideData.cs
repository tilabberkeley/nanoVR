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
    private string sequence = "?";
    private Color color = Color.white;
    private Color highlight = Color.white;
    private int insertion = 0;
    private bool isDeletion = false;

    private XoverComponent xover = null; // Gameobject of xover or loopout attached to this nucleotide. Null if there isn't a xover or loopout.
    private int domainIdx = -1;          // Index of the domain within Strand's domain list
    private bool isHighlighted = false;
    private bool inExtension = false;

    private int oxViewId = -1;

    // This is used to save the position of the nucleotide before simulations.
    // Also used for oxDNA nucleotides since they don't have helices to grab position from.
    private Vector3 savedPosition;

    public int Id { get => id; }
    public int HelixId { get => helixId; }
    public int StrandId { get => strandId; set => strandId = value; }
    public int Direction { get => direction; }
    public string Sequence { get => sequence; set => sequence = value; }
    public Color Color 
    {   get => color; 
        set 
        {
            color = value;
            if (helixId != -1)
            {
                Helix helix = GetHelix();
                if (direction == 1)
                {
                    helix.NucleotideColorAChanged = true;
                    helix.NucleotideHighlightAChanged = true;
                    helix.BackboneColorAChanged = true;
                }
                else
                {
                    helix.NucleotideColorBChanged = true;
                    helix.NucleotideHighlightBChanged = true;
                    helix.BackboneColorBChanged = true;
                }
            }
        } 
    }
    public int Insertion 
    { 
        get => insertion;
        set 
        { 
            insertion = value; 
        } 
    }
    public bool IsInsertion { get => insertion > 0; }
    public bool IsDeletion 
    { 
        get => isDeletion;
        set 
        { 
            isDeletion = value; 

        }
    }
    public XoverComponent Xover { get => xover; set => xover = value; }
    public bool HasXover { get => xover != null; }
    public int DomainIdx { get => domainIdx; set => domainIdx = value; }

    public bool IsHighlighted { get => isHighlighted; set { isHighlighted = value; } }
    public Color Highlight
    {
        get
        {
            if (isHighlighted)
            {
                //Debug.Log("Returning highlight color");
                return highlight;
            }
                
            else
                return color; // default to color if not highlighted so that result is consistent
        }
        set 
        {
            highlight = value;
            Helix helix = GetHelix();
            if (direction == 1)
            {
                helix.NucleotideHighlightAChanged = true;
            }
            else
            {
                helix.NucleotideHighlightBChanged = true;
            }
        }
    }

    public bool InExtension { get => inExtension; set => inExtension = value; }
    public int OxViewId { get => oxViewId; set => oxViewId = value; }
    public Vector3 SavedPosition { get => savedPosition; set => savedPosition = value; }

    public NucleotideData(int id, int helixId, int direction, bool inExtension = false)
    {
        this.id = id;
        this.helixId = helixId;
        this.direction = direction;
        this.inExtension = inExtension;
    }

    public Matrix4x4 GetMatrix()
    {
        if (inExtension)
        {
            return GetDomain().GetNucleotideMesh(id);
        }
        if (oxViewId != -1)
        {
            return Matrix4x4.identity;
        }
        Helix helix = GlobalVariables.s_helixDict[helixId];
        return helix.GetNucleotideMesh(id, direction);
    }

    public Vector3 GetPosition()
    {
        if (oxViewId != -1)
        {
            return savedPosition;
        }
        return (GetHelix().GetCurrentOffset() * GetMatrix()).MultiplyPoint3x4(Vector3.zero);
    }

    public Helix GetHelix()
    {
        if (helixId == -1)
        {
            return null;
        }
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
        if (helixId == -1)
        {
            return false; // No helix associated
        }
        Helix helix = GetHelix();
        return id == helix.Length - 1;
    }

    public override string ToString()
    {
        return string.Format("id={0}, helix={1}, strand={2}, dir={3}", id, helixId, strandId, direction);
    }

    public NucleotideData GetComplement()
    {
        if (helixId == -1)
        {
            return null; // No helix associated
        }
        Helix helix = GetHelix();
        int complementDirection = 1 - direction;
        return helix.GetNucleotideData(id, complementDirection);
    }

    public void UpdatePosition(Vector3 newPos)
    {
        if (oxViewId != -1)
        {
            OxView oxView = GlobalVariables.s_oxViewDict[oxViewId];
            Matrix4x4 mat = oxView.Nucleotides[id];
            mat.SetColumn(3, new Vector4(newPos.x, newPos.y, newPos.z, 1f));
            oxView.Nucleotides[id] = mat;
        }
        else
        {
            Helix helix = GetHelix();
            helix.UpdatePosition(id, direction, newPos);
        }
    }

    public void Reset()
    {
        if (xover != null)
        {
            DrawCrossover.DeleteXover(xover);
        }

        strandId = -1;
        sequence = "?";
        Color = Color.white;
        Highlight = Color.white;
        insertion = 0;
        isDeletion = false;

        xover = null; // Gameobject of xover or loopout attached to this nucleotide. Null if there isn't a xover or loopout.
        domainIdx = -1;          // Index of the domain within Strand's domain list
        isHighlighted = false;
        inExtension = false;

        oxViewId = -1;
    }

    public bool IsHead()
    {
        return this.id == GetStrand().GetHead().Id;
    }

    public bool IsTail()
    {
        return this.id == GetStrand().GetTail().Id;
    }
}
