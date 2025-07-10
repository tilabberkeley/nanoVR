/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang<davidmyang@berkeley.edu> and Oliver Petrick<odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

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
    private static NucleotideData s_nd;
    private static List<List<Domain>> s_newDomains = new List<List<Domain>>();
    bool allValid = true;

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

        if (!s_drawTogOn && !s_eraseTogOn)
        {
            return;
        }

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

        //if (pasting && !rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        //{
        //    UnhighlightNucleotideSelection(s_currNucleotides, false);
        //}

        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);
        if (pasting && rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            NucleotideColliderComponent ntc = s_hit.collider.GetComponent<NucleotideColliderComponent>();
            if (ntc != null)
            {
                if (s_nd != ntc.Data)
                {
                    Reset();
                    s_nd = ntc.Data;
                    DrawNucleotideDynamic.ExtendIfLastNucleotide(ntc.Data);
                    allValid = CheckAllPasteable(s_nd, s_copied, s_newDomains);
                }

                if (allValid)
                {
                    // Highlight nucls
                    foreach (List<Domain> domains in s_newDomains)
                    {
                        foreach (Domain domain in domains)
                        {
                            Highlight.HighlightDomain(domain, allValid);
                        }
                    }
                }
                else
                {
                    // Unhighlight nucls
                    foreach (List<Domain> domains in s_newDomains)
                    {
                        foreach (Domain domain in domains)
                        {
                            Highlight.UnhighlightDomain(domain, false);
                        }
                    }
                }

                if (triggerValue && allValid)
                {
                    Debug.Log("Trigger pulled and pasting");

                    triggerReleased = false;
                    for (int i = 0; i < s_copied.Count; i++)
                    {
                        Strand strand = Utils.CreateStrand(s_newDomains[i], s_numStrands, s_copied[i].Color, s_copied[i].IsScaffold, s_copied[i].GetLoopouts());
                        strand.SetSequenceRevamp(s_copied[i].Sequence);
                    }
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
        //UnhighlightNucleotideSelection(s_currNucleotides, false);
        // s_currNucleotides.Clear();
        s_newDomains.Clear();
        //pasting = false;
        //s_copied.Clear();
    }

    private bool CheckAllPasteable(NucleotideData newNucl, List<Strand> copiedStrands, List<List<Domain>> allNewDomains)
    {
        foreach (Strand strand in copiedStrands)
        {
            List<Domain> domains = new List<Domain>();
            if (!CheckPasteable(newNucl, strand, domains))
            {
                return false;
            }
            allNewDomains.Add(domains);
        }
        return true;
    }

    private bool CheckPasteable(NucleotideData newNucl, Strand strand, List<Domain> newDomains)
    {
        foreach (Domain domain in strand.Domains)
        {
            if (!CheckPasteableDomain(newNucl, domain, strand.GetHeadDomain(), newDomains))
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckPasteableDomain(NucleotideData newNucl, Domain currDomain, Domain firstDomain, List<Domain> newDomains)
    {
        if (newNucl.Direction != currDomain.Direction)
        {
            return false;
        }

        int domainLength = currDomain.EndId - currDomain.StartId;
        int newStartId, newEndId, newNuclPosOffset;

        if (newNucl.Direction == 1)
        {
            newNuclPosOffset = newNucl.Id - firstDomain.StartId;
            newStartId = currDomain.GetHeadData().Id + newNuclPosOffset;
            newEndId = newStartId + domainLength + newNuclPosOffset;
        }
        else
        {
            newNuclPosOffset = newNucl.Id - firstDomain.EndId;
            newEndId = currDomain.GetHeadData().Id + newNuclPosOffset;
            newStartId = newEndId - domainLength + newNuclPosOffset;
        }

        // Find new helix
        GridPoint firstGP = firstDomain.GetHelix()._gridComponent.GridPoint;
        GridPoint currGP = currDomain.GetHelix()._gridComponent.GridPoint;
        int deltaX = currGP.X - firstGP.X;
        int deltaY = currGP.Y - firstGP.Y;

        GridPoint newNuclGP = newNucl.GetHelix()._gridComponent.GridPoint;
        int newX = newNuclGP.X + deltaX;
        int newY = newNuclGP.Y + deltaY;

        DNAGrid grid = newNucl.GetHelix().GetGrid();
        int newIndexX = grid.GridXToIndex(newX);
        int newIndexY = grid.GridYToIndex(newY);

        Helix helix = grid.Grid2D[newIndexX, newIndexY]?.Helix;

        if (helix == null)
        {
            return false;
        }

        // Check nucleotides valid
        List<NucleotideData> newNucls = helix.GetSubHelix(newStartId, newEndId, newNucl.Direction);
        if (newNucls == null || newNucls.Count == 0)
        {
            return false;
        }

        foreach (NucleotideData nd in newNucls)
        {
            if (nd.IsSelected())
            {
                return false;
            }
        }

        Dictionary<int, int> newInsertions = new Dictionary<int, int>();
        List<int> newDeletions = new List<int>();

        foreach (var item in currDomain.Insertions)
        {
            newInsertions.Add(item.Key + newNuclPosOffset, item.Value);
        }
        foreach (int item in currDomain.Deletions)
        {
            newDeletions.Add(item + newNuclPosOffset);
        }

        Domain domain = new Domain(helix.Id, currDomain.Direction, newStartId, newEndId, newInsertions, newDeletions);
        newDomains.Add(domain);
        return true;
    }
}
