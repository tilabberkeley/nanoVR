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

public class DrawLoopout : MonoBehaviour
{
    private const int DEFAULT_LENGTH = 1;

    // Device and UI fields
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;

    private bool triggerReleased = true;
    private bool gripReleased = true;
    private static RaycastHit s_hit;
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
        tempXover = Instantiate(Xover, Vector3.zero, Quaternion.identity) as GameObject;
        tempXover.SetActive(false);
    }

    private void Update()
    {
        if (!(s_loopoutOn || s_eraseTogOn) || s_hideStencils)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

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
                    DoCreateLoopout(s_startNuc, s_endNuc);
                    ResetNucleotides();
                }
            }
            else if (hit.collider.GetComponent<LoopoutComponent>()!= null)
            {
                // If the hit is on an existing crossover (and not a loopout), erase it.
                DoEraseLoopout(hit.collider.GetComponent<LoopoutComponent>());
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
    /// Does a loopout command.
    /// </summary>
    public static void DoCreateLoopout(NucleotideData first, NucleotideData second)
    {
        if (!DrawCrossover.IsValid(first, second))
        {
            return;
        }
        ICommand command = new LoopoutCommand(first, second, DEFAULT_LENGTH);
        CommandManager.AddCommand(command);
    }

    public static LoopoutComponent CreateLoopoutHelper(Domain prevDomain, Domain nextDomain, int strandId, int loopoutLength, int prevStrandId = -1, bool showXover = true)
    {
        // Create crossover, assign appropiate prev and next properties.
        GameObject loopout = DrawPoint.MakeLoopout(prevDomain, nextDomain);
        //Debug.Log("Finished creating loopout");
        LoopoutComponent loopoutComponent = loopout.AddComponent<LoopoutComponent>();
        loopoutComponent.PrevNucl = prevDomain.GetTailData();
        loopoutComponent.NextNucl = nextDomain.GetHeadData();
        loopoutComponent.SequenceLength = loopoutLength;
        loopoutComponent.StrandId = strandId;
        loopoutComponent.PrevStrandId = prevStrandId;

        prevDomain.NextXover = loopoutComponent;
        nextDomain.PrevXover = loopoutComponent;

        prevDomain.GetTailData().Xover = loopoutComponent;
        nextDomain.GetHeadData().Xover = loopoutComponent;

        loopoutComponent.Color = prevDomain.Color;
        loopoutComponent.SavedColor = nextDomain.Color;

        // Adds xover to each endpoint's helix
        prevDomain.GetHelix().AddXover(loopoutComponent);
        nextDomain.GetHelix().AddXover(loopoutComponent);

        loopoutComponent.IsLoopout = true;

        loopout.SetActive(showXover);
        return loopoutComponent;
    }

    public static LoopoutComponent CreateLoopout(NucleotideData nd1, NucleotideData nd2, int length)
    {
        if (!DrawCrossover.IsValid(nd1, nd2))
            return null;

        DrawCrossover.CalcPrevNextDomains(nd1, nd2, out Domain prevDomain, out Domain nextDomain);
        LoopoutComponent loopComp = CreateLoopoutHelper(prevDomain, nextDomain, nd1.StrandId, length, prevStrandId: nd2.StrandId);
        DrawCrossover.MergeStrand(nd1, nd2);
        return loopComp;
    }

    /// <summary>
    /// Does a erase loopout command.
    /// </summary>
    public static void DoEraseLoopout(LoopoutComponent loopout)
    {
        ICommand command = new EraseLoopoutCommand(loopout);
        CommandManager.AddCommand(command);
    }
}
