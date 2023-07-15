using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeComponent : MonoBehaviour
{
    private Outline _outline;

    // Start is called before the first frame update
    void Start()
    {
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }
}
