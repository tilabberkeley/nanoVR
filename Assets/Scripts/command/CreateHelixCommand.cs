using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVariables;

public class CreateHelixCommand : ICommand
{
    private const float OFFSET = 0.034f;

    // Grid that helix belongs to.
    private Grid _grid;

    // Helix id.
    private int _id;
    private Vector3 _startPoint;
    private Vector3 _endPoint;

    // Number of nucleotides in helix.
    private int _length;
    private string _orientation;

    // 3D position of grid circle.
    private GridComponent _gridComp;

    // List containing all nucleotides in spiral going in direction 1.
    private List<GameObject> _nucleotidesA;

    // List containing all backbones in spiral going in direction 1.
    private List<GameObject> _backbonesA;

    // List containing all nucleotides in spiral going in direction 0.
    private List<GameObject> _nucleotidesB;

    // List containing all backbones in spiral going in direction 0.
    private List<GameObject> _backbonesB;

    // List of strand ids created on helix.
    private List<int> _strandIds;

    private List<(int, List<GameObject>, Color)> _deletedStrands;

    // Positions of last nucleotides in helix
    private Vector3 lastPositionA;
    private Vector3 lastPositionB;

    public CreateHelixCommand(Grid grid, int id, Vector3 startPoint, Vector3 endPoint, string orientation, GridComponent gridComponent)
    {
        _id = id;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _length = 0;
        _orientation = orientation;
        _gridComp = gridComponent;
        _strandIds = new List<int>();
        _deletedStrands = new List<(int, List<GameObject>, Color)>();

        /*
        _nucleotidesA = new List<GameObject>();
        _backbonesA = new List<GameObject>();
        _nucleotidesB = new List<GameObject>();
        _backbonesB = new List<GameObject>();
        lastPositionA = _gridPoint - new Vector3(0, OFFSET, 0);
        lastPositionB = _gridPoint + new Vector3(0, OFFSET, 0);
        */
    }

    public void Do()
    {
        _grid.AddHelix(_id, _startPoint, _endPoint, _orientation, _gridComp);
    }

    public void Redo()
    {
        _grid.AddHelix(_id, _startPoint, _endPoint, _orientation, _gridComp);
        for (int i = 0; i < _deletedStrands.Count; i++)
        {
            //DrawCrossover.CreateStrand(_deletedStrands[i].Item2, _deletedStrands[i].Item1, _deletedStrands[i].Item3);
        }
    }

    public void Undo()
    {
        _strandIds = s_helixDict[_id].GetStrandIds();
        foreach (int strandId in _strandIds)
        {
            Strand strand = s_strandDict[strandId];
            _deletedStrands.Add((strandId, strand.GetNucleotides(), strand.GetColor()));
            SelectStrand.DeleteStrand(strand.GetHead());
        }
        // Delete helix
    }
}
