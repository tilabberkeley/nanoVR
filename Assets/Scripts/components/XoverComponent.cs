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
    private int _helixId; // id of helix
    private int _strandId = -1; // id of strand
    private int _direction; // 0 = 5' to 3' right->left, 1 = left->right
    private double _length;
    private GameObject _prevGO = null;
    private GameObject _nextGO = null; // GameObject to split strand at if xover is deleted.
    
    private bool _selected = false;

    private Color _color = Color.white;

    private Renderer _ntRenderer;
   
    public int GetHelixId() { return _helixId; }
    public void SetHelixId(int helixId) { _helixId = helixId; }
    public int GetStrandId() { return _strandId; }
    public void SetStrandId(int strandId) { _strandId = strandId; }
    public int GetDirection() { return _direction; }
    public void SetDirection(int direction) { _direction = direction; }
    public double GetLength() { return _length; }
    public void SetLength(double length) { _length = length; }
    public GameObject GetPrevGO() { return _prevGO; }
    public void SetPrevGO(GameObject s) { _prevGO = s; }
    public GameObject GetNextGO() { return _nextGO; }
    public void SetNextGO(GameObject s) { _nextGO = s; }
    public bool IsSelected() { return _selected; }
    public void SetSelected(bool selected) { _selected = selected; }

    public Color GetColor() { return _color; }
    public void SetColor(Color c)
    {
        _color = c;
        _ntRenderer.material.SetColor("_Color", c);
    }

    public void ResetColor()
    {
        _color = Color.white;
        _ntRenderer.material.SetColor("_Color", _color);
    }

    public void Highlight(Color color)
    {
        gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
    }

    void Start()
    {
        _ntRenderer = gameObject.GetComponent<Renderer>();
    }
}
