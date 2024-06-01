/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DomainComponent : MonoBehaviour
{
    private CapsuleCollider cc;
    private Helix _helix;
    public Helix Helix { get => _helix; set => _helix = value; }

    private List<DNAComponent> _nucleotides = new List<DNAComponent>();
    public List<DNAComponent> Nucleotides { get => _nucleotides; set => _nucleotides = value; }

    public void ShowNucleotides()
    {
        foreach(DNAComponent nucleotide in _nucleotides)
        {
            nucleotide.gameObject.SetActive(true);
            nucleotide.Complement.gameObject.SetActive(true);
        }
    }

    public void HideNucleotides()
    {
        foreach (DNAComponent nucleotide in _nucleotides)
        {
            nucleotide.gameObject.SetActive(false);
            nucleotide.Complement.gameObject.SetActive(false);
        }
    }

    public void UpdateCapsuleCollider()
    {
        cc.center = Vector3.Lerp(_nucleotides[0].transform.position, _nucleotides.Last().transform.position, 0.5f);
        cc.height = Vector3.Distance(_nucleotides[0].transform.position, _nucleotides.Last().transform.position) / 2;
    }

    private void Awake()
    {
        cc = GetComponent<CapsuleCollider>();
    }

}
