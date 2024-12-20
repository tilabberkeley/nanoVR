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
    private static List<DNAGrid> s_copied = new List<DNAGrid>();
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
            s_copied = SelectGrid.Grids;

            /* We only want to copy paste the selected grid if no strands are selected.
             * This is because both Grids and Strands use the same two buttons to copy paste,
             * so we don't want to copy paste a Grid when the user was trying to copy paste a Strand.
             */
            if (s_copied != null && SelectStrand.Strands.Count == 0)
            {
                s_json = CopyGrids(s_copied);
                //Debug.Log("Copied grid!");
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
                //Debug.Log("Pasted grid!");
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

    private string CopyGrids(List<DNAGrid> grids)
    {
        if (grids.Count == 0)
        {
            Debug.Log("Please select a grid to copy");
            return "";
        }

        List<string> gridIds = new List<string>();
        foreach (DNAGrid grid in grids)
        {
            gridIds.Add(grid.Id);
        }

        string json = FileExport.GetSCJSON(gridIds, true);
        Debug.Log(json);
        return json;
    }

    private void PasteGrid(string json)
    {
        //StartCoroutine(FileImport.ParseSC(json, true));
        if (json.Length == 0)
        {
            return;
        }
        FileImport.Instance.ParseSC(json, true);
    }
}