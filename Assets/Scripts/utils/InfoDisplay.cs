/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using static GlobalVariables;

public class InfoDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;

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
        if (!_device.isValid)
        {
            GetDevice();
        }

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit s_hit))
        {
            if (s_hit.collider.GetComponent<NucleotideComponent>() != null)
            {
                DisplayNucleotideInfo(s_hit.collider.gameObject);
            }
            else if (s_hit.collider.GetComponent<BackBoneComponent>() != null)
            {
                DisplayBackboneInfo(s_hit.collider.gameObject);
            }
            else if (s_hit.collider.GetComponent<XoverComponent>() != null)
            {
                DisplayXoverInfo(s_hit.collider.gameObject);
            }
        }
    }

    private void DisplayNucleotideInfo(GameObject go)
    {
        var comp = go.GetComponent<NucleotideComponent>();
        string text = "<b>Nucleotide</b>\n";
        text += "DNA: " + comp.Sequence + "\n";
        text += "Nucl Id: " + comp.Id + "\n";
        text += "Direction: " + (comp.Direction == 1 ? "Forward" : "Reverse") + "\n\n";
        DisplayStrandInfo(comp.StrandId, text);
    }

    private void DisplayBackboneInfo(GameObject go)
    {
        var comp = go.GetComponent<BackBoneComponent>();
        string text = "<b>Backbone</b>\n";
        text += "Backbone Id: " + comp.Id + "\n";
        text += "Direction: " + (comp.Direction == 1 ? "Forward" : "Reverse") + "\n\n";
        DisplayStrandInfo(comp.StrandId, text);
    }

    private void DisplayXoverInfo(GameObject go)
    {
        var comp = go.GetComponent<XoverComponent>();
        string text = "<b>Xover</b>\n";
        text += "Length: " + comp.Length + "\n";
        text += "First Nucl: " + comp.PrevGO.name + "\n";
        text += "Second Nucl: " + comp.NextGO.name + "\n\n";
        DisplayStrandInfo(comp.StrandId, text);
    }

    private void DisplayStrandInfo(int strandId, string text)
    {
        if (strandId == -1)
        {
            textBox.text = text;
            return;
        }

        s_strandDict.TryGetValue(strandId, out Strand strand);
        text += "<b>Strand</b>\n";
        text += "Strand Id: " + strand.Id + "\n";
        text += "Helix Id: " + strand.HelixId + "\n";
        text += "Length: " + strand.Length + "\n";
        text += "Xovers: " + strand.Xovers.Count;
        textBox.text = text;
    }
}