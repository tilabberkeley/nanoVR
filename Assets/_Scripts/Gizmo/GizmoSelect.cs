using UnityEngine;
using System.Collections;

public class GizmoSelect : MonoBehaviour 
{
    private Gizmo gizmoControl;
    private bool shiftDown;
	// Use this for initialization
	void Start () 
    {
        gizmoControl = GameObject.Find("Gizmo").GetComponent<Gizmo>();
	}
	
    void OnMouseDown()
    {
        if (gizmoControl != null)
        {
            if (!shiftDown)
            {
                gizmoControl.ClearSelection();
            }
            gizmoControl.Show();
            gizmoControl.SelectObject(transform);
            gameObject.layer = 2;
        }
    }
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            shiftDown = true;
        }
        else
        {
            shiftDown = false;
        }
	}

    public void Unselect()
    {
        gameObject.layer = 0;
    }
}
