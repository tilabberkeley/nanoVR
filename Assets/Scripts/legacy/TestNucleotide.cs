/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

/*
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static GlobalVariables;

/// <summary>
/// Handles all interactions for strand creations, editing, and deletions.
/// </summary>
public class TestNucleotide : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] public XRRayInteractor rightRayInteractor;
    bool triggerReleased = true;
    GameObject highlightedGO = null;
    static GameObject s_startGO = null;
    static GameObject s_prevGO = null;
    static GameObject s_currGO = null;
    public static RaycastHit s_hit;

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
                // set currGO to whatever nucl is currently hit
                s_currGO = s_hit.collider.gameObject;
                if (s_startGO == null)
                {
                    s_startGO = s_hit.collider.gameObject;
                    if (!s_startGO.GetComponent<NucleotideComponent>().IsSelected())
                    {
                        CreateStrand();
                    }
                    else
                    {
                       // StartEraseStrand(s_st)
                    }
                }
                else
                {
                    EditStrand();
                }
            }
            else
            {
                ResetNucleotides();
            }
        }

        // HIGHLIGHT
        /*
        if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
                && !triggerValue
                && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            if (s_hit.collider.name.Contains("nucleotide"))
            {
                GameObject nt = s_hit.collider.gameObject;
                nt.GetComponent<NucleotideComponent>().Highlight();
                highlightedGO = nt;
            }
        } */

