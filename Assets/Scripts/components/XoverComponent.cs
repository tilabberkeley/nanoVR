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
    protected MaterialPropertyBlock _mpb;

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
            if (_ntRenderer == null)
            {
                _ntRenderer = GetComponent<Renderer>();
            }
            if (_mpb == null)
            {
                _mpb = new MaterialPropertyBlock();
            }

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

            _color = value;
            _mpb.SetColor("_Color", _color);
            _ntRenderer.SetPropertyBlock(_mpb);
        }
    }

    private Color32 _savedColor;
    public Color32 SavedColor { get { return _savedColor; } set { _savedColor = value; } }

    /// <summary>
    /// Whether XoverComponet is xover or loopout.
    /// </summary>
    private bool _isXover = true;
    public bool IsXover { get => _isXover; set => _isXover = value; }

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
            transform.localScale = new Vector3(0.25f, dist, 0.25f);

            // Position
            transform.position = (end + start) / 2.0F;

            // Rotation
            transform.up = end - start;
            _length = dist;
            Color = Utils.GetStrand(_prevGO).Color;

            if (_bezier != null)
            {
                _bezier.Destroy();

                _bezier = DrawPoint.MakeXoverBezier(this, _savedColor);
            }
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
        _mpb = new MaterialPropertyBlock();
    }

    private Bezier? _bezier;
    public Bezier Bezier { get => _bezier; }

    /// <summary>
    /// Returns xover back from simplified strand view (nucleotide view).
    /// </summary>
    public void NucleotideView()
    {
        _ntRenderer.enabled = true;
        if (_bezier == null)
        {
            return;
        }

        _bezier.Destroy();
        _bezier = null;
    }

    /// <summary>
    /// Puts xover in simplified strand view.
    /// </summary>
    /// <param name="color">Color to make simplified version.</param>
    public void StrandView(Color color)
    {
        if (_bezier == null)
        {
            //Debug.Log("creating xover bezier");
            _bezier = DrawPoint.MakeXoverBezier(this, color);
            _ntRenderer.enabled = false;
        }
    }
}
