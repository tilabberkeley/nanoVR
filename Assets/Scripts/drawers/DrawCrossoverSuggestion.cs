using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using static GlobalVariables;
using static Highlight;

/// <summary>
/// Handles crossover suggestion interactions
/// </summary>
public class DrawCrossoverSuggestion : MonoBehaviour
{
    private const double SUGGESTION_LENGTH = 0.053;

    // Input controller variables
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    private bool triggerReleased = true;
    bool triggerValue;

    // helper variables
    private RaycastHit _hit;
    private GameObject _currentXoverSuggestion = null;
    private bool _lookingAtXover = false;

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
        if (!s_drawTogOn)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool gotTriggerValue = _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue);
        bool hitFound = rightRayInteractor.TryGetCurrent3DRaycastHit(out _hit);

        // Set helper variables
        XoverSuggestionComponent xoverSuggestionComponent = null;
        bool hitIsXoverSuggestion = false;
        GameObject hitGO = null;
        bool isPrevXoverSuggestion = false;
        if (hitFound)
        {
            xoverSuggestionComponent = _hit.transform.GetComponent<XoverSuggestionComponent>();
            hitIsXoverSuggestion = xoverSuggestionComponent != null;
            hitGO = _hit.collider.gameObject;
            isPrevXoverSuggestion = ReferenceEquals(hitGO, _currentXoverSuggestion);
        }

        //Handles hovering over crossover suggestion
        if (hitFound)
        {
            if (hitIsXoverSuggestion)
            {
                if (!isPrevXoverSuggestion)
                {
                    _lookingAtXover = true;
                    _currentXoverSuggestion = hitGO;
                    HighlightXoverSuggestion(xoverSuggestionComponent);
                }
            }
            else
            {
                _lookingAtXover = false;
                if (_currentXoverSuggestion != null)
                {
                    UnhighlightXoverSuggestion(_currentXoverSuggestion.GetComponent<XoverSuggestionComponent>());
                }
            }
        }
        // Not currenlty looking at anything, so unhighlight if previously looking at crossover suggestion.
        else if (_lookingAtXover)
        {
            _lookingAtXover = false;
            UnhighlightXoverSuggestion(_currentXoverSuggestion.GetComponent<XoverSuggestionComponent>());
            _currentXoverSuggestion = null;
        }

        // Handles clicking on crossover suggestion.
        if (gotTriggerValue && triggerValue && triggerReleased)
        {
            triggerReleased = false;
            if (hitFound && hitIsXoverSuggestion)
            {
                _lookingAtXover = false;
                xoverSuggestionComponent.CreateXover();
            }
        }
        
        // Trigger is released                                       
        else if (gotTriggerValue && !triggerReleased && !triggerValue)
        {
            triggerReleased = true;
        }
    }

    /// <summary>
    /// Returns whether there can be a crossover between the two inputted nucleotides.
    /// All conditions for a crossover must meet.
    /// </summary>
    /// <param name="nucleotide0">First nucleotide to create suggestion between.</param>
    /// <param name="nucleotide1">Second nucleotide to create suggestion between.</param>
    /// <returns>True is there can be a crossover suggestion. False otherwise.</returns>
    public static bool IsValid(NucleotideComponent nucleotide0, NucleotideComponent nucleotide1)
    {
        return DrawCrossover.IsValid(nucleotide0.gameObject, nucleotide1.gameObject);
    }

    /// <summary>
    /// Checks for valid crossover suggestions and creates them is they're valid. Valid crossover suggestions
    /// come in pairs and must average below a predefined distance. This method looks through every possible
    /// nucleotide pair in every strand.
    /// </summary>
    public static void CheckForCrossoverSuggestions()
    {
        foreach (KeyValuePair<int, Strand> pair0 in s_strandDict)
        {
            List<NucleotideComponent> nucleotides0 = pair0.Value.GetNucleotidesOnly();
            for (int i = 0; i < nucleotides0.Count - 1; i++)
            {
                foreach (KeyValuePair<int, Strand> pair1 in s_strandDict)
                {
                    List<NucleotideComponent> nucleotides1 = pair1.Value.GetNucleotidesOnly();
                    for (int j = 0; j < nucleotides1.Count - 1; j++)
                    {
                        NucleotideComponent nucleotide0i = nucleotides0[i];
                        NucleotideComponent nucleotide1i = nucleotides0[i + 1];
                        NucleotideComponent nucleotide0j = nucleotides1[j];
                        NucleotideComponent nucleotide1j = nucleotides1[j + 1];

                        // (0, 0), (1, 1) pair
                        float matchPairDistance = ((nucleotide0i.transform.position - nucleotide0j.transform.position).magnitude + (nucleotide1i.transform.position - nucleotide1j.transform.position).magnitude) / 2.0f;
                        // (0, 1), (1, 0) pair
                        float disJointPairDistance = ((nucleotide0i.transform.position - nucleotide1j.transform.position).magnitude + (nucleotide1i.transform.position - nucleotide0j.transform.position).magnitude) / 2.0f;

                        //Debug.Log("Match pair distance: " + matchPairDistance);
                        //Debug.Log("Disjoint pair distance: " + disJointPairDistance);

                        if (disJointPairDistance > SUGGESTION_LENGTH)
                        {
                            continue;
                        }
                        else if (IsValid(nucleotide0i, nucleotide1j) && IsValid(nucleotide1i, nucleotide0j))
                        {
                            DrawPoint.MakeXoverSuggestion(nucleotide0i.gameObject, nucleotide1j.gameObject);
                            DrawPoint.MakeXoverSuggestion(nucleotide1i.gameObject, nucleotide0j.gameObject);
                        }
                        /* if (matchPairDistance <= disJointPairDistance
                            && IsValid(nucleotide0i, nucleotide0j)
                            && IsValid(nucleotide1i, nucleotide1j))
                        {
                            DrawPoint.MakeXoverSuggestion(nucleotide0i.gameObject, nucleotide0j.gameObject);
                            DrawPoint.MakeXoverSuggestion(nucleotide1i.gameObject, nucleotide1j.gameObject);
                        }
                        else if (matchPairDistance > disJointPairDistance
                            && IsValid(nucleotide0i, nucleotide1j)
                            && IsValid(nucleotide1i, nucleotide0j))
                        {
                            DrawPoint.MakeXoverSuggestion(nucleotide0i.gameObject, nucleotide1j.gameObject);
                            DrawPoint.MakeXoverSuggestion(nucleotide1i.gameObject, nucleotide0j.gameObject);
                        } */
                    }
                }
            }
        }
    }

    public static void ClearCrossoverSuggestions()
    {
        foreach (XoverSuggestionComponent xoverSuggestion in s_xoverSuggestions)
        {
            Destroy(xoverSuggestion.gameObject);
        }
        s_xoverSuggestions.Clear();
    }
}
