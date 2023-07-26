/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;

/// <summary>
/// Component attached to each xover gameobject. Handles direct Ray interactions and gameobject visuals.
/// </summary>
public class XoverComponent : MonoBehaviour
{
    public static Color s_defaultColor = Color.white;
    private int _strandId = -1;
    public int StrandId { get { return _strandId; } set { _strandId = value; } }

    private double _length;
    public double Length { get { return _length; } set { _length = value; } }

    // Start GameObject of xover.
    private GameObject _prevGO = null;
    public GameObject PrevGO { get { return _prevGO; } set { _prevGO = value; } }

    // End GameObject of xover.
    private GameObject _nextGO = null;
    public GameObject NextGO { get { return _nextGO; } set { _nextGO = value; } }

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
            _ntRenderer.material.SetColor("_Color", value);
        }
    }

    // Components
    private Renderer _ntRenderer;
    private Outline _outline;

    public void Highlight(Color color)
    {
        gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
    }

    void Start()
    {
        _ntRenderer = GetComponent<Renderer>();
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }
}
