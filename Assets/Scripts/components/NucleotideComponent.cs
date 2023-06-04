/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;

/// <summary>
/// Component attached to each nucleotide gameobject. Handles direct Ray interactions and gameobject visuals.
/// </summary>
public class NucleotideComponent : MonoBehaviour
{
    private int _id; // index of nucl in helix
    private int _helixId; // id of helix
    private int _strandId = -1; // id of strand
    private int _direction; // 0 = 5' to 3' right->left, 1 = left->right
    private GameObject _crossoverGO = null;
    private GameObject _crossoverBB = null;

    private GameObject _prevGO = null;
    private GameObject _nextGO = null;
    private GameObject _prevBB = null;
    private GameObject _nextBB = null;
    private GameObject _complementGO = null;

    private bool _selected = false;

    private Color _color = Color.white;
    private static Color s_yellow = new Color(1, 0.92f, 0.016f, 0.5f);

    private Renderer _ntRenderer;

    public int Id { get; set; }
    public void SetId(int id) { _id = id; }
    public int GetHelixId() { return _helixId; }
    public void SetHelixId(int helixId) { _helixId = helixId; }
    public int GetStrandId() { return _strandId; }
    public void SetStrandId(int strandId) { _strandId = strandId; }
    public int GetDirection() { return _direction; }
    public void SetDirection(int direction) { _direction = direction; }
    public Vector3 GetPosition() { return transform.position; }
    public void SetPosition(Vector3 p) { transform.position = p; }
    public bool HasCrossover() { return _crossoverGO != null; }
    public GameObject GetCrossoverGO() { return _crossoverGO; }
    public void SetCrossoverGO(GameObject c) { _crossoverGO = c; }
    public GameObject GetCrossoverBB() { return _crossoverBB; }
    public void SetCrossoverBB(GameObject c) { _crossoverBB = c; }
    public GameObject GetPrevGO() { return _prevGO; }
    public void SetPrevGO(GameObject p) { _prevGO = p; }
    public GameObject GetPrevBB() { return _prevBB; }
    public void SetPrevBB(GameObject p) { _prevBB = p; }
    public GameObject GetNextGO() { return _nextGO; }
    public void SetNextGO(GameObject n) { _nextGO = n; }
    public GameObject GetNextBB() { return _nextBB; }
    public void SetNextBB(GameObject n) { _nextBB = n; }
    public GameObject GetComplementGO() { return _complementGO; }
    public void SetComplementGO(GameObject c) { _complementGO = c; }
    public bool IsSelected() { return _selected; }
    public void SetSelected(bool selected) { _selected = selected; }

    public Color GetColor() { return _color; }
    public void SetColor(Color c) 
    { 
        _color = c; 
        _ntRenderer.material.SetColor("_Color", c); 
    }

    public void ResetColor() { 
        _color = Color.white;
        _ntRenderer.material.SetColor("_Color", _color);
    }
    
    public void Highlight(Color color)
    {
        gameObject.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
    }

    // Start is called before the first frame update
    void Start()
    {
        _ntRenderer = gameObject.GetComponent<Renderer>();
    }

}
