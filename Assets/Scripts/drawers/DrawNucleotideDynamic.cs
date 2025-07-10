/*
 * nanoVR, a VR application for DNA nanostructures.
 * authors: David Yang <davidmyang@berkeley.edu and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
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
    private static NucleotideData s_startGO = null;
    private static NucleotideData s_endGO = null;
    private static RaycastHit s_hit;
    private static List<NucleotideData> s_currentNucleotides;
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
        NucleotideData nd = null;
        bool hitIsNucleotide = false;
        bool isStartNucleotide = false;
        bool isPrevNucleotide = false;
        if (hitFound)
        {
            var comp = s_hit.transform.GetComponent<NucleotideColliderComponent>();
            if (comp != null)
            {
                nd = comp.Data;
            }
            hitIsNucleotide = nd != null;
            isStartNucleotide = ReferenceEquals(nd, s_startGO);
            isPrevNucleotide = ReferenceEquals(nd, s_endGO);
        }

        if (hitIsNucleotide && creatingStrand)
        {
            ExtendIfLastNucleotide(nd);
        }

        // Handles first nucleotide selection
        if (gotTriggerValue && triggerValue && triggerReleased)
        {
            triggerReleased = false;
            if (s_startGO == null && hitFound && hitIsNucleotide)
            {
                creatingStrand = true;
                s_startGO = nd;
                s_currentNucleotides = MakeNuclList(s_startGO, s_startGO);
                //HighlightNucleotideSelection(s_currentNucleotides, !s_eraseTogOn);
            }
        }
        // Holding down trigger, highlight current strand                                             
        else if (gotTriggerValue && triggerValue && !triggerReleased)
        {
            if (hitFound && hitIsNucleotide && !isStartNucleotide && !isPrevNucleotide && creatingStrand && IsValidNucleotideSelection(s_startGO, nd))
            {
                s_endGO = nd;
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
        List<NucleotideData> nucleotides = MakeNuclList(s_startGO, s_endGO);
        if (nucleotides == null)
        {
            return;
        }

        // Checks that we are not drawing over another strand.
        for (int i = 1; i < nucleotides.Count - 1; i += 1)
        {
            if (nucleotides[i].IsSelected())
            {
                return;
            }
        }

        bool startSelected = s_startGO.IsSelected();
        bool endSelected = s_endGO.IsSelected();
        if (!startSelected && !endSelected)
        {
            if (s_startGO.Id < s_endGO.Id)
                DoCreateStrand(s_startGO, s_endGO);
            else
                DoCreateStrand(s_endGO, s_startGO);
        }
        else if (startSelected && !endSelected)
        {
            DoEditStrand(s_startGO, s_endGO);
        }
        else if (startSelected && endSelected)
        {
            DoEraseStrand(s_startGO, s_endGO);
        }
    }

    /// <summary>
    /// Checks whether given nucleotides are on the same strand and direction in helix.
    /// </summary>
    private static bool IsValidNucleotideSelection(NucleotideData start, NucleotideData end)
    {
        return start.HelixId == end.HelixId && start.Direction == end.Direction;
    }

    /// <summary>
    /// Returns a list of nucleotides that are in between the start and end nucleotide
    /// selected by the user.
    /// </summary>
    public static List<NucleotideData> MakeNuclList(NucleotideData start, NucleotideData end)
    {
        if (!IsValidNucleotideSelection(start, end))
        {
            Debug.Log($"Not valid nucleotide selection. startId: {start.Id}, startHelix: {start.HelixId}; endId: {end.Id}, endHelix: {end.HelixId}");
            return null;
        }

        int startId = start.Id;
        int endId = end.Id;
        int helixId = start.HelixId;
        int direction = start.Direction;

        s_helixDict.TryGetValue(helixId, out Helix helix);
        if (startId < endId)
        {
            return helix.GetSubHelix(startId, endId, direction);
        }
        return helix.GetSubHelix(endId, startId, direction);
    }

    public static void DoCreateStrand(NucleotideData startNucl, NucleotideData endNucl)
    {
        ICommand command = new CreateCommand(startNucl, endNucl);
        CommandManager.AddCommand(command);
    }

    public static void CreateStrand(NucleotideData start, NucleotideData end, int strandId, Color color)
    {
        Domain domain = new Domain(start.HelixId, start.Direction, start.Id, end.Id, new Dictionary<int, int>(), new List<int>());
        Utils.CreateStrand(new List<Domain> { domain }, strandId, color);
    }

    public void DoEditStrand(NucleotideData start, NucleotideData end)
    {
        if (start.IsSelected() && !end.IsSelected()
            && (start.IsHead() || start.IsTail()))
        {
            ICommand command = new EditCommand(start, end);
            CommandManager.AddCommand(command);
        }
    }

    /// <summary>
    /// Adds list of nucleotides to beginning or end of strand. Adjusts head/tail of strand
    /// and strand id/color of each nucleotide component accordingly.
    /// </summary>
    /// <param name="newNucls">List of nucleotides to add to strand. A nucleotide, either the 
    /// first or last GameObject in the list, is apart of a strand.</param>
    public static void EditStrand(NucleotideData nd1, NucleotideData nd2)
    {
        int strandId = nd1.StrandId;

        if (strandId == -1)
        {
            strandId = nd2.StrandId;
        }

        Domain domain = nd1.GetDomain();

        if ((nd1.IsTail() && nd1.Direction == 1)
            || (nd1.IsHead() && nd1.Direction == 0))
        {
            domain.EndId = nd2.Id;
            if (nd1.IsTail())
            {
                domain.SetDomain(nd1.GetStrand().Domains.Count - 1, strandId, domain.Color, nd1.Id, nd2.Id);
            }
            else
            {
                domain.SetDomain(id: 0, strandId, domain.Color, nd1.Id, nd2.Id);
            }
        }
        else if ((nd1.IsHead() && nd1.Direction == 1)
            || (nd1.IsTail() && nd1.Direction == 0))
        {
            domain.StartId = nd2.Id;
            if (nd1.IsHead())
            {
                domain.SetDomain(id: 0, strandId, domain.Color, nd2.Id, nd1.Id);
            }
            else
            {
                domain.SetDomain(nd1.GetStrand().Domains.Count - 1, strandId, domain.Color, nd2.Id, nd1.Id);
            }
        }
    }

    /// <summary>
    /// Handles strand deletions.
    /// </summary>
    public void DoEraseStrand(NucleotideData start, NucleotideData end)
    {
        if (start.IsSelected() && end.IsSelected()
            && (start.IsHead() || start.IsTail()))
        {
            ICommand command = new EraseCommand(start, end);
            CommandManager.AddCommand(command);
        }
    }

    /// <summary>
    /// Deletes selected nucleotides from the strand.
    /// </summary>
    public static void EraseStrand(NucleotideData nd1, NucleotideData nd2)
    {
        Domain domain = nd1.GetDomain();

        if ((nd1.IsTail() && nd1.Direction == 1)
            || (nd1.IsHead() && nd1.Direction == 0))
        {
            domain.EndId = nd2.Id;
            domain.Reset(nd2.Id + 1, nd1.Id);
        }
        else if ((nd1.IsHead() && nd1.Direction == 1)
            || (nd1.IsTail() && nd1.Direction == 0))
        {
            domain.StartId = nd2.Id;
            domain.Reset(nd1.Id, nd2.Id - 1);
        }        
    }

    public static void ExtendIfLastNucleotide(NucleotideData nd)
    {
        if (nd.IsHelixEnd())
        {
            Helix helix = nd.GetHelix();
            helix.Extend(64);
        }
    }
}