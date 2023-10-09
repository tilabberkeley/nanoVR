/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Highlight;

public class CopyPaste : MonoBehaviour
{
    [SerializeField] private XRNode _XRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    private bool primaryReleased = true;
    private bool secondaryReleased = true;
    private bool triggerReleased = true;
    private bool pasting = false;
    private static Strand s_copied = null;
    private static RaycastHit s_hit;
    private static List<GameObject> s_currNucleotides = null;
    private static List<(GameObject, GameObject)> s_newEndpoints = null;

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
        if (!s_gridTogOn)
        {
            return;
        }
        /*
        if (!s_drawTogOn && !s_eraseTogOn)
        {
            return;
        }*/

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool primaryValue;
        if (_device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue) && primaryValue && primaryReleased && !pasting)
        {
            primaryReleased = false;
            s_copied = SelectStrand.s_strand;
            Debug.Log("Copied!");
        }

        bool secondaryValue;
        if (_device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryValue)
            && secondaryValue
            && secondaryReleased)
        {
            secondaryReleased = false;
            if (s_copied != null)
            {
                Debug.Log("Pasting!");
                pasting = true;
            }
        }

        if (pasting && rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            GameObject go = s_hit.collider.gameObject;
            if (go.GetComponent<NucleotideComponent>() && !go.GetComponent<NucleotideComponent>().IsBackbone)
            {
                Debug.Log("Copied and hovering over nucleotide!");
                List<GameObject> nucleotides = GetNucleotides(s_copied, go);
                if (IsValid(nucleotides))
                {
                    Debug.Log("Copied and highlighting valid nucleotides!");
                    UnhighlightNucleotideSelection(s_currNucleotides);
                    s_currNucleotides = nucleotides;
                    HighlightNucleotideSelection(s_currNucleotides);
                }
            }
        }

        bool triggerValue;
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue && triggerReleased && pasting)
        {
            triggerReleased = false;
            if (s_currNucleotides != null)
            {
                UnhighlightNucleotideSelection(s_currNucleotides);

                // TODO: Make new xover list
                List<GameObject> xovers = new List<GameObject>();
                for (int i = 1; i < s_currNucleotides.Count; i++)
                {
                    if (!s_currNucleotides[i - 1].GetComponent<NucleotideComponent>().IsBackbone 
                        && !s_currNucleotides[i].GetComponent<NucleotideComponent>().IsBackbone)
                    {
                        GameObject xover = DrawCrossover.CreateXover(s_currNucleotides[i - 1], s_currNucleotides[i]);
                        xovers.Add(xover);
                    } 
                }

                Debug.Log("Drawing copied strand!");
                DrawNucleotideDynamic.DoCreateStrand(s_currNucleotides, xovers, s_numStrands);
            }
            Reset();
        }

        // Resets trigger button.                                            
        if (!(_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue))
        {
            triggerReleased = true;
        }

        // Resets primary button.                                            
        if (!(_device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue)
            && primaryValue))
        {
            primaryReleased = true;
        }

        // Resets secondary button.                                            
        if (!(_device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryValue)
            && secondaryValue))
        {
            secondaryReleased = true;
        }

    }

    public void Reset()
    {
        s_currNucleotides = null;
        pasting = false;
        s_copied = null;
        s_newEndpoints = null;
        Debug.Log("Reset!");

    }

    public static List<GameObject> GetNucleotides(Strand strand, GameObject newGO)
    {
        if (strand.GetHead().GetComponent<NucleotideComponent>().Direction != newGO.GetComponent<NucleotideComponent>().Direction)
        {
            return null;
        }

        List<GameObject> nucleotides = new List<GameObject>();
        List<(int, int)> xyDistances = new List<(int, int)>();
        List<(GameObject, GameObject)> endpoints = new List<(GameObject, GameObject)>();
        GameObject head = strand.GetHead();
        Helix helix = s_helixDict[head.GetComponent<NucleotideComponent>().HelixId];
        GridPoint gp = helix._gridComponent.GridPoint;
        int x = gp.X;
        int y = gp.Y;


        // Adds start and end point of each substrand to endpoints list.
        if (strand.Xovers.Count > 0)
        {
            endpoints.Add((head, strand.Xovers[0].GetComponent<XoverComponent>().PrevGO));
        }
        for (int i = 0; i < strand.Xovers.Count - 1; i++)
        {
            endpoints.Add((strand.Xovers[i].GetComponent<XoverComponent>().NextGO, strand.Xovers[i+1].GetComponent<XoverComponent>().PrevGO));
        }
        if (strand.Xovers.Count > 0)
        {
            endpoints.Add((strand.Xovers.Last().GetComponent<XoverComponent>().NextGO, strand.GetTail()));
        }

        // Calculates distances between each strand segment's gridPoint and the start segment's.
        xyDistances.Add((0, 0));
        for (int i = 0; i < strand.Xovers.Count; i++) {
            GameObject go = strand.Xovers[i].GetComponent<XoverComponent>().NextGO;
            helix = s_helixDict[go.GetComponent<NucleotideComponent>().HelixId];
            gp = helix._gridComponent.GridPoint;
            (int, int) distance = (gp.X - x, gp.Y - y);
            xyDistances.Add(distance);
        }


        Helix newHelix = s_helixDict[newGO.GetComponent<NucleotideComponent>().HelixId];
        GridPoint newGP = newHelix._gridComponent.GridPoint;
        Grid grid = newHelix._gridComponent.Grid;
        int newX = newGP.X;
        int newY = newGP.Y;
        int offset = newGO.GetComponent<NucleotideComponent>().Id - head.GetComponent<NucleotideComponent>().Id;

        Debug.Log("Offset: " + offset);

        for (int i = 0; i < xyDistances.Count; i++)
        {
            int tempX = newX + xyDistances[i].Item1;
            int tempY = newY + xyDistances[i].Item2;
            int indexX = grid.GridXToIndex(tempX);
            int indexY = grid.GridYToIndex(tempY);
            GridComponent gc = grid.Grid2D[indexX, indexY];
            if (!gc.Selected)
            {
                return null;
            }

            // Grab nucleotides from helix object. Need to get indices for each one
            Debug.Log("EndPoint 1: " + endpoints[i].Item1.GetComponent<NucleotideComponent>().Id + " -- EndPoint 2: " + endpoints[i].Item2.GetComponent<NucleotideComponent>().Id);
            List<GameObject> subNucleotides = GetSubList(endpoints[i].Item1, endpoints[i].Item2, gc, offset);
            //CreateEndPoints(endpoints[i].Item1, endpoints[i].Item2, gc, offset);
            nucleotides.AddRange(subNucleotides);
        }
        return nucleotides;
    }

