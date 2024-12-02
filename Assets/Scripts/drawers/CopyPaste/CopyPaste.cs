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
    private static GameObject s_go;
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
            //Debug.Log("cop7ing selected strands");

            s_copied = SelectStrand.Strands;
            //Debug.Log("copied");

            // Keep track of insertions/deletions
            foreach (Strand strand in s_copied)
            {
                insertions.Add(strand.Insertions);
                deletions.Add(strand.Deletions);
            }
            //Debug.Log("finished insertion/deletion tracking");
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
           /* if (go == s_go)
            {

                return;
            }*/
            //s_go = go;
            NucleotideComponent ntc = go.GetComponent<NucleotideComponent>();
            if (ntc != null)
            {
                bool allValid = true;

                if (s_go != go)
                {
                    Reset();
                    s_go = go;
                    DrawNucleotideDynamic.ExtendIfLastNucleotide(ntc);
                    //Highlight.UnhighlightNucleotideSelection(s_currNucleotides, false);
                    Debug.Log("Start checking highlighted strands");
                    foreach (Strand strand in s_copied)
                    {
                        List<GameObject> nucleotides = GetNucleotides(strand, go, s_copied[0].Head);
                        bool valid = Utils.IsValidNucleotides(nucleotides);
                        allValid &= valid;

                        if (!valid)
                        {
                            Debug.Log("Not valid resetting");

                            Reset();
                            break;
                        }
                        s_currNucleotides.AddRange(nucleotides);
                        newStrandNucls.Add(nucleotides);
                        Highlight.HighlightNucleotideSelection(nucleotides, valid);
                    }
                    Debug.Log("Finished looping through copied strands");
                } 

                if (triggerValue && allValid)
                {
                    Debug.Log("Trigger pulled and pasting");

                    triggerReleased = false;
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
            pasting = false;
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
        UnhighlightNucleotideSelection(s_currNucleotides, false);
        s_currNucleotides.Clear();
        newStrandNucls.Clear();
        //pasting = false;
        //s_copied.Clear();
    }

    public static List<GameObject> GetNucleotides(Strand strand, GameObject newGO, GameObject firstStrandHead)
    {
        /*if (s_copied[0].Head.GetComponent<NucleotideComponent>().Direction != newGO.GetComponent<NucleotideComponent>().Direction)
        {
            return null;
        }*/

        List<GameObject> nucleotides = new List<GameObject>();
        List<(GameObject, GameObject)> endpoints = new List<(GameObject, GameObject)>();
        bool differentDirection = newGO.GetComponent<DNAComponent>().Direction != firstStrandHead.GetComponent<DNAComponent>().Direction;
        List<(bool, int)> isXover = new List<(bool, int)>();

        /* Adds start and end point of each substrand to endpoints list. */

        // NOTE: Change this to domains!!! DY 9/12
        if (strand.Xovers.Count > 0)
        {
            endpoints.Add((strand.Head, strand.Xovers[0].GetComponent<XoverComponent>().PrevGO));
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
                /* Determine if we're copying xover or loopout */
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
        }
        else
        {
            endpoints.Add((strand.Head, strand.Tail));
        }
        isXovers.Add(isXover);

        /* Calculate distances between each strand segment's gridPoint and the start segment's. */
        List<(int, int)> xyDistances = CalculateXYDistances(strand, firstStrandHead);

        /* Get pasted position's GridPoint */
        Helix newHelix = s_helixDict[newGO.GetComponent<NucleotideComponent>().HelixId];
        GridPoint newGP = newHelix._gridComponent.GridPoint;
        DNAGrid grid = newHelix._gridComponent.Grid;
        int newX = newGP.X;
        int newY = newGP.Y;

        /* Calculate offset between each strand's starting index and new pasting idx */
        int firstStrandHeadIdx = firstStrandHead.GetComponent<NucleotideComponent>().Id;
        int newGOOffset = newGO.GetComponent<NucleotideComponent>().Id - strand.Head.GetComponent<NucleotideComponent>().Id;
        int firstStrandHeadOffset = firstStrandHeadIdx - strand.Head.GetComponent<NucleotideComponent>().Id;
        int offset = newGOOffset - firstStrandHeadOffset;

        /* Getting nucleotide list of new potentially pasted strand */
        for (int i = 0; i < xyDistances.Count; i++)
        {
            int tempX = newX + xyDistances[i].Item1;
            int tempY = newY + xyDistances[i].Item2;
            int indexX = grid.GridXToIndex(tempX);
            int indexY = grid.GridYToIndex(tempY);
            GridComponent gc = grid.Grid2D[indexX, indexY];
            Debug.Log($"tempX: {tempX}, tempY: {tempY}");

            if (gc == null || !gc.Selected)
            {
                Debug.Log("GC null or doesn't have helix");
                return null;
            }

            List<GameObject> subNucleotides = GetSubList(endpoints[i].Item1, endpoints[i].Item2, gc, offset, differentDirection);
            if (subNucleotides == null)
            {
                Debug.Log("subnucl list null");
                return null;
            }
            nucleotides.AddRange(subNucleotides);
        }
        return nucleotides;
    }

    public static List<GameObject> GetSubList(GameObject start, GameObject end, GridComponent gc, int offset, bool differentDirection)
    {
        int direction = start.GetComponent<NucleotideComponent>().Direction;
        int realDirection = differentDirection ? 1 - direction : direction;
        int startId = GetNewIndex(start.GetComponent<NucleotideComponent>().Id, offset);
        int endId = GetNewIndex(end.GetComponent<NucleotideComponent>().Id, offset);
        if (startId < endId)
        {
            return gc.Helix.GetHelixSub(startId, endId, realDirection);
        }
        return gc.Helix.GetHelixSub(endId, startId, realDirection);
    }

    public static int GetNewIndex(int origIndex, int offset)
    {
        return origIndex + offset;
    }

    

    private static List<(int, int)> CalculateXYDistances(Strand strand, GameObject firstStrandHead)
    {
        Helix firstDomainHelix = s_helixDict[strand.Head.GetComponent<NucleotideComponent>().HelixId];
        GridPoint firstDomainGP = firstDomainHelix._gridComponent.GridPoint;
        int fdX = firstDomainGP.X;
        int fdY = firstDomainGP.Y;

        Helix firstStrandHelix = s_helixDict[firstStrandHead.GetComponent<DNAComponent>().HelixId];
        GridPoint firstStrandGP = firstStrandHelix._gridComponent.GridPoint;
        int fsX = firstStrandGP.X;
        int fsY = firstStrandGP.Y;

        int dx = fdX - fsX;
        int dy = fdY - fsY;


        List<(int, int)> xyDistances = new List<(int, int)>
        {
            (dx, dy) // NOTE: Fix this since it won't be (0, 0) for strands besides the original strand
        };
        for (int i = 0; i < strand.Xovers.Count; i++)
        {
            GameObject go = strand.Xovers[i].GetComponent<XoverComponent>().NextGO;
            Helix helix = s_helixDict[go.GetComponent<NucleotideComponent>().HelixId];
            GridPoint gp = helix._gridComponent.GridPoint;
            (int, int) distance = (gp.X - fdX + dx, gp.Y - fdY + dy);
            Debug.Log($"XY distance: {distance}");
            xyDistances.Add(distance);
        }
        return xyDistances;
    }

}
