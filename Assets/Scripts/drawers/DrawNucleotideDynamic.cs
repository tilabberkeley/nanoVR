using System.Collections;
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
    [SerializeField] private GameObject canvas;
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
        if (!s_gridTogOn)
        {
            return;
        }

        if (!s_drawTogOn && !s_eraseTogOn)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool gotTriggerValue = _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue);
        bool hitFound = rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit);
        if (!hitFound)
        {
            return;
        }

        // Set helper variables
        NucleotideComponent nucComp = s_hit.transform.GetComponent<NucleotideComponent>();
        bool hitIsNucleotide = nucComp != null;
        GameObject hitGO = s_hit.collider.gameObject;
        bool isStartNucleotide = ReferenceEquals(hitGO, s_startGO);
        bool isPrevNucleotide = ReferenceEquals(hitGO, s_endGO);

        if(hitIsNucleotide)
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
                HighlightNucleotideSelection(s_currentNucleotides);
            }
        }
        // Holding down trigger, highlight current strand                                             
        else if (gotTriggerValue && triggerValue && !triggerReleased)
        {
            if (hitFound && hitIsNucleotide && !isStartNucleotide && !isPrevNucleotide && creatingStrand && isValidNucleotideSelection(s_startGO, hitGO))
            {
                s_endGO = hitGO;
                UnhighlightNucleotideSelection(s_currentNucleotides);
                s_currentNucleotides = MakeNuclList(s_startGO, s_endGO);
                HighlightNucleotideSelection(s_currentNucleotides);
            }
        }
        // Trigger is released, create strand                                       
        else if (!triggerReleased && !triggerValue)
        {
            triggerReleased = true;
            if (creatingStrand)
            {
                UnhighlightNucleotideSelection(s_currentNucleotides);
                if (s_drawTogOn)
                {
                    BuildStrand();
                }
                else if (s_eraseTogOn)
                {
                    DoEraseStrand();
                }
                ResetNucleotides();
                creatingStrand = false;
            }
        }
    }

    /// <summary>
    /// Resets the start and end nucleotides.
    /// </summary>
    private static void ResetNucleotides()
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

        if (!s_startGO.GetComponent<NucleotideComponent>().Selected
            && !s_endGO.GetComponent<NucleotideComponent>().Selected)
        {
            DoCreateStrand(nucleotides, s_numStrands);
        }
        else
        {
            DoEditStrand(nucleotides);
        }
    }

    /// <summary>
    /// Checks whether given nucleotides are on the same strand and direction in helix.
    /// </summary>
    /// <param name="start">Nucleotide GameObject</param>
    /// <param name="end">Nucleotide GameObject</param>
    /// <returns></returns>
    private static bool isValidNucleotideSelection(GameObject start, GameObject end)
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
    private static List<GameObject> MakeNuclList(GameObject start, GameObject end)
    {
        NucleotideComponent startNtc = start.GetComponent<NucleotideComponent>();
        NucleotideComponent endNtc = end.GetComponent<NucleotideComponent>();
        if (!isValidNucleotideSelection(start, end))
        {
            return null;
        }

        int startId = startNtc.Id;
        int endId = endNtc.Id;
        int helixId = startNtc.HelixId;
        int direction = startNtc.Direction;

        // CHANGE THIS SINCE HELICES CAN BE DELETED
        Helix helix = s_gridList[0].GetHelix(helixId);
        if (startId < endId)
        {
            return helix.GetHelixSub(startId, endId, direction);
        }
        return helix.GetHelixSub(endId, startId, direction);
    }

    public void DoCreateStrand(List<GameObject> nucleotides, int strandId)
    {
        ICommand command = new CreateCommand(nucleotides, strandId);
        CommandManager.AddCommand(command);
        command.Do();
    }

    /// <summary>
    /// Creates a new strand with it's own id, color, and list of nucleotides.
    /// Adds new strand to the global strand dictionary.
    /// </summary>
    /// <param name="nucleotides">List of nucleotides to use in new strand.</param>
    public static void CreateStrand(List<GameObject> nucleotides, int strandId)
    {
        Strand strand = new Strand(nucleotides, strandId);
        strand.SetComponents();
        s_strandDict.Add(strandId, strand);
        CreateButton(strandId);
        AddStrandToHelix(nucleotides[0]);
        s_numStrands += 1;
    }

    /// <summary>
    /// Adds strandId to corresponding helix object.
    /// </summary>
    /// <param name="go">Gameobject in strand.</param>
    public static void AddStrandToHelix(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        int helixId = ntc.HelixId;
        Helix helix = s_gridList[0].GetHelix(helixId);
        helix.AddStrandId(ntc.StrandId);
    }

    /// <summary>
    /// Creates strand button in UI corresponding to each strand object.
    /// </summary>
    /// <param name="strandId">Strand's id.</param>
    public static void CreateButton(int strandId)
    {
        ObjectListManager.CreateButton(strandId);
    }

    public void DoEditStrand(List<GameObject> newNucls)
    {
        ICommand command = new EditCommand(newNucls);
        CommandManager.AddCommand(command);
        command.Do();
    }

    /// <summary>
    /// Adds list of nucleotides to beginning or end of strand. Adjusts head/tail of strand
    /// and strand id/color of each nucleotide component accordingly.
    /// </summary>
    /// <param name="newNucls">List of nucleotides to add to current strand.</param>
    public static void EditStrand(List<GameObject> newNucls)
    {
        var startNtc = newNucls[0].GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        if (strandId == -1)
        {
            strandId = newNucls.Last().GetComponent<NucleotideComponent>().StrandId;
        }
        Strand strand = s_strandDict[strandId];

        if (newNucls.Last() == strand.GetHead())
        {
            // Add nucleotides to the beginning of 0 strand
            //newNucls.Remove(newNucls.Last());
            strand.AddToHead(newNucls.GetRange(0, newNucls.Count - 1));
        }
        else if (newNucls[0] == strand.GetTail())
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
            List<GameObject> nucleotides = MakeNuclList(s_startGO, s_endGO);
            if (nucleotides == null)
            {
                return;
            }
            ICommand command = new EraseCommand(nucleotides);
            CommandManager.AddCommand(command);
            command.Do();
        }
    }

    /// <summary>
    /// Deletes selected nucleotides from the strand. If all nucleotides are deleted,
    /// the strand itself is also deleted.
    /// </summary>
    /// <param name="nucleotides">List of nucleotides to delete from selected strand.</param>
    public static void EraseStrand(List<GameObject> nucleotides)
    {
        // DEBUG THIS
        var startNtc = nucleotides[0].GetComponent<NucleotideComponent>();
        int strandId = startNtc.StrandId;
        Strand strand = s_strandDict[strandId];

        if (nucleotides.Last() == strand.GetTail())
        {
            // Remove nucls from tail of strand with direction 0
            strand.RemoveFromTail(nucleotides.GetRange(1, nucleotides.Count - 1));
        }
        else if (nucleotides[0] == strand.GetHead())
        {
            // Remove nucls from head of strand with direction 0
            strand.RemoveFromHead(nucleotides.GetRange(0, nucleotides.Count - 1));
        }
    }

    /// <summary>
    /// Extends the helix of given nucleoltide if the nucleotide is last in the helix.
    /// </summary>
    /// <param name="nucComp">Nucleotide Component of the nucleotide game object.</param>
    private void ExtendIfLastNucleotide(NucleotideComponent nucComp)
    {
        if (nucComp.isEndNuclueotide())
        {
            int helixId = nucComp.HelixId;
            s_helixDict.TryGetValue(helixId, out Helix helix);
            helix.Extend();
        }
    }
}