// UNHIGHLIGHT
/*
if (_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
        && !triggerValue
        && !rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
{
    if (highlightedGO != null)
    {
        highlightedGO.GetComponent<NucleotideComponent>().Unhighlight();
        highlightedGO = null;
    }
} 

// Resets triggers to avoid multiple selections.                                              
if ((_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
        && !triggerValue))
{
    triggerReleased = true;
    ResetNucleotides();
}

// Set prevGO to currGO
if ((_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
        && triggerValue
        && s_startGO != null))
{
    s_prevGO = s_currGO;
}
}

/// <summary>
/// Resets the start and end nucleotides.
/// </summary>
public static void ResetNucleotides()
{
s_startGO = null;
s_prevGO = null;
s_currGO = null;
}

/// <summary>
/// Controls strand creation and editing.
/// </summary>
public void BuildStrand()
{
List<GameObject> nucleotides = MakeNuclList(s_startGO, s_endGO);

if (!s_startGO.GetComponent<NucleotideComponent>().IsSelected()
    && !s_endGO.GetComponent<NucleotideComponent>().IsSelected())
{
    CreateStrand(nucleotides);
}
else if (s_startGO.GetComponent<NucleotideComponent>().IsSelected()
    && !s_endGO.GetComponent<NucleotideComponent>().IsSelected())
{
    EditStrand(nucleotides);
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
List<GameObject> nucleotides = new List<GameObject>();
var startNtc = start.GetComponent<NucleotideComponent>();
var endNtc = end.GetComponent<NucleotideComponent>();
if (startNtc.GetHelixId() != endNtc.GetHelixId()
    || startNtc.GetDirection() != endNtc.GetDirection()
    || (startNtc.GetStrandId() != endNtc.GetStrandId() && endNtc.GetStrandId() != -1))
{
    nucleotides.Add(start);
    return nucleotides;
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


public void CreateStrand()
{
var startNtc = s_startGO.GetComponent<NucleotideComponent>();
int direction = startNtc.GetDirection();

Strand strand = new Strand(s_startGO, s_numStrands, direction); ;
strand.SetComponents();
s_strandDict.Add(s_numStrands, strand);
s_numStrands++;
}

public void EditStrand()
{
var startNtc = s_startGO.GetComponent<NucleotideComponent>();
var currNtc = s_currGO.GetComponent<NucleotideComponent>();
var prevNtc = s_prevGO.GetComponent<NucleotideComponent>();
int startId = startNtc.GetStrandId();
Strand strand = s_strandDict[startId];

if (currNtc.GetId() < prevNtc.GetId())
{
    //strand.AddToLeft(newGO);
}
else
{
    //strand.AddToRight(newGO);
}
/*
else if (newNucls.Last() == strand.GetTail())
{
    // Add nucl to the end of 1 strand "beginning in indices"
    newNucls.Remove(newNucls.Last());
    strand.AddToLeftTail(newNucls);
}
else if (newNucls[0] == strand.GetHead())
{
    // Add nucls to the beginning of 1 strand "end in indices"
    newNucls.Remove(newNucls[0]);
    strand.AddToRightHead(newNucls);
} 


// Remember to adjust each component
strand.SetComponents();

// Update strand dictionary
s_strandDict[startId] = strand;
}

/// <summary>
/// Creates a new strand with it's own id, color, and list of nucleotides.
/// Adds new strand to the global strand dictionary.
/// </summary>
/// <param name="nucleotides">List of nucleotides to use in new strand.</param>
public void CreateStrand(List<GameObject> nucleotides)
{
var startNtc = s_startGO.GetComponent<NucleotideComponent>();
int direction = startNtc.GetDirection();

Strand strand;
if (direction == 0)
{
    strand = new Strand(nucleotides, s_numStrands, direction);
}
else
{
    strand = new Strand(nucleotides, s_numStrands, direction);
}
strand.SetComponents();
s_strandDict.Add(s_numStrands, strand);
s_numStrands++;
}

/// <summary>
/// Adds list of nucleotides to beginning or end of strand. Adjusts head/tail of strand
/// and strand id/color of each nucleotide component accordingly.
/// </summary>
/// <param name="newNucls">List of nucleotides to add to current strand.</param>
public void EditStrand(List<GameObject> newNucls)
{
var startNtc = s_startGO.GetComponent<NucleotideComponent>();
int strandId = startNtc.GetStrandId();
Strand strand = s_strandDict[strandId];

if (newNucls.Last() == strand.GetHead())
{
    // Add nucleotides to the beginning of 0 strand
    newNucls.Remove(newNucls.Last());
    strand.AddToHead(newNucls);
}
else if (newNucls[0] == strand.GetTail())
{
    // Add nucleotides ot the end of 0 strand
    newNucls.Remove(newNucls[0]);
    strand.AddToTail(newNucls);
}
/*
else if (newNucls.Last() == strand.GetTail())
{
    // Add nucl to the end of 1 strand "beginning in indices"
    newNucls.Remove(newNucls.Last());
    strand.AddToLeftTail(newNucls);
}
else if (newNucls[0] == strand.GetHead())
{
    // Add nucls to the beginning of 1 strand "end in indices"
    newNucls.Remove(newNucls[0]);
    strand.AddToRightHead(newNucls);
} 


// Remember to adjust each component
strand.SetComponents();

// Update strand dictionary
s_strandDict[strandId] = strand;
}

/// <summary>
/// Handles strand deletions.
/// </summary>
public void EraseStrand()
{
if (s_startGO.GetComponent<NucleotideComponent>().IsSelected()
    && s_endGO.GetComponent<NucleotideComponent>().IsSelected())
{
    List<GameObject> nucleotides = MakeNuclList(s_startGO, s_endGO);
    EraseStrand(nucleotides);
}
}


/// <summary>
/// Deletes selected nucleotides from the strand. If all nucleotides are deleted,
/// the strand itself is also deleted.
/// </summary>
/// <param name="nucleotides">List of nucleotides to delete from selected strand.</param>
public void EraseStrand(List<GameObject> nucleotides)
{
var startNtc = nucleotides[0].GetComponent<NucleotideComponent>();
int strandId = startNtc.GetStrandId();
Strand strand = s_strandDict[strandId];
int newStrandLength = -1;

if (nucleotides[0] == strand.GetHead())
{
    // Remove nucls from head of strand with direction 0
    newStrandLength = strand.RemoveFromHead(nucleotides);
}
else if (nucleotides.Last() == strand.GetTail())
{
    // Remove nucls from tail of strand with direction 0
    newStrandLength = strand.RemoveFromTail(nucleotides);
}

/*
else if (nucleotides[0] == strand.GetTail())
{
    // Remove nucls from tail of strand with direction 1
    newStrandLength = strand.RemoveFromRightHead(nucleotides);

}
else if (nucleotides.Last() == strand.GetHead())
{
    // Remove nucls from head of strand with direction 1
    newStrandLength = strand.RemoveFromLeftTail(nucleotides);
} 

if (newStrandLength != -1)
{
    strand.ResetComponents(nucleotides);
}
if (newStrandLength == 0)
{
    s_strandDict.Remove(strandId);
}
}
}
*/
