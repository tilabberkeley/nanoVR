/*
 * nanoVR, a VR application for building DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


public class CopyPasteGrid : MonoBehaviour
{
    [SerializeField] private XRNode _XRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    private bool primaryReleased = true;
    private bool secondaryReleased = true;
    private bool triggerReleased = true;
    private bool pasting = false;
    private static DNAGrid s_copied = null;
    private static string s_json = null;

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_XRNode, _devices);
        if (_devices.Count > 0)
        {
            _device = _devices[0];
        }
    }

    private void OnEnable()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }
    }

    private void Update()
    {
        if (!_device.isValid)
        {
            GetDevice();
        }

        _device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryValue);
        if (primaryValue && primaryReleased && !pasting)
        {
            primaryReleased = false;
            s_copied = SelectGrid.Grid;

            if (s_copied != null)
            {
                s_json = CopyGrid(s_copied);
                Debug.Log("Copied grid!");
            }
        }

        _device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryValue);
        if (secondaryValue && secondaryReleased)
        {
            secondaryReleased = false;
            if (s_json != null)
            {
                pasting = true;
                PasteGrid(s_json);
                Debug.Log("Pasted grid!");
            }
        }

        _device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool triggerValue);
        if (triggerValue && triggerReleased && !rayInteractor.TryGetCurrent3DRaycastHit(out _))
        {
            triggerReleased = false;
            Reset();
        }

        // Resets trigger button.                                            
        if (!triggerValue)
        {
            triggerReleased = true;
        }

        // Resets primary button.                                            
        if (!primaryValue)
        {
            primaryReleased = true;
        }

        // Resets secondary button.                                            
        if (!secondaryValue)
        {
            secondaryReleased = true;
        }
    }

    public void Reset()
    {
        pasting = false;
        s_copied = null;
        s_json = null;
    }

    private string CopyGrid(DNAGrid grid)
    {
        List<string> gridIds = new List<string> { grid.Id };
        return FileExport.GetSCJSON(gridIds, true);
    }

    private void PasteGrid(string json)
    {
        //StartCoroutine(FileImport.ParseSC(json, true));
        FileImport.Instance.ParseSC(json, true);
    }
}