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
using static Highlight;

public class CopyPaste : MonoBehaviour
{
    [SerializeField] private XRNode _XRNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] private XRRayInteractor rayInteractor;
    private bool gripReleased = true;
    private bool primaryReleased = true;
    private bool secondaryReleased = true;
    private bool pasting = false;
    private static Strand s_copied = null;
    private static RaycastHit s_hit;
    private static GameObject s_currNucl = null;
    private static List<GameObject> s_currNucleotides;

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
        if (!s_gridTogOn)
        {
            return;
        }
        /*
        if (!s_drawTogOn && !s_eraseTogOn)
        {
            return;
        }*/

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool primaryValue;
        if (_device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue) && primaryValue && primaryReleased)
        {
            primaryReleased = false;
            s_copied = SelectStrand.s_strand;
        }

        bool secondaryValue;
        if (_device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryValue)
            && secondaryValue
            && secondaryReleased)
        {
            secondaryReleased = false;
            if (s_copied != null)
            {
                pasting = true;
            }
        }

        if (pasting && rayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            GameObject go = s_hit.collider.gameObject;
            List<GameObject> nucleotides = MoveStrand.GetNucleotides(s_copied.GetHead(), go);
            if (IsValid(nucleotides))
            {
                s_currNucl = go;
                UnhighlightNucleotideSelection(s_currNucleotides);
                s_currNucleotides = nucleotides;
                HighlightNucleotideSelection(s_currNucleotides);
            }
        }

        bool gripValue;
        if (_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
                && gripValue && !gripReleased)
        {
            gripReleased = false;
            UnhighlightNucleotideSelection(s_currNucleotides);
            DrawNucleotideDynamic.DoCreateStrand(s_currNucleotides[0], s_currNucleotides.Last(), s_numStrands);
            Reset();
        }

        // Resets grip button.                                            
        if (!(_device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue)
            && gripValue))
        {
            gripReleased = true;
        }

        // Resets primary button.                                            
        if (!(_device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue)
            && primaryReleased))
        {
            primaryReleased = true;
        }

        // Resets secondary button.                                            
        if (!(_device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryValue)
            && secondaryValue))
        {
            secondaryReleased = true;
        }

    }

    public void Reset()
    {
        s_currNucl = null;
        s_currNucleotides = null;
        pasting = false;
    }

    public static bool IsValid(List<GameObject> nucleotides)
    {
        if (nucleotides == null) { return false; }

        foreach (GameObject nucleotide in nucleotides)
        {
            var ntc = nucleotide.GetComponent<NucleotideComponent>();
            if (ntc.Selected)
            {
                return false;
            }
        }
        return true;
    }
}
