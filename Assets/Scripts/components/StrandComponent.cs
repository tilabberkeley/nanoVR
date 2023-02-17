using System.Collections;
using UnityEngine;


public class StrandComponent : MonoBehaviour
{

    private int _strandId = -1;
    private GameObject _crossoverGO = null;
    private GameObject _crossoverBB = null;

    private GameObject _prevGO = null;
    private GameObject _nextGO = null;
    private GameObject _prevBB = null;
    private GameObject _nextBB = null;

    private bool _selected = false;

    private Color _color;

    private Renderer _ntRenderer;

    public int GetStrandId() { return _strandId; }
    public void SetStrandId(int strandId) { _strandId = strandId; }

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

    public bool IsSelected() { return _selected; }

    public Color GetStrandColor() { return _color; }
    public void SetStrandColor(Color c)
    {
        _color = c;
        GetComponent<Renderer>().material.SetColor("_Color", c);
    }
}
