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
public class MoveStrand : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    bool gripReleased = true;
    private static RaycastHit s_hit;
    private static GameObject s_nucleotide = null;

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
        if (!s_gridTogOn || s_hideStencils)
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
            if (s_hit.collider.gameObject.GetComponent<NucleotideComponent>())
            {
                if (s_nucleotide == null)
                {
                    s_nucleotide = s_hit.collider.gameObject;
                }
                else
                {
                    if (!s_hit.collider.gameObject.Equals(s_nucleotide))
                    {
                        DoMove(s_nucleotide, s_hit.collider.gameObject);
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
        s_nucleotide = null;
    }

    public void DoMove(GameObject oldNucl, GameObject newNucl)
    {
        List<GameObject> nucleotides = GetNucleotides(oldNucl, newNucl);
        if (!IsValid(nucleotides, oldNucl))
        {
            return;
        }
        ICommand command = new MoveStrandCommand(oldNucl, newNucl);
        CommandManager.AddCommand(command);
        command.Do();
    }

    // Moves helix's nucleotide objects to a new Grid Circle's position.
    public static void Move(GameObject oldNucl, GameObject newNucl)
    {
        List<GameObject> nucleotides = GetNucleotides(oldNucl, newNucl);

        if (!IsValid(nucleotides, oldNucl))
        {
            return;
        }

        var oldComp = oldNucl.GetComponent<NucleotideComponent>();
        s_strandDict.TryGetValue(oldComp.StrandId, out Strand strand);
        strand.ResetComponents(strand.Nucleotides);
        strand.Nucleotides = nucleotides;
        strand.SetComponents();
    }

    public static List<GameObject> GetNucleotides(GameObject oldNucl, GameObject newNucl)
    {
        var oldComp = oldNucl.GetComponent<NucleotideComponent>();
        var newComp = newNucl.GetComponent<NucleotideComponent>();
        Strand strand = s_strandDict[oldComp.StrandId];
        Helix helix = s_helixDict[newComp.HelixId];
        int count = strand.Count;
        int distToHead = oldComp.Id - strand.GetHead().GetComponent<NucleotideComponent>().Id;
        int startId = newComp.Id - distToHead;
        int endId = startId + count;

        return helix.GetHelixSub(startId, endId, newComp.Direction);
    }

    public static bool IsValid(List<GameObject> nucleotides, GameObject oldNucl)
    {
        if (nucleotides == null) { return false; }

        var oldComp = oldNucl.GetComponent<NucleotideComponent>();
        foreach (GameObject nucleotide in nucleotides)
        {
            var ntc = nucleotide.GetComponent<NucleotideComponent>();
            if (ntc.StrandId != -1 && ntc.StrandId != oldComp.StrandId)
            {
                return false;
            }
        }
        return true;
    }
}
