/*
 * nanoVR, a VR application for DNA nanostructures.
 * authors: David Yang <davidmyang@berkeley.edu and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using static GlobalVariables;
using static Highlight;

public class DrawNucleotideDynamic : MonoBehaviour
{
    // Input controller variables
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    //[SerializeField] private GameObject canvas;
    private bool triggerReleased = true;
    bool triggerValue;

    // Helper variables
    private static GameObject s_startGO = null;
    private static GameObject s_endGO = null;
    private static RaycastHit s_hit;
    private static List<GameObject> s_currentNucleotides;
    bool creatingStrand = false;

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

        if (!s_drawTogOn && !s_eraseTogOn)
        {
            return;
        }

        if (s_visualMode) { return; }

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool gotTriggerValue = _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue);
        bool hitFound = rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit);

        // Set helper variables
        NucleotideComponent nucComp = null;
        bool hitIsNucleotide = false;
        GameObject hitGO = null;
        bool isStartNucleotide = false;
        bool isPrevNucleotide = false;
        if (hitFound)
        {
            nucComp = s_hit.transform.GetComponent<NucleotideComponent>();
            hitIsNucleotide = nucComp != null;
            hitGO = s_hit.collider.gameObject;
            isStartNucleotide = ReferenceEquals(hitGO, s_startGO);
            isPrevNucleotide = ReferenceEquals(hitGO, s_endGO);
        }

        if (hitIsNucleotide && creatingStrand)
        {
            ExtendIfLastNucleotide(nucComp);
        }

        // Handles first nucleotide selection
        if (gotTriggerValue && triggerValue && triggerReleased)
        {
            triggerReleased = false;
            if (s_startGO == null && hitFound && hitIsNucleotide)
            {
                creatingStrand = true;
                s_startGO = hitGO;
                s_currentNucleotides = MakeNuclList(s_startGO, s_startGO);
                HighlightNucleotideSelection(s_currentNucleotides, !s_eraseTogOn);
            }
        }
        // Holding down trigger, highlight current strand                                             
        else if (gotTriggerValue && triggerValue && !triggerReleased)
        {
            if (hitFound && hitIsNucleotide && !isStartNucleotide && !isPrevNucleotide && creatingStrand && IsValidNucleotideSelection(s_startGO, hitGO))
            {
                s_endGO = hitGO;
                UnhighlightNucleotideSelection(s_currentNucleotides, false);
                s_currentNucleotides = MakeNuclList(s_startGO, s_endGO);
                HighlightNucleotideSelection(s_currentNucleotides, !s_eraseTogOn);
            }
        }
        // Trigger is released, create strand                                       
        else if (gotTriggerValue && !triggerReleased && !triggerValue)
        {
            triggerReleased = true;
            if (creatingStrand)
            {
                UnhighlightNucleotideSelection(s_currentNucleotides, false);
                // Null if didn't select any other valid nucleotides
                if (s_endGO != null)
                {
                    if (s_drawTogOn)
                    {
                        BuildStrand();
                    }
                    else if (s_eraseTogOn)
                    {
                        DoEraseStrand();
                    }
                }
                ResetNucleotides();
                creatingStrand = false;
            }
            ResetNucleotides();
            creatingStrand = false;
        }
    }

    /// <summary>
    /// Resets the start and end nucleotides.
    /// </summary>
    public static void ResetNucleotides()
    {
        s_startGO = null;
        s_endGO = null;
    }

    /// <summary>
    /// Controls strand creation and editing.
    /// </summary>
    private void BuildStrand()
    {
        List<GameObject> nucleotides = MakeNuclList(s_startGO, s_endGO);
        if (nucleotides == null)
        {
            return;
        }

        // Checks that we are not drawing over another strand.
        for (int i = 1; i < nucleotides.Count - 1; i += 1)
        {
            DNAComponent ntc = nucleotides[i].GetComponent<DNAComponent>();
            if (ntc.Selected)
            {
                return;
            }
        }

        bool startSelected = s_startGO.GetComponent<DNAComponent>().Selected;
        bool endSelected = s_endGO.GetComponent<DNAComponent>().Selected;
        if (!startSelected && !endSelected)
        {
            DoCreateStrand(s_startGO, s_endGO, s_numStrands);
        }
        else if ((startSelected && !endSelected) || (!startSelected && endSelected))
        {
            DoEditStrand(s_startGO, s_endGO);
        }
    }

    /// <summary>
    /// Checks whether given nucleotides are on the same strand and direction in helix.
    /// </summary>
    /// <param name="start">Nucleotide GameObject</param>
    /// <param name="end">Nucleotide GameObject</param>
    /// <returns></returns>
    private static bool IsValidNucleotideSelection(GameObject start, GameObject end)
    {
        NucleotideComponent startNtc = start.GetComponent<NucleotideComponent>();
        NucleotideComponent endNtc = end.GetComponent<NucleotideComponent>();

        return startNtc.HelixId == endNtc.HelixId && startNtc.Direction == endNtc.Direction;
    }

    /// <summary>
    /// Returns a list of GameObjects that are in between the start and end GameObject
    /// selected by the user. These GameObjects are the nucleotides and backbones.
    /// </summary>
    /// <param name="start">GameObject that marks the beginning of nucleotide list.</param>
    /// <param name="end">GameObject that marks the end of nucleotide list.</param>
    /// <returns>Returns a list of GameObjects.</returns>
    public static List<GameObject> MakeNuclList(GameObject start, GameObject end)
    {
        NucleotideComponent startNtc = start.GetComponent<NucleotideComponent>();
        NucleotideComponent endNtc = end.GetComponent<NucleotideComponent>();
        if (!IsValidNucleotideSelection(start, end))
        {
            return null;
        }

        int startId = startNtc.Id;
        int endId = endNtc.Id;
        int helixId = startNtc.HelixId;
        int direction = startNtc.Direction;

        s_helixDict.TryGetValue(helixId, out Helix helix);
        if (startId < endId)
        {
            return helix.GetHelixSub(startId, endId, direction);
        }
        return helix.GetHelixSub(endId, startId, direction);
    }

    public static void DoCreateStrand(GameObject startGO, GameObject endGO, int strandId)
    {
        DoCreateStrand(MakeNuclList(startGO, endGO), strandId);
        //DoCreateStrand(MakeNuclList(startGO, endGO), new List<GameObject>(), strandId);

    }

    public static void DoCreateStrand(List<GameObject> nucleotides, int strandId)
    {
        ICommand command = new CreateCommand(nucleotides, strandId);
        CommandManager.AddCommand(command);
    }

    /*public static void DoCreateStrand(List<GameObject> nucleotides, List<GameObject> xovers, int strandId)
    {
        ICommand command = new CreateCommand(nucleotides, xovers, strandId);
        CommandManager.AddCommand(command);
        command.Do();
    }*/

    public void DoEditStrand(GameObject startGO, GameObject endGO)
    {
        ICommand command = new EditCommand(startGO, endGO);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Adds list of nucleotides to beginning or end of strand. Adjusts head/tail of strand
    /// and strand id/color of each nucleotide component accordingly.
    /// </summary>
    /// <param name="newNucls">List of nucleotides to add to strand. A nucleotide, either the 
    /// first or last GameObject in the list, is apart of a strand.</param>
    public static void EditStrand(GameObject startGO, GameObject endGO)
    {
        List<GameObject> newNucls = MakeNuclList(startGO, endGO);
        var startNtc = newNucls[0].GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        if (strandId == -1)
        {
            strandId = newNucls.Last().GetComponent<NucleotideComponent>().StrandId;
        }
        Strand strand = s_strandDict[strandId];

        if (newNucls.Last() == strand.Head)
        {
            // Add nucleotides to the beginning of 0 strand
            //newNucls.Remove(newNucls.Last());
            strand.AddToHead(newNucls.GetRange(0, newNucls.Count - 1));
        }
        else if (newNucls[0] == strand.Tail)
        {
            // Add nucleotides ot the end of 0 strand
            //newNucls.Remove(newNucls[0]);
            strand.AddToTail(newNucls.GetRange(1, newNucls.Count - 1));
        }
        // Remember to adjust each component
        strand.SetComponents();

        // Update strand dictionary
        // s_strandDict[strandId] = strand;
    }

    /// <summary>
    /// Handles strand deletions.
    /// </summary>
    public void DoEraseStrand()
    {
        if (s_startGO.GetComponent<NucleotideComponent>().Selected
            && s_endGO.GetComponent<NucleotideComponent>().Selected)
        {
            ICommand command = new EraseCommand(s_startGO, s_endGO);
            CommandManager.AddCommand(command);
        }
    }

    /// <summary>
    /// Deletes selected nucleotides from the strand. If all nucleotides are deleted,
    /// the strand itself is also deleted.
    /// </summary>
    /// <param name="nucleotides">List of nucleotides to delete from selected strand.</param>
    public static void EraseStrand(GameObject startGO, GameObject endGO)
    {
        // DEBUG THIS
        List<GameObject> nucleotides = MakeNuclList(startGO, endGO);
        var startNtc = nucleotides[0].GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        Strand strand = s_strandDict[strandId];

        if (nucleotides.Last() == strand.Tail && nucleotides[0] == strand.Head)
        {
            if (startGO == strand.Head)
            {
                strand.RemoveFromHead(nucleotides.GetRange(0, nucleotides.Count - 1));
            }
            else if (startGO == strand.Tail)
            {
                strand.RemoveFromTail(nucleotides.GetRange(1, nucleotides.Count - 1));
            }
        }

        else if (nucleotides.Last() == strand.Tail)
        {
            // Remove nucls from tail of strand with direction 0
            strand.RemoveFromTail(nucleotides.GetRange(1, nucleotides.Count - 1));
        }
        else if (nucleotides[0] == strand.Head)
        {
            // Remove nucls from head of strand with direction 0
            strand.RemoveFromHead(nucleotides.GetRange(0, nucleotides.Count - 1));
        }
    }


    /// <summary>
    /// Extends the helix of given nucleoltide if the nucleotide is last in the helix.
    /// </summary>
    /// <param name="nucComp">Nucleotide Component of the nucleotide game object.</param>
    public static void ExtendIfLastNucleotide(NucleotideComponent nucComp)
    {
        if (nucComp.IsEndHelix())
        {
            int helixId = nucComp.HelixId;
            s_helixDict.TryGetValue(helixId, out Helix helix);
            helix.ExtendAsync(64);
        }
    }
}