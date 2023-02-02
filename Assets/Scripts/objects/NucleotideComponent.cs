using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NucleotideComponent : MonoBehaviour
{
    private int _id;
    private GameObject _crossoverGO = null;
    private GameObject _crossoverBB = null;

    private GameObject _prevGO = null;
    private GameObject _nextGO = null;
    private GameObject _prevBB = null;
    private GameObject _nextBB = null;
    private GameObject _complementGO = null;

    private bool _selected = false;

    private Color _color = Color.green;
    private Color _transGray = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private Renderer _ntRenderer;


    public int GetId() {return _id;}
    public void SetId(int id) {_id = id; }

    public Vector3 GetPosition() {return transform.position;}
    public void SetPosition(Vector3 p) { transform.position = p;}

    public bool HasCrossover() {return _crossoverGO != null;}
    public GameObject GetCrossover() {return _crossoverGO;}
    public void SetCrossover(GameObject c) {_crossoverGO = c;}

    public GameObject GetPrevGO() {return _prevGO;}
    public void SetPrevGO(GameObject p) {_prevGO = p;}

    public GameObject GetPrevBB() {return _prevBB;}
    public void SetPrevBB(GameObject p) {_prevBB = p;}

    public GameObject GetNextGO() {return _nextGO;}
    public void SetNextGO(GameObject n) {_nextGO = n;}

    public GameObject GetNextBB() {return _nextBB;}
    public void SetNextBB(GameObject n) {_nextBB = n;}
    
    public GameObject GetComplementGO() {return _complementGO;}
    public void SetComplementGO(GameObject c) {_complementGO = c;}

    public bool IsSelected() {return _selected;}

    public void FlipSelected()
    {
        _selected = !_selected;
        if (_selected) 
        {
            _ntRenderer.material.SetColor("_Color", _color);
            if (_prevGO.GetComponent<NucleotideComponent>().IsSelected())
            {
                _prevBB.GetComponent<Renderer>().material.SetColor("_Color", _color);
            }
            if (_nextGO.GetComponent<NucleotideComponent>().IsSelected())
            {
                _nextBB.GetComponent<Renderer>().material.SetColor("_Color", _color);
            }
        } 
        else 
        {
            _ntRenderer.material.SetColor("_Color", _transGray);
            _prevBB.GetComponent<Renderer>().material.SetColor("_Color", _transGray);
            _nextBB.GetComponent<Renderer>().material.SetColor("_Color", _transGray);
        }
    }


    public void Highlight() 
    {
        _ntRenderer.material.SetColor("_Color", Color.yellow);
    }

    public void Unhighlight() 
    {
        if (_selected) 
        {
            _ntRenderer.material.SetColor("_Color", _color);
        }
        else
        {
            _ntRenderer.material.SetColor("_Color", _transGray);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _ntRenderer = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
