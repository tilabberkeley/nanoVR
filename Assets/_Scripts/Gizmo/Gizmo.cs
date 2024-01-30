using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GizmoTypes { Position, Rotation, Scale }
public enum GizmoControl { Horizontal, Vertical, Both }
public enum GizmoAxis { Center, X, Y, Z }

public class Gizmo : MonoBehaviour 
{
    public GizmoHandle AxisCenter;
    public GizmoHandle AxisX;
    public GizmoHandle AxisY;
    public GizmoHandle AxisZ;
    public GizmoTypes Type;
    public List<Transform> SelectedObjects;
    public Vector3 Center;
    public Camera Camera;
    public bool Visible;
    public float DefaultDistance = 3.2f;
    public float ScaleFactor = 0.2f;
    
    private Vector3 localScale;
    private Transform _transform;

    void Awake()
    {
        Visible = false;
        SetType(GizmoTypes.Position);
        Hide();
        // set the axis start type
        AxisCenter.Axis = GizmoAxis.Center;
        AxisCenter.Gizmo = this;
        AxisX.Axis = GizmoAxis.X;
        AxisX.Gizmo = this;
        AxisY.Axis = GizmoAxis.Y;
        AxisY.Gizmo = this;
        AxisZ.Axis = GizmoAxis.Z;
        AxisZ.Gizmo = this;

        _transform = transform;
        localScale = _transform.localScale;
        SelectedObjects = new List<Transform>();
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (Visible)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetType(GizmoTypes.Position);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetType(GizmoTypes.Rotation);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetType(GizmoTypes.Scale);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearSelection();
                Hide();
            }
        }
        if (SelectedObjects.Count > 0)
        {
            // Scale based on distance from the camera
            var distance = Vector3.Distance(_transform.position, Camera.transform.position);
            var scale = (distance - DefaultDistance) * ScaleFactor;
            _transform.localScale = new Vector3(localScale.x + scale, localScale.y + scale, localScale.z + scale);

            // Move the gizmo to the center of our parent
            UpdateCenter();
            _transform.position = Center;
        }
	}

    public void SetType(GizmoTypes type)
    {
        // set the type of all the axis
        Type = type;
        AxisCenter.SetType(type);
        AxisX.SetType(type);
        AxisY.SetType(type);
        AxisZ.SetType(type);
    }
    public void ClearSelection()
    {
        foreach (var obj in SelectedObjects)
        {
            (obj.gameObject.GetComponent<GizmoSelect>()).Unselect();
        }
        SelectedObjects.Clear();
        Center = Vector3.zero;
    }
    public void UpdateCenter()
    {
        if (SelectedObjects.Count > 1)
        {
            var vectors = new Vector3[SelectedObjects.Count];
            for (int i = 0; i < SelectedObjects.Count; i++)
            {
                vectors[i] = SelectedObjects[i].position;
            }
            Center = CenterOfVectors(vectors);
        }
        else
        {
            Center = SelectedObjects[0].position;
        }
    }
    public void SelectObject(Transform parent)
    {
        if (!SelectedObjects.Contains(parent))
            SelectedObjects.Add(parent);
        UpdateCenter();
    }
    public void ActivateAxis(GizmoAxis axis)
    {
        switch (axis)
        {
            case GizmoAxis.Center:
                AxisCenter.SetActive(true);
                break;
            case GizmoAxis.X:
                AxisX.SetActive(true);
                break;
            case GizmoAxis.Y:
                AxisY.SetActive(true);
                break;
            case GizmoAxis.Z:
                AxisZ.SetActive(true);
                break;
        }
        SetType(Type);
    }
    public void DeactivateAxis(GizmoAxis axis)
    {
        switch (axis)
        {
            case GizmoAxis.Center:
                AxisCenter.SetActive(false);
                break;
            case GizmoAxis.X:
                AxisX.SetActive(false);
                break;
            case GizmoAxis.Y:
                AxisY.SetActive(false);
                break;
            case GizmoAxis.Z:
                AxisZ.SetActive(false);
                break;
        }
        SetType(Type);
    }
    public void DeactivateHandles()
    {
        AxisCenter.SetActive(false);
        AxisX.SetActive(false);
        AxisY.SetActive(false);
        AxisZ.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActiveRecursively(true);
        SetType(Type);
        Visible = true;
    }

    public void Hide()
    {
        gameObject.SetActiveRecursively(false);
        gameObject.active = true;
        Visible = false;
    }

    public Vector3 CenterOfVectors(Vector3[] vectors)
    {
        Vector3 sum = Vector3.zero;
        if (vectors == null || vectors.Length == 0)
        {
            return sum;
        }

        foreach (Vector3 vec in vectors)
        {
            sum += vec;
        }
        return sum / vectors.Length;
    }

}
