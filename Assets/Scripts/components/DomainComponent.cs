/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GlobalVariables;

public class DomainComponent : MonoBehaviour
{
    // Number of nucleotides included in one "Bezier" of the Strand View.
    // It is crucial that this is some multiple of 3 + 4 so 3n + 4 and odd. See SplineInterpolation.SetSplinePoints for the reason why.
    private const int BEZIER_COUNT = 19;  

    private CapsuleCollider _capsuleCollider;
    private Helix _helix;
    public Helix Helix { get => _helix; set => _helix = value; }

    private List<DNAComponent> _dnaComponents = new List<DNAComponent>();
    public List<DNAComponent> DNAComponents { get => _dnaComponents; set => _dnaComponents = value; }

    private List<Bezier> _beziers = new List<Bezier>();
    public List<Bezier> Beziers { get => _beziers; }

    private Strand _strand;
    public Strand Strand { get => _strand; set => _strand = value; }

    private Vector3 _bezierStartPoint;
    public Vector3 BezierStartPoint { get => _bezierStartPoint; }

    private Vector3 _bezierEndPoint;
    public Vector3 BezierEndPoint { get => _bezierEndPoint; }

    /// <summary>
    /// Configures the domain with given dna components.
    /// </summary>
    public void Configure(List<DNAComponent> dnaList)
    {
        foreach (DNAComponent nucleotide in dnaList)
        {
            _dnaComponents.Add(nucleotide);
            nucleotide.Domain = this;
        }

        UpdateCapsuleCollider();
        // Create bezier on domain creation. And then hide it.
        DrawBezier();
        HideBezier();
        // Should always be disabled when created
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Draws the bezier curve representation of this domain.
    /// </summary>
    private void DrawBezier()
    {
        if (_beziers.Count == 0)
        {
            List<DNAComponent> nuclSubList = new List<DNAComponent>();
            for (int i = 0; i < _dnaComponents.Count; i++)
            {
                nuclSubList.Add(_dnaComponents[i]);
                if (nuclSubList.Count % BEZIER_COUNT == 0 || i == _dnaComponents.Count - 1)
                {
                    Bezier bezier = DrawPoint.MakeDomainBezier(nuclSubList, _strand.Color, out Vector3 bezierStartPoint, out Vector3 bezierEndPoint);
                    bezier.Tube.transform.SetParent(transform, true);
                    //bezier.Endpoint0.transform.SetParent(transform, true);
                    //bezier.Endpoint1.transform.SetParent(transform, true);

                    if (_beziers.Count == 0)
                    {
                        _bezierStartPoint = bezierStartPoint;
                    }
                    if (i == _dnaComponents.Count - 1)
                    {
                        _bezierEndPoint = bezierEndPoint;
                    }

                    _beziers.Add(bezier);
                    nuclSubList.RemoveRange(0, nuclSubList.Count - 1); // Remove all but last nucl to keep Beziers continuous.
                }
            }
        }
    }

    /// <summary>
    /// Deletes the bezier curve representation of this domain.
    /// </summary>
    public void DeleteBezier()
    {
        if (_beziers.Count > 0)
        {
            foreach (Bezier bezier in _beziers)
            {
                bezier.Destroy();
            }
            _beziers.Clear();
        }
    }

    public void ShowBezier()
    {
        foreach (Bezier bezier in _beziers)
        {
            bezier.SetActive(true);
        }
    }

    public void HideBezier()
    {
        foreach (Bezier bezier in _beziers)
        {
            bezier.SetActive(false);
        }
    }

    public void ShowHideCone(bool enabled)
    {
        throw new NotImplementedException();
        // TODO
        // Maybe always keeping the cone visiable is fine? So you know the direction of the abstracted strand?
    }

    /// <summary>
    /// Shows the nucleotide representation of this domain.
    /// </summary>
    public void NucleotideView()
    {
        foreach(DNAComponent nucleotide in _dnaComponents)
        {
            nucleotide.gameObject.SetActive(true);
            nucleotide.Complement.gameObject.SetActive(true);

            // If nucleotide is connected to a xover, then it should be shown too.
            if (!nucleotide.IsBackbone)
            {
                XoverComponent xoverComponent = ((NucleotideComponent)nucleotide).Xover?.GetComponent<XoverComponent>();
                if (xoverComponent != null)
                {
                    xoverComponent.NucleotideView();
                }
            }
        }

        HideBezier();
        // Should deactivate itself to allow interaction with nucleotides
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the bezier curve representation of this domain (strand view). Hides the complement nucleotides.
    /// </summary>
    public void StrandView()
    {
        foreach (DNAComponent nucleotide in _dnaComponents)
        {
            nucleotide.gameObject.SetActive(false);
            nucleotide.Complement.gameObject.SetActive(false);

            // If nucleotide is connected to a xover, then it should be hidden too.
            //if (!nucleotide.IsBackbone)
            //{
            //    XoverComponent xoverComponent = ((NucleotideComponent)nucleotide).Xover?.GetComponent<XoverComponent>();
            //    if (xoverComponent != null)
            //    {
            //        xoverComponent.Hide(_strand.Color);
            //    }
            //}
        }

        ShowBezier();
        // Should activate itself if nucleotides aren't visable
        gameObject.SetActive(true);
    }

    public void HelixView()
    {
        HideBezier();
        // Should activate itself if nucleotides aren't visable
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the bezier curve representation of this domain (strand view). Doesn't hide the complement nucleotides.
    /// </summary>
    public void StrandViewWithoutComplement()
    {
        foreach (DNAComponent nucleotide in _dnaComponents)
        {
            nucleotide.gameObject.SetActive(false);

            // If nucleotide is connected to a xover, then it should be hidden too.
            if (!nucleotide.IsBackbone)
            {
                XoverComponent xoverComponent = ((NucleotideComponent)nucleotide).Xover?.GetComponent<XoverComponent>();
                if (xoverComponent != null)
                {
                    xoverComponent.StrandView(_strand.Color);
                }
            }
        }

        ShowBezier();
        // Should activate itself if nucleotides aren't visable
        gameObject.SetActive(true);
    }

    public void UpdateCapsuleCollider()
    {
        DNAComponent firstNucleotide = _dnaComponents[0];

        s_helixDict.TryGetValue(firstNucleotide.HelixId, out Helix helix);
        Helix = helix;

        Vector3 domainCenter = Vector3.Lerp(firstNucleotide.transform.position, _dnaComponents.Last().transform.position, 0.5f);

        // If domain is not extension,
        // center of the gridcircle will be the center the domain component (dependent on axis), default is the z position being constant,
        // TODO: configure domain component to be centered correctly around any orientation.
        if (!firstNucleotide.IsExtension)
        {
            Vector3 gridCircleCenter = Helix.GridComponent.transform.position;
            domainCenter.x = gridCircleCenter.x;
            domainCenter.y = gridCircleCenter.y;
        }
       
        transform.position = domainCenter;

        // TODO: Update rotation as of the collider to by dependent on the rotation of the helix/grid.
        _capsuleCollider.height = Vector3.Distance(_dnaComponents[0].transform.position, _dnaComponents.Last().transform.position);

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
