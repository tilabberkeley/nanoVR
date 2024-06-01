/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using UnityEngine;
using static GlobalVariables;

/// <summary>
/// Abstract class for DNA buildings blocks - nucleotides and backbones.
/// </summary>
public abstract class DNAComponent : MonoBehaviour
{
    public static Color s_defaultColor = Color.white;

    // Components
    protected Renderer _ntRenderer;
    protected MeshFilter _meshFilter;
    protected MaterialPropertyBlock _mpb;
    protected Outline _outline;

    // Color of this DNA Component.
    private Color _color = s_defaultColor;
    public Color Color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            _mpb.SetColor("_Color", _color);
            _ntRenderer.SetPropertyBlock(_mpb);
            //_ntRenderer.sharedMaterial.SetColor("_Color", value);
        }
    }

    // Whether this DNA Component is apart of a strand.
    protected bool _selected = false;
    public bool Selected { get { return _selected; } set { _selected = value; } }

    // Id of the DNA Component.
    protected int _id;
    public int Id { get { return _id; } set { _id = value; } }

    // Helix id of the helix this DNA Component is apart of.
    protected int _helixId;
    public int HelixId { get { return _helixId; } set { _helixId = value; } }

    // Strand id of the strand this DNA Component is apart of. Default as -1, meaning not apart of strand.
    protected int _strandId = -1;
    public int StrandId { get { return _strandId; } set { _strandId = value; } }

    public string GridId { get { return s_helixDict[_helixId].GridId; } }

    // Direction of this DNA Component. 0 = 5' to 3' left<-right, 1 = left->right
    protected int _direction;
    public int Direction { get { return _direction; } set { _direction = value; } }

    // Whether DNA Component is a backbone.
    protected bool _isBackbone;
    public bool IsBackbone { get { return _isBackbone; } set { _isBackbone = value; } }

    public GameObject Complement
    {
        get
        {
            Helix helix = s_helixDict[HelixId];
            if (!_isBackbone)
            {
                return helix.GetNucleotide(Id, 1 - Direction);
            }
            return helix.GetBackbone(Id, 1 - Direction);
        }
    }

    protected DomainComponent _domain = null;
    public DomainComponent Domain { get => _domain; set => _domain = value; }

    public virtual void ResetComponent()
    {
        _selected = false;
        _strandId = -1;
        Color = s_defaultColor;
        if (_domain != null)
        {
            GameObject.Destroy(_domain.gameObject);
        }
        _domain = null;
    }

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        _ntRenderer = GetComponent<Renderer>();
        _meshFilter = GetComponent<MeshFilter>();
        _mpb = new MaterialPropertyBlock();
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }

    /// <summary>
    /// Returns whether this DNA component is apart of a strand.
    /// </summary>
    /// <returns>True if this DNA component is apart of a strand. False otherwise.</returns>
    public bool IsInStrand()
    {
        return _strandId != -1;
    }
}
