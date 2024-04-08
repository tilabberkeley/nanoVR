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

    // Components
    private Renderer _ntRenderer;
    private Outline _outline;

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

    private void Update()
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
        }
    }

    void Awake()
    {
        _ntRenderer = gameObject.GetComponent<Renderer>();
        _outline = gameObject.GetComponent<Outline>();
        _outline.enabled = false;
    }
}
