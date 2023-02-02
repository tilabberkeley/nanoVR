using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Export : MonoBehaviour
{
    // Start is called before the first frame update
    public void ExportStructure() {
        // calculate number of nucleotides
        //int numNs = CalculateNs((Line) GlobalVariables.origamis[0]);
    }

    // public int CalculateNs(Line line) {
    //     float distance = 0;
    //     for (int i = 1; i < line.getPoints().Count; i++) {
    //         float deltaX = line.getPoints()[i].getX() - line.getPoints()[i-1].getX();
    //         float deltaY = line.getPoints()[i].getY() - line.getPoints()[i-1].getY();
    //         float deltaZ = line.getPoints()[i].getZ() - line.getPoints()[i-1].getZ();
    //         distance += (float) Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
    //     }
    //     return (int) (distance / 5);
    // }
}