/*    public static void CreateEndPoints(GameObject start, GameObject end, GridComponent gc, int offset)
    {
        int direction = start.GetComponent<NucleotideComponent>().Direction;
        int startId = GetNewIndex(start.GetComponent<NucleotideComponent>().Id, offset);
        int endId = GetNewIndex(end.GetComponent<NucleotideComponent>().Id, offset);

        GameObject startNucl = gc.Helix.GetNucleotide(startId, direction);
        GameObject endNucl = gc.Helix.GetNucleotide(endId, direction);
        s_newEndpoints.Add((startNucl, endNucl));
    }*/

    public static List<GameObject> GetSubList(GameObject start, GameObject end, GridComponent gc, int offset)
    {
        int direction = start.GetComponent<NucleotideComponent>().Direction;
        int startId = GetNewIndex(start.GetComponent<NucleotideComponent>().Id, offset);
        int endId = GetNewIndex(end.GetComponent<NucleotideComponent>().Id, offset);
        if (startId < endId)
        {
            // Check what happens if id's are out of bounds (bigger than number of nucleotides in helix that currently exist)
            return gc.Helix.GetHelixSub(startId, endId, direction);
        }
        return gc.Helix.GetHelixSub(endId, startId, direction);
    }

    public static int GetNewIndex(int origIndex, int offset)
    {
        return origIndex + offset;
    }

    public static bool IsValid(List<GameObject> nucleotides)
    {
        if (nucleotides == null) { return false; }

        Debug.Log("Start checking validness");

        foreach (GameObject nucleotide in nucleotides)
        {
            if (nucleotide == null)
            {
                return false;
            }
            
            var ntc = nucleotide.GetComponent<NucleotideComponent>();
            if (ntc.Selected)
            {
                return false;
            }
        }

        Debug.Log("Finish checking validness");

        return true;
    }
}
