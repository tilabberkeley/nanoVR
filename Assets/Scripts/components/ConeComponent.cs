/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;

public class ConeComponent : MonoBehaviour
{
    private Outline _outline;

    private GameObject _head;

    public GameObject Head { get { return _head; } set { _head = value; } }

    // Start is called before the first frame update
    void Awake()
    {
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }
}
