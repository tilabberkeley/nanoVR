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

/// <summary>
/// Handles all interactions for strand creations, editing, and deletions.
/// </summary>
public class DrawNucleotide : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool triggerReleased = true;
    private static GameObject s_startGO = null;
    private static GameObject s_endGO = null;
    private static RaycastHit s_hit;

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

        // Handles start and end nucleotide selection.
        bool triggerValue;
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && triggerValue
                && triggerReleased
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            if (s_hit.collider.name.Contains("nucleotide"))
            {
                if (s_startGO == null)
                {   
                    s_startGO = s_hit.collider.gameObject;
                    DrawCrossover.Highlight(s_startGO, Color.green);
                }
                else
                {
                    s_endGO = s_hit.collider.gameObject;
                    DrawCrossover.Unhighlight(s_startGO);
                    
                    if (s_drawTogOn)
                    {
                        BuildStrand();
                    }
                    else if (s_eraseTogOn)
                    {
                        DoEraseStrand();
                    }
                    ResetNucleotides();
                }
            }
            else
            {
                ResetNucleotides();
            }
        }

        // Resets triggers to avoid multiple selections.                                              
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && !triggerValue)
        {
            triggerReleased = true;
        }

        // Resets start and end nucleotide.
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue
            && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            ResetNucleotides();
        }
    }

    /// <summary>
    /// Resets the start and end nucleotides.
    /// </summary>
    public static void ResetNucleotides()
    {
        if (s_startGO != null)
        {
            DrawCrossover.Unhighlight(s_startGO);
        }
        s_startGO = null;
        s_endGO = null;
    }

    /// <summary>
    /// Controls strand creation and editing.
    /// </summary>
    public void BuildStrand()
    {
        List<GameObject> nucleotides = MakeNuclList(s_startGO, s_endGO);
        if (nucleotides == null)
        {
            return;
        }

        if (!s_startGO.GetComponent<NucleotideComponent>().IsSelected()
            && !s_endGO.GetComponent<NucleotideComponent>().IsSelected())
        {
            DoCreateStrand(nucleotides, s_numStrands);
        }
        else if (s_startGO.GetComponent<NucleotideComponent>().IsSelected()
            && !s_endGO.GetComponent<NucleotideComponent>().IsSelected())
        {
            DoEditStrand(nucleotides);
        }
    }
    
    /// <summary>
    /// Returns a list of GameObjects that are in between the start and end GameObject
    /// selected by the user.
    /// </summary>
    /// <param name="start">GameObject that marks the beginning of nucleotide list.</param>
    /// <param name="end">GameObject that marks the end of nucleotide list.</param>
    /// <returns>Returns a list of GameObjects.</returns>
    public List<GameObject> MakeNuclList(GameObject start, GameObject end)
    {
        var startNtc = start.GetComponent<NucleotideComponent>();
        var endNtc = end.GetComponent<NucleotideComponent>();
        if (startNtc.GetHelixId() != endNtc.GetHelixId()
            || startNtc.GetDirection() != endNtc.GetDirection()
            || (startNtc.GetStrandId() != endNtc.GetStrandId() 
            && endNtc.GetStrandId() != -1))
        {
            return null;
        }
       
        int startId = startNtc.GetId();
        int endId = endNtc.GetId();
        int helixId = startNtc.GetHelixId();
        int direction = startNtc.GetDirection();

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
        s_numStrands += 1;
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
        int strandId = startNtc.GetStrandId();
        if (strandId == -1)
        {
            strandId = newNucls.Last().GetComponent<NucleotideComponent>().GetStrandId();
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
        if (s_startGO.GetComponent<NucleotideComponent>().IsSelected()
            && s_endGO.GetComponent<NucleotideComponent>().IsSelected())
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
        int strandId = startNtc.GetStrandId();
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
}
