/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Highlight;

/// <summary>
/// Copy-paste an arbitrary number of Strands.
/// </summary>
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
    private static List<Strand> s_copied = new List<Strand>();
    private static RaycastHit s_hit;
    private static List<GameObject> s_currNucleotides = new List<GameObject>();
    private static List<List<GameObject>> newStrandNucls = new List<List<GameObject>>();
    private static List<List<(int, int)>> insertions = new List<List<(int, int)>>();
    private static List<List<int>> deletions = new List<List<int>>();
    private static List<List<(bool, int)>> isXovers = new List<List<(bool, int)>>();

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
        /*
        if (!s_drawTogOn && !s_eraseTogOn)
        {
            return;
        }*/

        if (!_device.isValid)
        {
            GetDevice();
        }

        if (_device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryValue) && primaryValue && primaryReleased && !pasting)
        {
            primaryReleased = false;
            Debug.Log("cop7ing selected strands");

            s_copied = SelectStrand.Strands;
            Debug.Log("copied");

            // Keep track of insertions/deletions
            foreach (Strand strand in s_copied)
            {
                insertions.Add(strand.Insertions);
                deletions.Add(strand.Deletions);
            }
            Debug.Log("finished insertion/deletion tracking");
        }

        if (_device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryValue)
            && secondaryValue
            && secondaryReleased)
        {
            secondaryReleased = false;
            if (s_copied.Count > 0)
            {
                pasting = true;
                Debug.Log("pasting");
            }
        }

        /*if (pasting && !rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            UnhighlightNucleotideSelection(s_currNucleotides, false);
        }*/

        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);
        if (pasting && rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            GameObject go = s_hit.collider.gameObject;
            NucleotideComponent ntc = go.GetComponent<NucleotideComponent>();
            if (ntc != null)
            {
                DrawNucleotideDynamic.ExtendIfLastNucleotide(ntc);
                Highlight.UnhighlightNucleotideSelection(s_currNucleotides, false);
                bool allValid = true;
                foreach (Strand strand in s_copied)
                {
                    List<GameObject> nucleotides = GetNucleotides(strand, go);
                    bool valid = IsValid(nucleotides);
                    allValid &= valid;

                    // Can't add null to list
                    if (valid) {
                        s_currNucleotides.AddRange(nucleotides);
                    }
                    newStrandNucls.Add(nucleotides);
                    Highlight.HighlightNucleotideSelection(nucleotides, valid);
                }

                if (triggerValue && allValid)
                {
                    triggerReleased = false;
                    Highlight.UnhighlightNucleotideSelection(s_currNucleotides, false);
                    for (int i = 0; i < newStrandNucls.Count; i++)
                    {

                        List<GameObject> nucls = newStrandNucls[i];
                        List<(int, int)> insertion = insertions[i];
                        List<(GameObject, int)> newInsertions = new List<(GameObject, int)>();
                        List<GameObject> newDeletions = new List<GameObject>();
                        List<int> deletion = deletions[i];
                        int strandId = s_numStrands;
                        for (int j = 0; j < insertion.Count; j++)
                        {
                            int index = insertion[j].Item1;
                            GameObject insertionGO = nucls[index];
                            newInsertions.Add((insertionGO, insertion[j].Item2));
                        }
                        for (int j = 0; j < deletion.Count; j++)
                        {
                            int index = deletion[j];
                            GameObject deletionGO = nucls[index];
                            newDeletions.Add(deletionGO);
                        }
                        Strand strand = Utils.CreateStrand(nucls, strandId, s_copied[i].Color, newInsertions, newDeletions, s_copied[i].Sequence, s_copied[i].IsScaffold);
                        List<(bool, int)> isXover = isXovers[i];
                        int xoverCount = 0;
                        //Make new xover list
                        for (int j = 1; j < nucls.Count; j++)
                        {
                            DNAComponent prevComp = nucls[j - 1].GetComponent<DNAComponent>();
                            DNAComponent nextComp = nucls[j].GetComponent<DNAComponent>();
                            if (!prevComp.IsBackbone
                                    && !nextComp.IsBackbone)
                            {
                                if (isXover[xoverCount].Item1)
                                {
                                    DrawCrossover.CreateXoverHelper(nucls[j - 1], nucls[j]);
                                }
                                else
                                {
                                    DrawLoopout.CreateLoopoutHelper(nucls[j - 1], nucls[j], isXover[xoverCount].Item2);
                                }
                                xoverCount += 1;
                            }
                        }
                        strand.Sequence = s_copied[i].Sequence; 
                    }
                    Reset();
                }
            }
        }

        if (triggerValue && triggerReleased && !rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
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
        s_currNucleotides.Clear();
        pasting = false;
        //s_copied.Clear();
    }

    public static List<GameObject> GetNucleotides(Strand strand, GameObject newGO)
    {
        /*if (strand.Head.GetComponent<NucleotideComponent>().Direction != newGO.GetComponent<NucleotideComponent>().Direction)
        {
            return null;
        }*/

        List<GameObject> nucleotides = new List<GameObject>();
        List<(GameObject, GameObject)> endpoints = new List<(GameObject, GameObject)>();
        GameObject head = strand.Head;
        Helix helix = s_helixDict[head.GetComponent<NucleotideComponent>().HelixId];
        GridPoint gp = helix._gridComponent.GridPoint;
        int newDirection = newGO.GetComponent<DNAComponent>().Direction;
        int x = gp.X;
        int y = gp.Y;
        List<(bool, int)> isXover = new List<(bool, int)>();

        // Adds start and end point of each substrand to endpoints list.

        // NOTE: Change this to domains!!! DY 9/12
        if (strand.Xovers.Count > 0)
        {
            endpoints.Add((head, strand.Xovers[0].GetComponent<XoverComponent>().PrevGO));
            if (strand.Xovers[0].GetComponent<XoverComponent>().IsXover)
            {
                isXover.Add((true, 0));
            }
            else
            {
                isXover.Add((false, strand.Xovers[0].GetComponent<LoopoutComponent>().SequenceLength));
            }
            for (int i = 0; i < strand.Xovers.Count - 1; i++)
            {
                // Determine if we're copying xover or loopout
                if (strand.Xovers[i].GetComponent<XoverComponent>().IsXover)
                {
                    isXover.Add((true, 0));
                }
                else
                {
                    isXover.Add((false, strand.Xovers[i].GetComponent<LoopoutComponent>().SequenceLength));
                }
                endpoints.Add((strand.Xovers[i].GetComponent<XoverComponent>().NextGO, strand.Xovers[i + 1].GetComponent<XoverComponent>().PrevGO));
            }
            endpoints.Add((strand.Xovers.Last().GetComponent<XoverComponent>().NextGO, strand.Tail));
            if (strand.Xovers.Last().GetComponent<XoverComponent>().IsXover)
            {
                isXover.Add((true, 0));
            }
            else
            {
                isXover.Add((false, strand.Xovers.Last().GetComponent<LoopoutComponent>().SequenceLength));
            }
        }
        else
        {
            endpoints.Add((head, strand.Tail));
        }

        isXovers.Add(isXover);

        // Calculates distances between each strand segment's gridPoint and the start segment's.
        List<(int, int)> xyDistances = CalculateXYDistances(strand, x, y);

        Helix newHelix = s_helixDict[newGO.GetComponent<NucleotideComponent>().HelixId];
        GridPoint newGP = newHelix._gridComponent.GridPoint;
        DNAGrid grid = newHelix._gridComponent.Grid;
        int newX = newGP.X;
        int newY = newGP.Y;
        int offset = newGO.GetComponent<NucleotideComponent>().Id - head.GetComponent<NucleotideComponent>().Id;

        for (int i = 0; i < xyDistances.Count; i++)
        {
            int tempX = newX + xyDistances[i].Item1;
            int tempY = newY + xyDistances[i].Item2;
            int indexX = grid.GridXToIndex(tempX);
            int indexY = grid.GridYToIndex(tempY);
            GridComponent gc = grid.Grid2D[indexX, indexY];
            if (gc == null || !gc.Selected)
            {
                return null;
            }

            List<GameObject> subNucleotides = GetSubList(endpoints[i].Item1, endpoints[i].Item2, gc, offset, newDirection);
            if (subNucleotides == null)
            {
                return null;
            }
            nucleotides.AddRange(subNucleotides);
        }
        return nucleotides;
    }

    public static List<GameObject> GetSubList(GameObject start, GameObject end, GridComponent gc, int offset, int newDirection)
    {
        int startId = GetNewIndex(start.GetComponent<NucleotideComponent>().Id, offset);
        int endId = GetNewIndex(end.GetComponent<NucleotideComponent>().Id, offset);
        if (startId < endId)
        {
            return gc.Helix.GetHelixSub(startId, endId, newDirection);
        }
        return gc.Helix.GetHelixSub(endId, startId, newDirection);
    }

    public static int GetNewIndex(int origIndex, int offset)
    {
        return origIndex + offset;
    }

    public static bool IsValid(List<GameObject> nucleotides)
    {
        if (nucleotides == null) { return false; }

        foreach (GameObject nucleotide in nucleotides)
        {
            if (nucleotide == null)
            {
                return false;
            }
            
            var ntc = nucleotide.GetComponent<DNAComponent>();
            if (ntc.Selected)
            {
                return false;
            }
        }
        return true;
    }

    private static List<(int, int)> CalculateXYDistances(Strand strand, int x, int y)
    {
        List<(int, int)> xyDistances = new List<(int, int)>
        {
            (0, 0) // NOTE: Fix this since it won't be (0, 0) for strands besides the original strand
        };
        for (int i = 0; i < strand.Xovers.Count; i++)
        {
            GameObject go = strand.Xovers[i].GetComponent<XoverComponent>().NextGO;
            Helix helix = s_helixDict[go.GetComponent<NucleotideComponent>().HelixId];
            GridPoint gp = helix._gridComponent.GridPoint;
            (int, int) distance = (gp.X - x, gp.Y - y);
            xyDistances.Add(distance);
        }
        return xyDistances;
    }

}
