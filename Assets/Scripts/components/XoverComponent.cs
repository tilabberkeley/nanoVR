/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System;
using UnityEngine;

/// <summary>
/// Component attached to each xover gameobject. Handles direct Ray interactions and gameobject visuals.
/// </summary>
public class XoverComponent : MonoBehaviour
{

    // Components
    private Renderer _ntRenderer;
    private Outline _outline;

    public static Color s_defaultColor = Color.white;
    private int _strandId = -1;
    public int StrandId { get { return _strandId; } set { _strandId = value; } }

    private double _length;
    public double Length { get { return _length; } set { _length = value; } }

    // Start nucleotide of xover. Direction of strand should go from _nextGO -> _prevGO
    private GameObject _prevGO = null;
    public GameObject PrevGO { get { return _prevGO; } set { _prevGO = value; } }

    // End nucleotide of xover.
    private GameObject _nextGO = null;
    public GameObject NextGO { get { return _nextGO; } set { _nextGO = value; } }

    protected Color _color = s_defaultColor;
    public virtual Color Color
    {
        get
        {
            return _color;
        }
        set
        {
            if (_length <= 0.05)
            {
                _color = value;
            }
            else if (_length <= 0.07)
            {
                _color = Color.gray;
            }
            else
            {
                _color = Color.black;
            }
            _ntRenderer.material.SetColor("_Color", _color);
        }
    }

    private Color _savedColor;
    public Color SavedColor { get { return _savedColor; } set { _savedColor = value; } }

    protected virtual void Update()
    {
        // Dynamically update xover gameobject when its prev and next gameobjects move
        if ((_prevGO != null && _prevGO.transform.hasChanged)
            || _nextGO != null && _nextGO.transform.hasChanged)
        {
            _prevGO.transform.hasChanged = false;
            _nextGO.transform.hasChanged = false;

            Vector3 start = _prevGO.transform.position;
            Vector3 end = _nextGO.transform.position;

            // Scale        
            float dist = Vector3.Distance(end, start);
            transform.localScale = new Vector3(0.005f, dist / 2, 0.005f);

            // Position
            transform.position = (end + start) / 2.0F;

            // Rotation
            transform.up = end - start;
            Color = Utils.GetStrand(_prevGO).Color;
        }
    }

    // Strand id of the strand that was merged with the first strand
    private int _prevStrandId;
    public int PrevStrandId { get { return _prevStrandId; } set { _prevStrandId = value; } }

    void Awake()
    {
        _ntRenderer = gameObject.GetComponent<Renderer>();
        _outline = gameObject.GetComponent<Outline>();
        _outline.enabled = false;
    }
}
