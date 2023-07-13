/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

public class CreateCommand : ICommand
{
    private List<GameObject> _nucleotides;
    //private GameObject _startGO;
    //private GameObject _endGO;
    private int _helixId;
    private int _startId;
    private int _endId;
    private int _strandId;
    private int _direction;
    private GameObject _startGO;
    private GameObject _endGO;
    private List<GameObject> _xovers;
    private Color _color;

    public CreateCommand(GameObject startGO, GameObject endGO, int strandId)
    {
        _startId = startGO.GetComponent<NucleotideComponent>().Id;
        _endId = endGO.GetComponent<NucleotideComponent>().Id;
        _helixId = startGO.GetComponent<NucleotideComponent>().HelixId;
        _direction = startGO.GetComponent<NucleotideComponent>().Direction;
        _xovers = new List<GameObject>();
        _strandId = strandId;
    }

    public void Do()
    {
        GameObject startGO = FindNucleotide(_startId, _helixId, _direction);
        GameObject endGO = FindNucleotide(_endId, _helixId, _direction);
        DrawNucleotideDynamic.CreateStrand(startGO, endGO, _strandId);
        _color = s_strandDict[_strandId].GetColor();
    }

    public void Undo()
    {
        // Delete entire strand.
        GameObject startGO = FindNucleotide(_startId, _helixId, _direction);
        SelectStrand.DeleteStrand(startGO);
    }

    public void Redo()
    {
        GameObject startGO = FindNucleotide(_startId, _helixId, _direction);
        GameObject endGO = FindNucleotide(_endId, _helixId, _direction);
        DrawCrossover.CreateStrand(startGO, endGO, _xovers, _strandId, _color);
    }

    public GameObject FindNucleotide(int id, int helixId, int direction)
    {
        s_helixDict.TryGetValue(helixId, out Helix helix);
        return helix.GetNucleotide(id, direction);
    }
}
