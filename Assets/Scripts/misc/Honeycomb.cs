using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Honeycomb : MonoBehaviour
{
    private int id;
    private string plane;
    private Vector3 startPos;
    private List<Vector3> nodes = new List<Vector3>();
    private List<Line> lines = new List<Line>();
    private List<Helix> helices = new List<Helix>();
    private int numNodes = 100;


    public Honeycomb(string plane, Vector3 startPos) {
        this.plane = plane;
        this.startPos = startPos;
        Generate();
        DrawGrid();
    }

    public String getPlane() {
        return this.plane;
    }

    public Vector3 getStartPos() {
        return this.startPos;
    }

     public List<Vector3> getNodes() {
        return this.nodes;
    }

    
    public List<Line> getLines() {
        return this.lines;
    }

    public Line getLine(int index) {
        return this.lines[index];
    }

    private void Generate () {
		for (int i = 0, secDim = 0; secDim < 10; secDim++) {
			for (int firstDim = 0; firstDim < 10; firstDim++, i++) {
                if (this.plane.Equals("XY")) {
                    switch (secDim % 3) {
                        case 0:
                            nodes.Add(new Vector3(startPos.x + (firstDim + 0.5f)/20f, startPos.y + ((secDim*(2*(float)Math.Sqrt(3)/9f)))/20f, startPos.z));
                            break;
                        case 1:
                            nodes.Add(new Vector3(startPos.x + firstDim/20f, startPos.y + (((float)Math.Sqrt(3)/6f) + ((secDim - 1) *(2*(float)Math.Sqrt(3)/9f)))/20f, startPos.z));
                            break;
                        case 2:
                            nodes.Add(new Vector3(startPos.x + firstDim/20f, startPos.y + (((float)Math.Sqrt(3)/2f) + ((secDim - 2) * (2*(float)Math.Sqrt(3)/9f)))/20f, startPos.z));
                            break;
                    }
                } 
                // else if (this.plane.Equals("YZ")) {
                //     switch (secDim % 4) {
                //         case 0:
                //             nodes[i] = new Vector3(startPos.x, startPos.y + (firstDim + 0.5f)/32f, startPos.z + secDim/(float)Math.Sqrt(3)/32f);
                //             break;
                //         case 1:
                //             nodes[i] = new Vector3(startPos.x, startPos.y + firstDim/32f, startPos.z + secDim/(float)(2*Math.Sqrt(3))/32f);
                //             break;
                //         case 2:
                //             nodes[i] = new Vector3(startPos.x, startPos.y + firstDim/32f , startPos.z + secDim/(float)Math.Sqrt(3)/32f);
                //             break;
                //         case 3:
                //             nodes[i] = new Vector3(startPos.x, startPos.y + (firstDim + 0.5f)/32f, startPos.z + secDim/(float)(2*Math.Sqrt(3))/32f);
                //             break;
                //     }                
                // } else {
                //     switch (secDim % 4) {
                //         case 0:
                //             nodes[i] = new Vector3(startPos.x + (firstDim + 0.5f)/32f, startPos.y, startPos.z + secDim/(float)Math.Sqrt(3)/32f);
                //             break;
                //         case 1:
                //             nodes[i] = new Vector3(startPos.x + firstDim/32f, startPos.y, startPos.z + secDim/(float)(2*Math.Sqrt(3))/32f);
                //             break;
                //         case 2:
                //             nodes[i] = new Vector3(startPos.x + firstDim/32f, startPos.y, startPos.z + secDim/(float)Math.Sqrt(3)/32f);
                //             break;
                //         case 3:
                //             nodes[i] = new Vector3(startPos.x + (firstDim + 0.5f)/32f, startPos.y, startPos.z + secDim/(float)(2*Math.Sqrt(3))/32f);
                //             break;
                //     }
                // }
			}
		}
	}

    private void DrawGrid () {
		if (nodes == null) {
			return;
		}
		for (int i = 0; i < nodes.Count; i++) {
			DrawPoint.MakeSphere(nodes[i], "honeycomb");
		}
	}

    public void AddLine(int id, Vector3 startPoint, Vector3 endPoint) {
        Line line = new Line(id, startPoint, endPoint);
        lines.Add(line);
    }

    public void AddHelix(int id, Vector3 startPoint, Vector3 endPoint, string orientation) {
        //Helix helix = new Helix(id, startPoint, endPoint, orientation);
       // helices.Add(helix);
    }

    public void ShowHelices() {
        for (int i = 0; i < lines.Count; i++) {
            lines[i].HideLine();
            //helices[i].ShowHideHelix(true);
        }
    }
}
