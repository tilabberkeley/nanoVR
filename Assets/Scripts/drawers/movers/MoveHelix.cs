/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

/// <summary>
/// Moves a helix from one grid circle to another.
/// </summary>
public class MoveHelix : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    bool gripReleased = true;
    private static RaycastHit s_hit;
    private static GameObject s_gridCircle = null;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
        if (_devices.Count > 0)
        {
            _device = _devices[0];
        }
    }

    void OnEnable()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }
    }

    void Update()
    {
        if (s_hideStencils)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool gripValue;
        if (gripReleased
            && _device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
            && gripValue
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            gripReleased = false;
            if (s_hit.collider.gameObject.GetComponent<GridComponent>())
            {
                if (s_gridCircle == null)
                {
                    s_gridCircle = s_hit.collider.gameObject;
                }
                else
                {
                    if (!s_hit.collider.gameObject.Equals(s_gridCircle))
                    {
                        var gc = s_hit.collider.gameObject.GetComponent<GridComponent>();
                        DoMove(s_gridCircle, s_hit.collider.gameObject);
                        gc.Grid.CheckExpansion(gc);
                        Reset();
                    }
                }
            }
        }

        if (!(_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
            && gripValue))
        {
            gripReleased = true;
        }
    }

    public void Reset()
    {
        s_gridCircle = null;
    }

    public void DoMove(GameObject oldCircle, GameObject newCircle)
    {
        if (!IsValid(oldCircle, newCircle))
        {
            return;
        }
        ICommand command = new MoveHelixCommand(oldCircle, newCircle);
        CommandManager.AddCommand(command);
        command.Do();
    }

    // Moves helix's nucleotide objects to a new Grid Circle's position.
    public static void Move(GameObject oldCircle, GameObject newCircle)
    {
        if (!IsValid(oldCircle, newCircle))
        {
            return;
        }
        float diffX = newCircle.transform.position.x - oldCircle.transform.position.x;
        float diffY = newCircle.transform.position.y - oldCircle.transform.position.y;
        float diffZ = newCircle.transform.position.z - oldCircle.transform.position.z;

        var oldComp = oldCircle.GetComponent<GridComponent>();
        var newComp = newCircle.GetComponent<GridComponent>();

        oldComp.Helix.MoveNucleotides(new Vector3(diffX, diffY, diffZ));
        newComp.Helix = oldComp.Helix;
        oldComp.Helix = null;
        oldComp.Selected = false;
        newComp.Selected = true;
        //newComp.Helix.StartPoint = oldCircle.transform.position;
        newComp.Helix._gridComponent = newComp;
    }

    public static bool IsValid(GameObject oldCircle, GameObject newCircle)
    {
        var oldComp = oldCircle.GetComponent<GridComponent>();
        var newComp = newCircle.GetComponent<GridComponent>();

        if (oldComp.Selected && !newComp.Selected)
        {
            return true;
        }
        return false;
    }
}
