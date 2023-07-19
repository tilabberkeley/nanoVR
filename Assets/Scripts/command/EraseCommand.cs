/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using static GlobalVariables;

public class EraseCommand : MonoBehaviour, ICommand
{
    private GameObject _startGO;
    private GameObject _endGO;
    private int _startId;
    private int _endId;
    private int _helixId;
    private int _direction;

    public EraseCommand(GameObject startGO, GameObject endGO)
    {
        _startGO = startGO;
        _endGO = endGO;
        _startId = startGO.GetComponent<NucleotideComponent>().Id;
        _endId = endGO.GetComponent<NucleotideComponent>().Id;
        _helixId = startGO.GetComponent<NucleotideComponent>().HelixId;
        _direction = startGO.GetComponent<NucleotideComponent>().Direction;
    }

    public void Do()
    {
        DrawNucleotideDynamic.EraseStrand(_startGO, _endGO);
    }

    public void Undo()
    {
        GameObject startGO = FindNucleotide(_startId, _helixId, _direction);
        GameObject endGO = FindNucleotide(_endId, _helixId, _direction);
        DrawNucleotideDynamic.EditStrand(startGO, endGO);
    }

    public void Redo()
    {
        GameObject startGO = FindNucleotide(_startId, _helixId, _direction);
        GameObject endGO = FindNucleotide(_endId, _helixId, _direction);
        DrawNucleotideDynamic.EraseStrand(startGO, endGO);
    }

    public GameObject FindNucleotide(int id, int helixId, int direction)
    {
        s_helixDict.TryGetValue(helixId, out Helix helix);
        return helix.GetNucleotide(id, direction);
    }
}