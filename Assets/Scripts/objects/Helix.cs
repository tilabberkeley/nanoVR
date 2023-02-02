using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Helix
{
    private int _id;
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private int _length;
    private string _orientation;
    private List<Vector3> _ntAPos;
    private List<GameObject> _nucleotidesA;
    private List<GameObject> _backbonesA;
 	private List<Vector3> _ntBPos;
    private List<GameObject> _nucleotidesB;
    private List<GameObject> _backbonesB;
    
    public Helix(int id, Vector3 startPoint, Vector3 endPoint, string orientation) 
    {
        _id = id;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _length = (int)Math.Round(Vector3.Distance(startPoint, endPoint) * 30, 0);
        _orientation = orientation;
        _ntAPos = new List<Vector3>();
        _nucleotidesA = new List<GameObject>();
        _backbonesA = new List<GameObject>();
 		_ntBPos = new List<Vector3>();
        _nucleotidesB = new List<GameObject>();
        _backbonesB = new List<GameObject>();
        HelixFormation();
        DrawBackbone(_ntAPos, _backbonesA);
        DrawBackbone(_ntBPos, _backbonesB);
        SetNeighbors(_nucleotidesA, _nucleotidesB, _backbonesA);
        SetNeighbors(_nucleotidesB, _nucleotidesA, _backbonesB);
    }

    public void HelixFormation() 
    {
 		float OFFSET = 0.15f; // helical radius
		float RISE = 0.034f; // vertical rise per bp
        Vector3 targetPositionA = new Vector3(_startPoint.x, _startPoint.y, _startPoint.z + OFFSET);
        Vector3 targetPositionB = new Vector3(_startPoint.x, _startPoint.y, _startPoint.z);
        
        for (int i = 0; i < _length; i++) 
        {
            string nameA = "nucleotideA" + i;
            GameObject sphereA = DrawPoint.MakeNucleotide(targetPositionA, nameA);
            sphereA.GetComponent<Renderer>().enabled = false;
            _ntAPos.Add(targetPositionA);
            _nucleotidesA.Add(sphereA);

            string nameB = "nucleotideB" + i;
            GameObject sphereB = DrawPoint.MakeNucleotide(targetPositionB, nameB);
            sphereB.GetComponent<Renderer>().enabled = false;
            _ntBPos.Add(targetPositionB);
            _nucleotidesB.Add(sphereB);

            float angle = i * (2 * (float)(Math.PI)/10); // rotation per bp in radians
            float axisOneChange = Mathf.Cos(angle) * 0.02f;
            float axisTwoChange = Mathf.Sin(angle) * 0.02f;

            if (_orientation.Equals("XY")) 
            {
                targetPositionA = new Vector3(targetPositionA.x + axisOneChange, targetPositionA.y + axisTwoChange, targetPositionA.z + RISE);
				targetPositionB = new Vector3(targetPositionB.x + axisOneChange, targetPositionB.y + axisTwoChange, targetPositionB.z + RISE);
			}
        }
    }

    public void DrawBackbone(List<Vector3> ntPos, List<GameObject> backbones) 
    {
        for (int i = 1; i < ntPos.Count; i++) 
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.layer = LayerMask.NameToLayer ("Ignore Raycast");
            var cylinderRenderer = cylinder.GetComponent<Renderer>();
            Color transGray = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        	cylinderRenderer.material.SetColor("_Color", transGray);
            Vector3 cylinderDefaultOrientation = new Vector3(0,1,0);
        
            // Position
            cylinder.transform.position = (ntPos[i] + ntPos[i - 1])/2.0F;

            // Rotation
            Vector3 dirV = Vector3.Normalize(ntPos[i] - ntPos[i - 1]);
            Vector3 rotAxisV = dirV + cylinderDefaultOrientation;
            rotAxisV = Vector3.Normalize(rotAxisV);
            cylinder.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);

            // Scale        
            float dist = Vector3.Distance(ntPos[i], ntPos[i - 1]);
            cylinder.transform.localScale = new Vector3(0.005f, dist/2, 0.005f);
            cylinder.GetComponent<Renderer>().enabled = false;
            backbones.Add(cylinder);
        }
    }

    public void SetNeighbors(List<GameObject> nucleotides, List<GameObject> complements, List<GameObject> backbones) 
    {
        for (int i = 0; i < nucleotides.Count; i++)
        {
            var ntc = nucleotides[i].GetComponent<NucleotideComponent>();
            if (i < nucleotides.Count - 1)
            {
                ntc.SetNextGO(nucleotides[i + 1]);
                ntc.SetNextBB(backbones[i]);
            }
            if (i > 0) {
                ntc.SetPrevGO(nucleotides[i - 1]);
                ntc.SetPrevBB(backbones[i - 1]);
            }
            ntc.SetComplementGO(complements[i]);
        }        
    }

    public void ShowHelix() 
    {
        for (int i = 0; i < _backbonesA.Count; i++) 
        {
            _nucleotidesA[i].GetComponent<Renderer>().enabled = true;
            _backbonesA[i].GetComponent<Renderer>().enabled = true;
   			_nucleotidesB[i].GetComponent<Renderer>().enabled = true;
            _backbonesB[i].GetComponent<Renderer>().enabled = true;
        }
        _nucleotidesA[_nucleotidesA.Count - 1].GetComponent<Renderer>().enabled = true;
        _nucleotidesB[_nucleotidesB.Count - 1].GetComponent<Renderer>().enabled = true;
    }

    public void HideHelix() 
    {
        for (int i = 0; i < _backbonesA.Count; i++) 
        {
            _nucleotidesA[i].GetComponent<Renderer>().enabled = false;
            _backbonesA[i].GetComponent<Renderer>().enabled = false;
			_nucleotidesB[i].GetComponent<Renderer>().enabled = false;
            _backbonesB[i].GetComponent<Renderer>().enabled = false;
        }
        _nucleotidesA[_nucleotidesA.Count - 1].GetComponent<Renderer>().enabled = false;
        _nucleotidesB[_nucleotidesB.Count - 1].GetComponent<Renderer>().enabled = false;
    }
}
