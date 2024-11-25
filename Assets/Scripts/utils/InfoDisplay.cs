/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Text;
using static GlobalVariables;

public class InfoDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;

    private static string PROTEIN_STRING = "Protein";

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
            else if (s_hit.collider.GetComponent<LoopoutComponent>() != null)
            {
                DisplayLoopoutInfo(s_hit.collider.gameObject);
            }
            else if (s_hit.collider.GetComponent<XoverComponent>() != null)
            {
                DisplayXoverInfo(s_hit.collider.gameObject);
            }
            else if (s_hit.collider.name.Contains(PROTEIN_STRING))
            {
                DisplayProteinInfo(s_hit.collider.gameObject);
            }
        }
    }

    private void DisplayProteinInfo(GameObject go) 
    {
        StringBuilder text = new StringBuilder();
        text.Append($"<b>{PROTEIN_STRING}</b>\n");
        text.Append(go.name);
        textBox.text = text.ToString();
    }

    private void DisplayNucleotideInfo(GameObject go)
    {
        var comp = go.GetComponent<NucleotideComponent>();
        StringBuilder text = new StringBuilder();
        text.Append("<b>Nucleotide</b>\n");
        text.Append("DNA: " + comp.Sequence + "\n");
        if (comp.IsInsertion) text.Append("Insertion Length: " + comp.Insertion + "\n");
        text.Append("Nucl Id: " + comp.Id + "\n");
        text.Append("Helix Id: " + comp.HelixId + "\n");
        text.Append("Direction: " + (comp.Direction == 1 ? "Forward" : "Reverse") + "\n");
        if (comp.IsExtension) text.Append("Is extension \n\n");
        DisplayStrandInfo(comp.StrandId, text);

    }

    /*private void DisplayBackboneInfo(GameObject go)
    {
        var comp = go.GetComponent<BackBoneComponent>();
        string text = "<b>Backbone</b>\n";
        text += "Backbone Id: " + comp.Id + "\n";
        text += "Helix Id: " + comp.HelixId + "\n";
        text += "Direction: " + (comp.Direction == 1 ? "Forward" : "Reverse") + "\n\n";
        DisplayStrandInfo(comp.StrandId, text);
    }*/

    private void DisplayLoopoutInfo(GameObject go)
    {
        LoopoutComponent comp = go.GetComponent<LoopoutComponent>();
        StringBuilder text = new StringBuilder();
        text.Append("<b>Loopout</b>\n");
        text.Append("Length: " + comp.SequenceLength + "\n");
        text.Append("Sequence: " + comp.Sequence + "\n");
        text.Append("First Nucl: " + comp.PrevGO.name + "\n");
        text.Append("Second Nucl: " + comp.NextGO.name + "\n\n");
        DisplayStrandInfo(comp.StrandId, text);
    }

    private void DisplayXoverInfo(GameObject go)
    {
        var comp = go.GetComponent<XoverComponent>();
        StringBuilder text = new StringBuilder();
        text.Append("<b>Xover</b>\n");
        text.Append("Length: " + Math.Round(comp.Length, 2) + "\n");
        text.Append("1st Nucl: " + comp.PrevGO.name + "\n");
        text.Append("2nd Nucl: " + comp.NextGO.name + "\n\n");
        DisplayStrandInfo(comp.StrandId, text);
    }

    private void DisplayStrandInfo(int strandId, StringBuilder text)
    {
        if (strandId == -1)
        {
            textBox.text = text.ToString();
            return;
        }
        Strand strand;
        if (s_visualMode)
        {
            s_visStrandDict.TryGetValue(strandId, out strand);
        }
        else
        {
            s_strandDict.TryGetValue(strandId, out strand);
        }
        text.Append("<b>Strand</b>\n");
        text.Append("Strand Id: " + strand.Id + "\n");
        text.Append("Length: " + strand.Length + "\n");
        text.Append("Xovers: " + strand.Xovers.Count);
        textBox.text = text.ToString();
    }
}
