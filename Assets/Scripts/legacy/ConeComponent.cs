/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

public class ConeComponent : MonoBehaviour
{
    protected Renderer _ntRenderer;

    private Outline _outline;

    private GameObject _head;

    private Color _color;

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


    public GameObject Head { get { return _head; } set { _head = value; } }

    // Start is called before the first frame update
    void Awake()
    {
        _ntRenderer = GetComponent<Renderer>();
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }
}
