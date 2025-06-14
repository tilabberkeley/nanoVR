/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;
using static Utils;

/// <summary>
/// Handles crossovers and necessary strand operations using the new ray-mesh intersection code.
/// </summary>
public class DrawCrossover : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;

    private bool triggerReleased = true;
    private static bool drawTempXover = false;
    private static NucleotideData s_startNuc = null;
    private static NucleotideData s_endNuc = null;
    private static GameObject s_hitHelixGO;
    private static GameObject tempXover = null;

    private void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
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

    private void Awake()
    {
        // Create the temporary crossover visualization object.
        tempXover = Instantiate(Xover, Vector3.zero, Quaternion.identity) as GameObject;
        //tempXover.name = "xover";
        tempXover.SetActive(false);
    }

    private void Update()
    {
        if ((!s_drawTogOn && !s_eraseTogOn) || s_hideStencils)
            return;

        if (!_device.isValid)
            GetDevice();

        // Get trigger state.
        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);

        // Check if trigger is pressed and was previously released and if we have a valid raycast hit.
        if (triggerValue && triggerReleased && rightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            triggerReleased = false;
            s_hitHelixGO = hit.collider.gameObject;

            // Check for nucleotide collider.
            if (s_hitHelixGO.TryGetComponent<NucleotideColliderComponent>(out var nucComp))
            {
                //Debug.Log("Hit nucleotide collider");
                NucleotideData nd = nucComp.Data;
                if (nd == null)
                {
                    ResetNucleotides();
                }
                else if (s_startNuc == null)
                {
                    s_startNuc = nd;
                }
                else
                {
                    s_endNuc = nd;
                    if (s_drawTogOn)
                    {
                        DoCreateXover(s_startNuc, s_endNuc);
                        ResetNucleotides();
                    }
                }
            }
            else if (hit.collider.GetComponent<XoverComponent>() != null &&
                     hit.collider.GetComponent<LoopoutComponent>() == null &&
                     s_eraseTogOn)
            {
                // If the hit is on an existing crossover (and not a loopout), erase it.
                DoEraseXover(hit.collider.GetComponent<XoverComponent>());
            }
            else
            {
                ResetNucleotides();
            }
        }

        // Update the temporary xover visualization when trigger is not pressed.
        bool isHit = rightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit2);
        if (triggerReleased && !triggerValue && s_startNuc != null && isHit && hit2.collider.gameObject == s_hitHelixGO)
        {
            Domain domain = s_startNuc.GetDomain();
            if (s_startNuc == domain.GetHeadData() || s_startNuc == domain.GetTailData())
            {
                tempXover.SetActive(true);
                drawTempXover = true;
            }          
        }

        if (drawTempXover)
        {
            Vector3 startPos = s_startNuc.GetPosition();
            Vector3 currentPos = rightRayInteractor.transform.position + rightRayInteractor.transform.forward * 0.7f; // Use hit point or recalc from helix data.
            UpdateXover(startPos, currentPos);
        }

        if (!triggerValue)
        {
            triggerReleased = true;
        }

        // If trigger is pressed but there is no valid raycast hit, reset nucleotides.
        if (triggerValue && !rightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit _))
        {
            triggerReleased = false;
            ResetNucleotides();
        }
    }

    /// <summary>
    /// Updates the temporary crossover object's position, rotation, and scale.
    /// </summary>
    private void UpdateXover(Vector3 start, Vector3 end)
    {
        tempXover.transform.position = (start + end) / 2.0f;
        Vector3 dirV = Vector3.Normalize(end - start);
        tempXover.transform.rotation = Quaternion.FromToRotation(Vector3.up, dirV);
        float dist = Vector3.Distance(start, end);
        tempXover.transform.localScale = new Vector3(XOVER_RAD, dist, XOVER_RAD);
    }

    /// <summary>
    /// Resets the virtual nucleotide selections and hides the temporary crossover visualization.
    /// </summary>
    private static void ResetNucleotides()
    {
        s_startNuc = null;
        s_endNuc = null;
        tempXover.SetActive(false);
        drawTempXover = false;
    }

    /// <summary>
    /// Does a xover command.
    /// </summary>
    public static void DoCreateXover(NucleotideData first, NucleotideData second)
    {
        if (!IsValid(first, second))
        {
            return;
        }
        ICommand command = new XoverCommand(first, second);
        CommandManager.AddCommand(command);
    }

    public static bool IsValid(NucleotideData nd1, NucleotideData nd2)
    {
        if (nd1.StrandId == -1 || nd2.StrandId == -1)
            return false;

        if (nd1.Xover != null || nd2.Xover != null)
            return false;

        if (nd1.StrandId == nd2.StrandId && nd1.DomainIdx == nd2.DomainIdx)
            return false;

        if (nd1.InExtension || nd2.InExtension)
            return false;

        Domain d1 = nd1.GetDomain();
        Domain d2 = nd2.GetDomain();

        if ((nd1 == d1.GetHeadData() && nd2 == d2.GetTailData()) || (nd1 == d1.GetTailData() && nd2 == d2.GetHeadData()))
            return true;

        return false;
    }

    public static XoverComponent CreateXover(NucleotideData nd1, NucleotideData nd2)
    {
        Debug.Log("create xover start");
        if (!IsValid(nd1, nd2))
        {
            return null;
        }

        Debug.Log("Can draw xover");

        //DrawSplit.SplitStrand(startGO, s_numStrands, Strand.GetDifferentColor(firstNtc.Color), false);
        //DrawSplit.SplitStrand(endGO, s_numStrands, Strand.GetDifferentColor(secondNtc.Color), true);

        CalcPrevNextDomains(nd1, nd2, out Domain prevDomain, out Domain nextDomain);
        XoverComponent xover = CreateXoverHelper(prevDomain, nextDomain, nd1.StrandId, nd1.Color, nd2.Color, nd2.StrandId);
        MergeStrand(nd1, nd2);
        return xover;
        // Create circular strand
        //if (firstNtc.StrandId == secondNtc.StrandId)
        //{
        //    HandleCycle(startGO);
        //}
        //else
        //{
        //    MergeStrand(nd1, nd2);
        //}
    }

    /// <summary>
    /// Creates xover GameObject between two domains and sets the necessary properties.
    /// </summary>
    /// <param name="prevDomain">Domain that comes before xover in strand order.</param>
    /// <param name="nextDomain">Domain that comse after xover in strand order.</param>
    /// <param name="strandId"></param>
    /// <param name="prevStrandId"></param>
    /// <param name="showXover"></param>
    public static XoverComponent CreateXoverHelper(Domain prevDomain, Domain nextDomain, int strandId, Color color, Color savedColor, int prevStrandId = -1, bool showXover = true)
    {
        // Create crossover, assign appropiate prev and next properties.
        Transform gc = prevDomain.GetHelix()._gridComponent.transform;
        XoverComponent xoverComponent = DrawPoint.MakeXover(prevDomain, nextDomain, gc);
        xoverComponent.PrevNucl = prevDomain.GetTailData();
        xoverComponent.NextNucl = nextDomain.GetHeadData();
        xoverComponent.StrandId = strandId;
        xoverComponent.PrevStrandId = prevStrandId;

        prevDomain.NextXover = xoverComponent;
        nextDomain.PrevXover = xoverComponent;

        prevDomain.GetTailData().Xover = xoverComponent;
        nextDomain.GetHeadData().Xover = xoverComponent;

        xoverComponent.Color = prevDomain.Color;
        xoverComponent.SavedColor = nextDomain.Color;

        // Adds xover to each endpoint's helix
        prevDomain.GetHelix().AddXover(xoverComponent);
        nextDomain.GetHelix().AddXover(xoverComponent);

        xoverComponent.gameObject.SetActive(showXover);
        return xoverComponent;
    }

    public static void CalcPrevNextDomains(NucleotideData nd1, NucleotideData nd2, out Domain prevDomain, out Domain nextDomain)
    {
        Domain d1 = nd1.GetDomain();
        Domain d2 = nd2.GetDomain();

        if (d1.GetHeadData() == nd1 && d2.GetTailData() == nd2)
        {
            prevDomain = d2;
            nextDomain = d1;
        }
        else
        {
            prevDomain = d1;
            nextDomain = d2;
        }       
    }

    /// <summary>
    /// Does an erase crossover command.
    /// </summary>
    public static void DoEraseXover(XoverComponent xover)
    {
        ICommand command = new EraseXoverCommand(xover);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Removes given crossover and creates a new strand with given strand id and color due to crossover deletion.
    /// </summary>
    public static void EraseXover(XoverComponent xover, int strandId, Color color)
    {
        NucleotideData prevNucl = xover.PrevNucl;
        NucleotideData nextNucl = xover.NextNucl;
        Domain prevDomain = prevNucl.GetDomain();
        Domain nextDomain = nextNucl.GetDomain();
        prevDomain.NextXover = null;
        nextDomain.PrevXover = null;
        prevNucl.Xover = null;
        nextNucl.Xover = null;

        prevDomain.GetHelix().RemoveXover(xover);
        nextDomain.GetHelix().RemoveXover(xover);

        Strand strand = prevNucl.GetStrand();
        Utils.CreateStrandWithoutXovers(strand.Split(prevNucl), strandId, color);
    }

    public static void DeleteXover(XoverComponent xover)
    {
        NucleotideData prevNucl = xover.PrevNucl;
        NucleotideData nextNucl = xover.NextNucl;
        Domain prevDomain = prevNucl.GetDomain();
        Domain nextDomain = nextNucl.GetDomain();
        prevDomain.NextXover = null;
        nextDomain.PrevXover = null;
        prevNucl.Xover = null;
        nextNucl.Xover = null;

        prevDomain.GetHelix().RemoveXover(xover);
        nextDomain.GetHelix().RemoveXover(xover);

        GameObject.Destroy(xover.gameObject);
    }

    public static void MergeStrand(NucleotideData nd1, NucleotideData nd2)
    {
        Strand s1 = nd1.GetStrand();
        Strand s2 = nd2.GetStrand();
        Domain d1 = nd1.GetDomain();
        Domain d2 = nd2.GetDomain();

        if (d1.GetHeadData() == nd1 && d2.GetTailData() == nd2)
        {
            s1.AddToHead(s2.Domains);
            SelectStrand.RemoveStrand(nd2.StrandId);
            s1.SetDomainsRevamp();
        }
        else if (d1.GetTailData() == nd1 && d2.GetHeadData() == nd2)
        {
            s1.AddToTail(s2.Domains);
            SelectStrand.RemoveStrand(nd2.StrandId);
            s1.SetDomainsRevamp();
        }
    }

    /*private static void HandleCycle(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        s_strandDict.TryGetValue(ntc.StrandId, out Strand strand);
        strand.ShowHideCone(false);
        strand.IsCircular = true;
    }*/
}
