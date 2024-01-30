using UnityEngine;
using System.Collections;

public class GizmoHandle : MonoBehaviour 
{
    public Gizmo Gizmo;
    public GizmoControl Control;
    public GizmoTypes Type;
    public GameObject PositionCap;
    public GameObject RotationCap;
    public GameObject ScaleCap;
    public Material ActiveMaterial;
    public GizmoAxis Axis;

    public float MoveSensitivity = 10f;
    public float RotationSensitivity = 64f;
    public float ScaleSensitivity = 10f;

    private Material inactiveMaterial;
    private bool activeHandle;

    void Awake()
    {
        inactiveMaterial = GetComponent<Renderer>().material;
    }

    public void OnMouseDown()
    {
        Gizmo.DeactivateHandles();
        SetActive(true);
    }

    public void OnMouseDrag()
    {
        var delta = 0f;
        var vert = 0f;
        var horz = 0f;
        if (activeHandle)
        {
            horz = Input.GetAxis("Mouse X") * Time.deltaTime;
            vert = Input.GetAxis("Mouse Y") * Time.deltaTime;

            // TODO: GizmoControl should be based on the camera not a selection -- X, Z are set to "both" for now.
            switch (Control)
            {
                case GizmoControl.Horizontal:
                    delta = Input.GetAxis("Mouse X") * Time.deltaTime;
                    break;
                case GizmoControl.Vertical:
                    delta = Input.GetAxis("Mouse Y") * Time.deltaTime;
                    break;
                case GizmoControl.Both:
                    delta = (Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y")) * Time.deltaTime;
                    break;
            }

            switch (Type)
            {
                case GizmoTypes.Position:
                    delta *= MoveSensitivity;
                    horz *= MoveSensitivity;
                    vert *= MoveSensitivity;
                    switch (Axis)
                    {
                        case GizmoAxis.X:
                            foreach (var obj in Gizmo.SelectedObjects)
                                obj.Translate(Vector3.right * delta, Space.World);
                            break;
                        case GizmoAxis.Y:
                            foreach (var obj in Gizmo.SelectedObjects)
                                obj.Translate(Vector3.up * delta, Space.World);
                            break;
                        case GizmoAxis.Z:
                            foreach (var obj in Gizmo.SelectedObjects)
                                obj.Translate(Vector3.forward * delta, Space.World);
                            break;
                        case GizmoAxis.Center:
                            // Based on the camera position we need to either move X horizontal or vertical / vice versa with Z
                            foreach (var obj in Gizmo.SelectedObjects)
                            {
                                obj.Translate(Vector3.right * horz, Space.World);
                                obj.Translate(Vector3.forward * vert, Space.World);
                            }
                            break;
                    }
                    break;

                case GizmoTypes.Scale:
                    delta *= ScaleSensitivity;
                    switch (Axis)
                    {
                        case GizmoAxis.X:
                            foreach (var obj in Gizmo.SelectedObjects)
                                obj.localScale = new Vector3(obj.localScale.x + delta, obj.localScale.y, obj.localScale.z);
                            break;
                        case GizmoAxis.Y:
                            foreach (var obj in Gizmo.SelectedObjects)
                                obj.localScale = new Vector3(obj.localScale.x, obj.localScale.y + delta, obj.localScale.z);
                            break;
                        case GizmoAxis.Z:
                            foreach (var obj in Gizmo.SelectedObjects)
                                obj.localScale = new Vector3(obj.localScale.x, obj.localScale.y, obj.localScale.z + delta);
                            break;
                        case GizmoAxis.Center:
                            foreach (var obj in Gizmo.SelectedObjects)
                                obj.localScale = new Vector3(obj.localScale.x + delta, obj.localScale.y + delta, obj.localScale.z + delta);
                            break;
                    }
                    break;

                case GizmoTypes.Rotation:
                    delta *= RotationSensitivity;
                    switch (Axis)
                    {
                        case GizmoAxis.X:
                            foreach (var obj in Gizmo.SelectedObjects)
                                obj.Rotate(Vector3.right * delta);
                            break;
                        case GizmoAxis.Y:
                            foreach (var obj in Gizmo.SelectedObjects)
                                obj.Rotate(Vector3.up * delta);
                            break;
                        case GizmoAxis.Z:
                            foreach (var obj in Gizmo.SelectedObjects)
                                obj.Rotate(Vector3.forward * delta);
                            break;
                        case GizmoAxis.Center:
                            foreach (var obj in Gizmo.SelectedObjects)
                            {
                                obj.Rotate(Vector3.right * delta);
                                obj.Rotate(Vector3.up * delta);
                                obj.Rotate(Vector3.forward * delta);
                            }
                            break;
                    }
                    break;
            }
        }
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            activeHandle = true;
            GetComponent<Renderer>().material = ActiveMaterial;
            if (Axis != GizmoAxis.Center)
            {
                PositionCap.GetComponent<Renderer>().material = ActiveMaterial;
                RotationCap.GetComponent<Renderer>().material = ActiveMaterial;
                ScaleCap.GetComponent<Renderer>().material = ActiveMaterial;
            }
        }
        else
        {
            activeHandle = false;
            GetComponent<Renderer>().material = inactiveMaterial;
            if (Axis != GizmoAxis.Center)
            {
                PositionCap.GetComponent<Renderer>().material = inactiveMaterial;
                RotationCap.GetComponent<Renderer>().material = inactiveMaterial;
                ScaleCap.GetComponent<Renderer>().material = inactiveMaterial;
            }
        }
    }

    public void SetType(GizmoTypes type)
    {
        Type = type;
        if (Axis != GizmoAxis.Center)
        {
            PositionCap.SetActiveRecursively(type == GizmoTypes.Position);
            RotationCap.SetActiveRecursively(type == GizmoTypes.Rotation);
            ScaleCap.SetActiveRecursively(type == GizmoTypes.Scale);
        }
    }

}
