/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Linq;
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
            if (s_hit.collider.GetComponent<NucleotideColliderComponent>() != null)
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
            else if (s_hit.collider.GetComponent<GridComponent>() != null)
            {
                DisplayGridComponentInfo(s_hit.collider.gameObject);
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
        text.AppendLine(string.Format("<b>{0}</b>", PROTEIN_STRING));
        text.AppendLine(go.name);
        textBox.text = text.ToString();
    }

    private void DisplayNucleotideInfo(GameObject go)
    {
        var comp = go.GetComponent<NucleotideColliderComponent>().Data;
        StringBuilder text = new StringBuilder();
        text.AppendLine("<b>Nucleotide</b>");
        string seq = comp.Sequence;
        if (comp.IsInsertion && comp.Direction == 0)
        {
            seq = new string(seq.Reverse().ToArray());
        }
        text.AppendLine(string.Format("DNA: {0}", seq));
        if (comp.IsInsertion) text.AppendLine(string.Format("Insertion Length: {0}", comp.Insertion));
        text.AppendLine(string.Format("Nucl Id: {0}", comp.Id));
        text.AppendLine(string.Format("Helix Id: {0}", comp.HelixId));
        text.AppendLine(string.Format("Direction: {0}", comp.Direction == 1 ? "Forward" : "Reverse"));
        if (comp.GetDomain() != null && comp.GetDomain().IsExtension) text.AppendLine("Is extension");
        DisplayStrandInfo(comp.StrandId, text);

    }

    /*private void DisplayBackboneInfo(GameObject go)
    {
        var comp = go.GetComponent<BackBoneComponent>();
        string text = "<b>Backbone</b>";
        text += string.Format("Backbone Id: {0}", comp.Id);
        text += string.Format("Helix Id: {0}", comp.HelixId);
        text += string.Format("Direction: {0}", comp.Direction == 1 ? "Forward" : "Reverse");
        DisplayStrandInfo(comp.StrandId, text);
    }*/

    private void DisplayLoopoutInfo(GameObject go)
    {
        LoopoutComponent comp = go.GetComponent<LoopoutComponent>();
        StringBuilder text = new StringBuilder();
        NucleotideData prevNucl = comp.PrevNucl;
        NucleotideData nextNucl = comp.NextNucl;

        text.AppendLine("<b>Loopout</b>");
        text.AppendLine(string.Format("Length: {0}", comp.SequenceLength));
        text.AppendLine(string.Format("Sequence: {0}", comp.Sequence));
        text.AppendLine(string.Format("1st Nucl: {0}", prevNucl.ToString()));
        text.AppendLine(string.Format("2nd Nucl: {0}", nextNucl.ToString()));
        DisplayStrandInfo(comp.StrandId, text);
    }

    private void DisplayXoverInfo(GameObject go)
    {
        var comp = go.GetComponent<XoverComponent>();
        StringBuilder text = new StringBuilder();
        NucleotideData prevNucl = comp.PrevNucl;
        NucleotideData nextNucl = comp.NextNucl;

        text.AppendLine("<b>Xover</b>");
        text.AppendLine(string.Format("Length: {0}", Math.Round(comp.Length, 2)));
        text.AppendLine(string.Format("1st Nucl: {0}", prevNucl.ToString()));
        text.AppendLine(string.Format("2nd Nucl: {0}", nextNucl.ToString()));
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
        text.AppendLine("<b>Strand</b>");
        text.AppendLine(string.Format("Strand Id: {0}", strand.Id));
        text.AppendLine(string.Format("Length: {0}", strand.GetLength()));
        text.AppendLine(string.Format("Domains: {0}", strand.Domains.Count));
        textBox.text = text.ToString();
    }

    private void DisplayGridComponentInfo(GameObject go)
    {
        var comp = go.GetComponent<GridComponent>();
        StringBuilder text = new StringBuilder();

        text.AppendLine("<b>Grid</b>");
        text.AppendLine(string.Format("Grid Id: {0}", comp.GridId));
        if (!comp.Grid.Type.Equals("none"))
        {
            text.AppendLine(string.Format("Coord: [{0}, {1}]", comp.GridPoint.X, comp.GridPoint.Y));
        }
        if (comp.Helix != null) text.AppendLine(string.Format("Helix Id: {0}", comp.Helix.Id));
        textBox.text = text.ToString();
    }
}
