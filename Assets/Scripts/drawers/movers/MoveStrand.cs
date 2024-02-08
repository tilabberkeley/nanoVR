/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Highlight;

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
    private static GameObject s_oldNucl = null;
    private static GameObject s_currNucl = null;
    private static List<GameObject> s_currNucleotides;
    // bool movingStrand = false;


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

        if (gripReleased
            && _device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue)
            && gripValue
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            //movingStrand = true;
            gripReleased = false;
            GameObject go = s_hit.collider.gameObject;
            if (go.GetComponent<NucleotideComponent>() != null && go.GetComponent<NucleotideComponent>().Selected)
            {
                s_oldNucl = go;
                s_currNucl = go;
                s_currNucleotides = GetNucleotides(s_oldNucl, s_oldNucl);
                HighlightNucleotideSelection(s_currNucleotides, true);
            }
        }
        else if (!gripReleased && _device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
            && gripValue && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            GameObject go = s_hit.collider.gameObject;
            if (go != s_currNucl && go.GetComponent<NucleotideComponent>() != null && go.GetComponent<NucleotideComponent>().Selected)
            {
                Debug.Log("Entered elif + if");

                List<GameObject> nucleotides = GetNucleotides(s_oldNucl, go);
                if (IsValid(nucleotides, s_oldNucl))
                {
                    Debug.Log("Entered elif + if + if");

                    s_currNucl = go;
                    UnhighlightNucleotideSelection(s_currNucleotides);
                    s_currNucleotides = nucleotides;
                    HighlightNucleotideSelection(s_currNucleotides, true);
                }
            }
        }
        else if (_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
            && !gripValue && !gripReleased)
        {
            gripReleased = true;
            UnhighlightNucleotideSelection(s_currNucleotides);
            DoMove(s_oldNucl, s_currNucl);
            Reset();
        }
    }

    public void Reset()
    {
        s_oldNucl = null;
        s_currNucl = null;
    }

    public void DoMove(GameObject oldNucl, GameObject newNucl)
    {
        Debug.Log("Entered DoMove()");
        if (oldNucl == null || newNucl == null)
        {
            return;
        }

        List<GameObject> nucleotides = GetNucleotides(oldNucl, newNucl);
        if (!IsValid(nucleotides, oldNucl))
        {
            return;
        }
        ICommand command = new MoveStrandCommand(oldNucl, newNucl);
        CommandManager.AddCommand(command);
        command.Do();
        Debug.Log("Finished DoMove()");

    }

    // Moves helix's nucleotide objects to a new Grid Circle's position.
    public static void Move(GameObject oldNucl, GameObject newNucl)
    {
        Debug.Log("Entered Move()");
        List<GameObject> nucleotides = GetNucleotides(oldNucl, newNucl);

        if (!IsValid(nucleotides, oldNucl))
        {
            Debug.Log("Move() not valid");

            return;
        }

        var oldComp = oldNucl.GetComponent<NucleotideComponent>();
        s_strandDict.TryGetValue(oldComp.StrandId, out Strand strand);
        strand.ResetComponents(strand.Nucleotides);
        strand.Nucleotides = nucleotides;
        strand.SetComponents();
        Debug.Log("Finished Move()");
    }

    public static List<GameObject> GetNucleotides(GameObject oldNucl, GameObject newNucl)
    {
        Debug.Log("Entered GetNucleotides");
        if (oldNucl.GetComponent<NucleotideComponent>() == null || newNucl.GetComponent<NucleotideComponent>() == null)
        {
            return null;
        }

        var oldComp = oldNucl.GetComponent<NucleotideComponent>();
        var newComp = newNucl.GetComponent<NucleotideComponent>();

        if (oldComp.Direction != newComp.Direction)
        {
            return null;
        }

        Strand strand = s_strandDict[oldComp.StrandId];
        Debug.Log("Get oldNucl strand");


        // CANNOT HANDLE XOVERS RIGHT NOW
        if (strand.Xovers.Count > 0) { return null; }

        Helix helix = s_helixDict[newComp.HelixId];
        Debug.Log("Get newNucl helix");

        int count = strand.Count;
        int distToHead = oldComp.Id - strand.GetHead().GetComponent<NucleotideComponent>().Id;
        int startId = newComp.Id - distToHead;

        if (newComp.Direction == 0)
        {
            int endId = startId + count - 1;
            return helix.GetHelixSub(startId, endId, newComp.Direction);
        }
        else
        {
            int endId = startId - count + 1;
            return helix.GetHelixSub(endId, startId, newComp.Direction);
        }
    }

    public static bool IsValid(List<GameObject> nucleotides, GameObject oldNucl)
    {
        Debug.Log("Entered IsValid()");

        if (nucleotides == null) { return false; }

        var oldComp = oldNucl.GetComponent<NucleotideComponent>();
        foreach (GameObject nucleotide in nucleotides)
        {
            var ntc = nucleotide.GetComponent<NucleotideComponent>();
            if (ntc != null && ntc.StrandId != -1 && ntc.StrandId != oldComp.StrandId)
            {
                Debug.Log("Finished IsValid() false");

                return false;
            }
        }
        Debug.Log("Finished IsValid() true");

        return true;
    }
}
