/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static GlobalVariables;
using static OVRPlugin;

public class DomainComponent : MonoBehaviour
{
    private CapsuleCollider _capsuleCollider;
    private Helix _helix;
    public Helix Helix { get => _helix; set => _helix = value; }

    private List<DNAComponent> _nucleotides = new List<DNAComponent>();
    public List<DNAComponent> Nucleotides { get => _nucleotides; set => _nucleotides = value; }

    private List<GameObject> _beziers;

    public void DrawBezier()
    {
        if (_beziers.Count == 0)
        {
            List<GameObject> nuclSubList = new List<GameObject>();
            for (int i = 0; i < _nucleotides.Count; i++)
            {
                DNAComponent dnaComponent = _nucleotides[i];
                nuclSubList.Add(dnaComponent.gameObject);
                if (nuclSubList.Count % Strand.BEZIER_COUNT == 0 || i == _nucleotides.Count - 1)
                {
                    Color color = dnaComponent.Color;
                    GameObject bezier = DrawPoint.MakeBezier(nuclSubList, color);
                    _beziers.Add(bezier);
                    nuclSubList.RemoveRange(0, nuclSubList.Count - 1); // Remove all but last nucl to keep Beziers continuous.
                }
            }
        }
    }

    public void DeleteBezier()
    {
        if (_beziers.Count > 0)
        {
            foreach (GameObject bezier in _beziers) { GameObject.Destroy(bezier); }
            _beziers.Clear();
        }
    }

    public void ShowHideCone(bool enabled)
    {
        // TODO
    }

    public void ShowNucleotides()
    {
        foreach(DNAComponent nucleotide in _nucleotides)
        {
            nucleotide.gameObject.SetActive(true);
            nucleotide.Complement.gameObject.SetActive(true);
        }

        DeleteBezier();
        // Should deactivate itself to allow interaction with nucleotides
        gameObject.SetActive(false);
    }

    public void HideNucleotides()
    {
        foreach (DNAComponent nucleotide in _nucleotides)
        {
            nucleotide.gameObject.SetActive(false);
            nucleotide.Complement.gameObject.SetActive(false);
        }

        DrawBezier();
        // Should activate itself if nucleotides aren't visable
        gameObject.SetActive(true);
    }

    public void UpdateCapsuleCollider()
    {
        DNAComponent firstNucleotide = _nucleotides[0].GetComponent<DNAComponent>();
        Helix helix;
        s_helixDict.TryGetValue(firstNucleotide.HelixId, out helix);
        Helix = helix;

        Vector3 domainCenter = Vector3.Lerp(_nucleotides[0].transform.position, _nucleotides.Last().transform.position, 0.5f);
        // Center of the gridcircle will be the center the domain component (dependent on axis), default is the z position being constant,
        // TODO: configure domain component to be centered correctly around any orientation.
        Vector3 gridCircleCetner = Helix.GridComponent.transform.position;
        domainCenter.x = gridCircleCetner.x;
        domainCenter.y = gridCircleCetner.y;
        transform.position = domainCenter;

        // TODO: Update rotation as of the collider to by dependent on the rotation of the helix/grid.
        _capsuleCollider.height = Vector3.Distance(_nucleotides[0].transform.position, _nucleotides.Last().transform.position);

        // Make capsule collider the same size as the grid circle. Dividing by two made it perfect? idk why didn't rly dive into this.
        _capsuleCollider.radius = Helix.GridComponent.transform.localScale.x / 2;
    }

    private void Awake()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
        // Disabled when created
        gameObject.SetActive(false);
    }
}
