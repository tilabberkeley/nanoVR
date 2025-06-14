/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkely.edu>
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

    protected NucleotideData prevNucl;
    protected NucleotideData nextNucl;

    public NucleotideData PrevNucl { get { return prevNucl; } set { prevNucl = value; } }
    public NucleotideData NextNucl { get { return nextNucl; } set { nextNucl = value; } }

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
    private bool isLoopout = false;
    public bool IsLoopout { get => isLoopout; set => isLoopout = value; }

    public virtual void UpdateXover()
    {
        Vector3 start = prevNucl.GetPosition();
        Vector3 end = nextNucl.GetPosition();

        // Scale        
        float dist = Vector3.Distance(end, start);
        transform.localScale = new Vector3(0.2f, dist, 0.2f);

        // Position
        transform.position = (end + start) / 2.0F;

        // Rotation
        transform.up = end - start;
    }

    // Strand id of the strand that was merged with the first strand
    private int _prevStrandId;
    public int PrevStrandId { get { return _prevStrandId; } set { _prevStrandId = value; } }

    void Awake()
    {
        _ntRenderer = gameObject.GetComponent<Renderer>();
        //_outline = gameObject.GetComponent<Outline>();
        //_outline.enabled = false;
        _mpb = new MaterialPropertyBlock();
    }
}
