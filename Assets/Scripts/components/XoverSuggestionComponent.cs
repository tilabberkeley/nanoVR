using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XoverSuggestionComponent : MonoBehaviour
{
    private NucleotideComponent _nucleotideComponent0;
    public NucleotideComponent NucleotideComponent0 { get { return _nucleotideComponent0; } set { _nucleotideComponent0 = value; } }
    private NucleotideComponent _nucleotideComponent1;
    public NucleotideComponent NucleotideComponent1 { get { return _nucleotideComponent1; } set { _nucleotideComponent0 = value; } }
    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        // On instantiation, this xover suggestion shouldn't be visable.
        // _renderer.enabled = false;
    }

    private void CreateXover()
    {
        DrawCrossover.DoCreateXover(_nucleotideComponent0.gameObject, _nucleotideComponent1.gameObject);
        Destroy(gameObject);
    }
}
