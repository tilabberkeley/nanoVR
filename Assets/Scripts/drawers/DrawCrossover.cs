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
/// Virtual representation of a nucleotide for ray-intersection selection.
/// </summary>
public struct VirtualNucleotide
{
    public Helix helix;  // Reference to the helix this nucleotide belongs to.
    public int index;    // Index within the helix’s nucleotide list.
    public int direction; // Direction of nucleotide (in helix list A or list B).

    /// <summary>
    /// Returns the world position of this nucleotide by extracting the translation
    /// component from its instance matrix. (Assumes strand A; modify if needed.)
    /// </summary>
    public Vector3 GetWorldPosition()
    {
        if (direction == 1)
            return helix.NucleotideMatricesA[index].GetColumn(3);
        return helix.NucleotideMatricesB[index].GetColumn(3);
    }
}

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

    // Instead of storing nucleotide GameObjects, we now store virtual nucleotides.
    private static NucleotideData s_startNuc = null;
    private static NucleotideData s_endNuc = null;
    // The helix collider GameObject that was hit.
    private static GameObject s_hitHelixGO;
    private static GameObject tempXover = null;

    // Radius for the ray-sphere test on nucleotides.
    [SerializeField] private float nucleotidePickRadius = 0.1f;

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
        tempXover.name = "xover";
        tempXover.SetActive(false);
    }

    private void Update()
    {
        if ((!s_drawTogOn && !s_eraseTogOn) || s_hideStencils)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        // Get trigger state.
        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue);

        // Use the XR ray interactor to get a hit.
        if (triggerValue && triggerReleased && rightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            triggerReleased = false;
            s_hitHelixGO = hit.collider.gameObject;

            // Instead of checking for a NucleotideComponent, check for a HelixComponent.
            HelixComponent helixComp = s_hitHelixGO.GetComponent<HelixComponent>();
            if (helixComp != null)
            {
                Debug.Log("Hit helix");
                Helix helix = helixComp.Helix; // The helix data object.
                                                // Use our new ray-mesh intersection to narrow down the nucleotide.
                NucleotideData nd = FindHitNucleotide(helix, rightRayInteractor);
                if (nd != null)
                {
                    Debug.Log("Hit nucleotide");
                    // If no start nucleotide has been selected, set it.
                    if (s_startNuc == null)
                    {
                        s_startNuc = nd;
                    }
                    else
                    {
                        // Otherwise, set the end nucleotide.
                        s_endNuc = nd;
                        if (s_drawTogOn)
                        {
                            CreateXover(s_startNuc, s_endNuc);
                            ResetNucleotides();
                        }
                    }
                }
                else
                {
                    ResetNucleotides();
                }
            }
            else if (hit.collider.GetComponent<XoverComponent>() != null
                && hit.collider.GetComponent<LoopoutComponent>() == null
                && s_eraseTogOn)
            {
                // If the hit is on an existing crossover (and not a loopout), erase it.
                DoEraseXover(hit.collider.gameObject);
            }
            else
            {
                ResetNucleotides();
            }
        }

        // While trigger is released, update the temporary xover visualization.
        if (triggerReleased && !triggerValue)
        {
            if (s_startNuc != null)
            {
                Debug.Log("drawing temp xover");
                Vector3 startPos = s_startNuc.GetPosition();
                // Use the hit point or recalc from helix data.
                Vector3 currentPos = rightRayInteractor.transform.forward;
                tempXover.SetActive(true);
                UpdateXover(startPos, currentPos);
            }
        }

        if (!triggerValue)
        {
            triggerReleased = true;
        }

        if (triggerValue && !rightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit _))
        {
            triggerReleased = false;
            ResetNucleotides();
        }
    }

    /// <summary>
    /// Iterates through the helix's nucleotide matrices to determine which nucleotide is hit.
    /// Returns the index of the best hit nucleotide, or -1 if none.
    /// </summary>
    private NucleotideData FindHitNucleotide(Helix helix, XRRayInteractor rayInteractor)
    {
        Ray ray = new Ray(rayInteractor.transform.position, rayInteractor.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance: 1f, rightRayInteractor.raycastMask, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));


        float bestT = float.MaxValue;
        NucleotideData nd = null;

        foreach (var hit in hits)
        {
            float hypotenuse = Vector3.Distance(hit.point, helix._gridComponent.transform.position);
            float distance = Mathf.Sqrt(hypotenuse * hypotenuse - RADIUS * RADIUS);
            int index = Mathf.RoundToInt(distance / RISE);

            Matrix4x4 nuclA = helix.GetNucleotideMesh(index, 1);
            Matrix4x4 nuclB = helix.GetNucleotideMesh(index, 0);

            float distA = Vector3.Distance(nuclA.GetColumn(3), hit.point);
            float distB = Vector3.Distance(nuclB.GetColumn(3), hit.point);

            if (distA < NUCL_RAD)
            {
                bestT = distA;
                nd = helix.GetNucleotideData(index, 1);
            }

            if (distB < NUCL_RAD && distB < distA)
            {
                bestT = distB;
                nd = helix.GetNucleotideData(index, 0);
            }

            if (nd != null)
            {
                XRInteractorLineVisual lineVisual = rightRayInteractor.GetComponent<XRInteractorLineVisual>();
                lineVisual.lineLength = bestT;
                break;
            }
        }

        return nd;
    }

    /// <summary>
    /// Performs a ray-sphere intersection test. Returns true if an intersection occurs and outputs the distance.
    /// </summary>
    private bool RaySphereIntersection(Ray ray, Vector3 center, float radius, out float t)
    {
        Vector3 oc = ray.origin - center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2.0f * Vector3.Dot(oc, ray.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4.0f * a * c;
        if (discriminant < 0)
        {
            t = -1;
            return false;
        }
        else
        {
            t = (-b - Mathf.Sqrt(discriminant)) / (2.0f * a);
            return t >= 0;
        }
    }

    /// <summary>
    /// Updates the temporary crossover object's position, rotation, and scale.
    /// </summary>
    private void UpdateXover(Vector3 start, Vector3 end)
    {
        Vector3 cylDefaultOrientation = new Vector3(0, 1, 0);
        tempXover.transform.position = (start + end) / 2.0f;
        Vector3 dirV = Vector3.Normalize(start - end);
        Vector3 rotAxisV = Vector3.Normalize(dirV + cylDefaultOrientation);
        tempXover.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);
        float dist = Vector3.Distance(start, end);
        tempXover.transform.localScale = new Vector3(0.005f, dist / 2.0f, 0.005f);
    }

    /// <summary>
    /// Resets the virtual nucleotide selections and hides the temporary crossover visualization.
    /// </summary>
    private static void ResetNucleotides()
    {
        s_startNuc = null;
        s_endNuc = null;
        tempXover.SetActive(false);
    }

    private static (NucleotideData, NucleotideData) GetNucleotideData(VirtualNucleotide vn1, VirtualNucleotide vn2)
    {
        NucleotideData nd1 = vn1.helix.GetNucleotideData(vn1.index, vn1.direction);
        NucleotideData nd2 = vn2.helix.GetNucleotideData(vn2.index, vn2.direction);

        return (nd1, nd2);
    }

    /// <summary>
    /// Returns whether there can be a crossover between the two inputted nucleotides.
    /// The nucleotides must be going in different directions and be apart of a strand.
    /// Addiionally, there can't be a crossover or loopout already on either of the
    /// nucleotides.
    /// </summary>
    /// <param name="startGO">First nucleotide game object.</param>
    /// <param name="endGO">Second nucleotide game object.</param>
    /// <returns>Whether there can be a crossover between the two nucleotide.</returns>
    public static bool IsValid(GameObject startGO, GameObject endGO)
    {
        var startNtc = startGO.GetComponent<NucleotideComponent>();
        int startId = startNtc.StrandId;
        int startDir = startNtc.Direction;

        var endNtc = endGO.GetComponent<NucleotideComponent>();
        int endId = endNtc.StrandId;
        int endDir = endNtc.Direction;

        if (startId == -1 || endId == -1)
        {
            return false;
        }

        if (startDir == endDir)
        {
            return false;
        }

        if (startNtc.Xover != null || endNtc.Xover != null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Does a xover command.
    /// </summary>
    public static void DoCreateXover(GameObject first, GameObject second)
    {
        if (!IsValid(first, second))
        {
            return;
        }
        Strand firstStr = Utils.GetStrand(first);
        Strand secondStr = Utils.GetStrand(second);

        // Bools help check if strands should merge with neighbors when xover is deleted or undo.
        bool firstIsEnd = first == firstStr.Head || first == firstStr.Tail;
        bool secondIsEnd = second == secondStr.Head || second == secondStr.Tail;
        bool firstIsHead = first == firstStr.Head;
        ICommand command = new XoverCommand(first, second, firstIsEnd, secondIsEnd, firstIsHead);
        CommandManager.AddCommand(command);
        //command.Do();
    }

    /// <summary>
    /// Splits strands (if necessary), draws xover, and merges strands connected by xover
    /// </summary>
    public static GameObject CreateXover(GameObject startGO, GameObject endGO)
    {
        if (!IsValid(startGO, endGO))
        {
            return null;
        }

        NucleotideComponent firstNtc = startGO.GetComponent<NucleotideComponent>();
        NucleotideComponent secondNtc = endGO.GetComponent<NucleotideComponent>();

        DrawSplit.SplitStrand(startGO, s_numStrands, Strand.GetDifferentColor(firstNtc.Color), false);
        DrawSplit.SplitStrand(endGO, s_numStrands, Strand.GetDifferentColor(secondNtc.Color), true);

        GameObject xover = CreateXoverHelper(startGO, endGO);

        // Create circular strand
        if (firstNtc.StrandId == secondNtc.StrandId)
        {
            HandleCycle(startGO);
        }
        else
        {
            MergeStrand(startGO, endGO, xover);
        }
        return xover;
    }

    /// <summary>
    /// Helper method to create a crossover between given nuleotides.
    /// </summary>
    public static GameObject CreateXoverHelper(GameObject startGO, GameObject endGO, bool showXover = true)
    {
        int strandId = startGO.GetComponent<NucleotideComponent>().StrandId;
        int prevStrandId = endGO.GetComponent<NucleotideComponent>().StrandId;

        // Create crossover, assign appropiate prev and next properties.
        Strand startStr = Utils.GetStrand(startGO);
        Strand endStr = Utils.GetStrand(endGO);
        GameObject prevGO = startGO;
        GameObject nextGO = endGO;
        if (startGO == startStr.Head)
        {
            nextGO = startGO;
        }
        if (endGO == endStr.Tail)
        {
            prevGO = endGO;
        }
        GameObject xover = DrawPoint.MakeXover(prevGO, nextGO, strandId, prevStrandId);
        xover.SetActive(showXover);
        return xover;
    }

    private static bool IsValid(NucleotideData nd1, NucleotideData nd2)
    {
        if (nd1.StrandId == -1 || nd2.StrandId == -1)
        {
            return false;
        }

        if (nd1.Xover != null || nd2.Xover != null)
        {
            return false;
        }

        Domain d1 = nd1.GetDomain();
        Domain d2 = nd2.GetDomain();

        if (nd1.Id != d1.StartId && nd1.Id != d1.EndId || nd2.Id != d2.StartId && nd2.Id != d2.EndId)
        {
            return false;
        }

        return true;
    }

    public static void CreateXover(NucleotideData nd1, NucleotideData nd2)
    {
        Debug.Log("create xover start");
        if (!IsValid(nd1, nd2))
        {
            return;
        }

        Debug.Log("Can draw xover");

        //DrawSplit.SplitStrand(startGO, s_numStrands, Strand.GetDifferentColor(firstNtc.Color), false);
        //DrawSplit.SplitStrand(endGO, s_numStrands, Strand.GetDifferentColor(secondNtc.Color), true);

        CalcPrevNextDomains(nd1, nd2, out Domain prevDomain, out Domain nextDomain);
        XoverComponent xover = CreateXoverHelper(prevDomain, nextDomain, nd1.StrandId, nd1.Color, nd2.Color, nd2.StrandId);

        // Create circular strand
        //if (firstNtc.StrandId == secondNtc.StrandId)
        //{
        //    HandleCycle(startGO);
        //}
        //else
        {
            MergeStrand(nd1, nd2);
        }
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
        GameObject xover = DrawPoint.MakeXover(prevDomain, nextDomain);
        XoverComponent xoverComponent = xover.GetComponent<XoverComponent>();
        xoverComponent.PrevDomainIdx = prevDomain.Id;
        xoverComponent.NextDomainIdx = nextDomain.Id;
        xoverComponent.StrandId = strandId;
        xoverComponent.PrevStrandId = prevStrandId;

        prevDomain.NextXover = xoverComponent;
        nextDomain.PrevXover = xoverComponent;

        xoverComponent.Color = color;
        xoverComponent.SavedColor = savedColor;

        xoverComponent.gameObject.transform.SetParent(prevDomain.GetHelix()._gridComponent.transform); // This helps with transformations
        xover.SetActive(showXover);
        return xoverComponent;
    }

    private static void CalcPrevNextDomains(NucleotideData nd1, NucleotideData nd2, out Domain prevDomain, out Domain nextDomain)
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
    /// Does a erase crossover command.
    /// </summary>
    public static void DoEraseXover(GameObject xover)
    {
        ICommand command = new EraseXoverCommand(xover, s_numStrands);
        CommandManager.AddCommand(command);
        //command.Do();
    }

    /// <summary>
    /// Removes given crossover and creates a new strand with given strand id and color due to crossover deletion.
    /// </summary>
    public static void EraseXover(GameObject xover, int strandId, Color color, bool splitBefore)
    {
        XoverComponent xoverComp = xover.GetComponent<XoverComponent>();
        GameObject nucleotide;

        if (splitBefore)
        {
            nucleotide = xoverComp.NextGO;
        }
        else
        {
            nucleotide = xoverComp.PrevGO;
        }
        Strand strand = s_strandDict[xoverComp.StrandId];
        strand.DeleteXover(xover);
        DrawSplit.SplitStrand(nucleotide, strandId, color, !splitBefore); // CHECK THIS
    }

    /* public static void SplitStrand(GameObject go, int id, Color color, bool splitAfter)
     {
         var startNtc = go.GetComponent<NucleotideComponent>();
         int strandId = startNtc.StrandId;
         s_strandDict.TryGetValue(strandId, out Strand strand);

         if (splitAfter)
         {
             CreateStrand(strand.SplitAfter(go), id, color);
         }
         else
         {
             CreateStrand(strand.SplitBefore(go), id, color);
         }
     }*/

    /// <summary>
    /// Splits strand at given nucleotide.
    /// </summary>
    public static void SplitStrand(GameObject nucleotide, int id, Color color, bool splitAfter)
    {
        var startNtc = nucleotide.GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        s_strandDict.TryGetValue(strandId, out Strand strand);

        if (splitAfter)
        {
            /*List<GameObject> xovers = strand.GetXoversBeforeIndex(goIndex);
            strand.RemoveXovers(xovers);*/
            CreateStrand(strand.SplitAfter(nucleotide), id, color);
        }
        else
        {
            /*List<GameObject> xovers = strand.GetXoversAfterIndex(goIndex);
            strand.RemoveXovers(xovers);*/
            /*List<GameObject> nucleotides = strand.SplitAfter(go);
            if (nucleotides.Count % 2 == 0) // Remove the trailing backbone
            {
                nucleotides.RemoveAt(nucleotides.Count - 1);
            }*/
            CreateStrand(strand.SplitBefore(nucleotide), id, color);
        }
    }

    /// <summary>
    /// Merges two strands specificed by the two given nucleotide gameobjects along with the given xover.
    /// (This differs from the standard merge by also considering a xover).
    /// </summary>
    public static void MergeStrand(GameObject firstGO, GameObject secondGO, GameObject xover)
    {
        var firstNtc = firstGO.GetComponent<NucleotideComponent>();
        var secondNtc = secondGO.GetComponent<NucleotideComponent>();
        var xoverComp = xover.GetComponent<XoverComponent>();
        int firstStrandId = firstNtc.StrandId;
        int secondStrandId = secondNtc.StrandId;
        Strand firstStrand = s_strandDict[firstStrandId];
        Strand secondStrand = s_strandDict[secondStrandId];

        if (firstStrand.Head == firstGO && secondStrand.Tail == secondGO)
        {
            xoverComp.PrevGO = secondGO;
            xoverComp.NextGO = firstGO;
            List<GameObject> nucleotides = secondStrand.Nucleotides;
            SelectStrand.RemoveStrand(nucleotides[0]);
            firstStrand.AddToHead(nucleotides);
        }
        else if (firstStrand.Tail == firstGO && secondStrand.Head == secondGO)
        {
            xoverComp.PrevGO = firstGO;
            xoverComp.NextGO = secondGO;
            List<GameObject> nucleotides = secondStrand.Nucleotides;
            SelectStrand.RemoveStrand(nucleotides[0]);
            firstStrand.AddToTail(nucleotides);
        }
        firstStrand.SetComponents();
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

    private static void HandleCycle(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        s_strandDict.TryGetValue(ntc.StrandId, out Strand strand);
        strand.ShowHideCone(false);
        strand.IsCircular = true;
    }

    /// <summary>
    /// Given two nucleotide game objects, the out variables are set such that the start variables hold the respective
    /// variables for the strand with the smallest strand id. The end variables hold the respective variables for the
    /// strand with the largest strand id.
    /// </summary>
    public static void SetNucleotideDirection(GameObject firstGO, GameObject secondGO,
        out GameObject startGO, out GameObject endGO, out Strand startStrand, out Strand endStrand)
    {
        NucleotideComponent firstNtc = firstGO.GetComponent<NucleotideComponent>();
        NucleotideComponent secondNtc = secondGO.GetComponent<NucleotideComponent>();
        int firstStrandId = firstNtc.StrandId;
        int secondStrandId = secondNtc.StrandId;

        // Strand with smallest id will be start GO. This strand will also be preserved with the merge.
        if (firstStrandId < secondStrandId)
        {
            startStrand = s_strandDict[firstStrandId];
            endStrand = s_strandDict[secondStrandId];

            startGO = firstGO;
            endGO = secondGO;
        }
        else
        {
            startStrand = s_strandDict[secondStrandId];
            endStrand = s_strandDict[firstStrandId];

            startGO = secondGO;
            endGO = firstGO;
        }
    }
}
