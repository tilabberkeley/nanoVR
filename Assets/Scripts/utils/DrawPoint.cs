/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.Object;
using static GlobalVariables;
using SplineMesh;
using static Utils;

/// <summary>
/// Creates needed gameobjects like nucleotides, backbones, cones, Xovers, spheres, and grids.
/// </summary>
public static class DrawPoint
{
    private const int SPLINE_RESOLUTION = 1;
    private const float TUBE_SIZE = 0.01f;
    // private const float LOOPOUT_SIZE = 0.005f;
    // Factor determines how much loopout "bends." Higher factor, more bending.
    private const float LOOPOUT_BEND_FACTOR = 0.25f;

    /// <summary>
    /// Creates grid circle game object at specified location and rotates it depending on plane direction.
    /// </summary>
    /// <param name="startPosition">Start position of grid object.</param>
    /// <param name="startGridCircle">Grid circle associated with original start position</param>
    /// <param name="xOffset">2D x direction offset of grid circle in grid object.</param>
    /// <param name="yOffset">2D y direction offset of grid circle in grid object.</param>
    /// <param name="plane">Plane direction of grid object.</param>
    /// <returns></returns>
    public static GameObject MakeGridCircleGO(Vector3 startPosition, GameObject startGridCircle, float xOffset, float yOffset, string plane, GridPoint gridPoint)
    {
        // Calculate position.
        Vector3 position;
        if (startGridCircle != null)
        {
            position = startPosition + xOffset * startGridCircle.transform.right + yOffset * startGridCircle.transform.up;
        }
        else
        {
            if (plane.Equals("XY"))
            {
                position = new Vector3(startPosition.x + xOffset, startPosition.y + yOffset, startPosition.z);
            }
            else if (plane.Equals("YZ"))
            {
                position = new Vector3(startPosition.x, startPosition.y + xOffset, startPosition.z + yOffset);
                Debug.Log($"YZ position: {position}");
            }
            else
            {
                position = new Vector3(startPosition.x + xOffset, startPosition.y, startPosition.z + yOffset);
            }
        }        

        GameObject gridCircle = Instantiate(GridCircle,
                   position,
                   Quaternion.identity) as GameObject;

        // Calculate rotation
        if (startGridCircle != null)
        {
            gridCircle.transform.rotation = startGridCircle.transform.rotation;
        }
        else
        {
            /*if (plane.Equals("XY"))
            {
                gridCircle.transform.Rotate(0f, 180f, 0f, 0);
            }*/
            if (plane.Equals("XZ"))
            {
                gridCircle.transform.Rotate(90f, 0f, 0f, 0);
            }
            else if (plane.Equals("YZ"))
            {
                gridCircle.transform.Rotate(0f, 90f, 90f, 0);
                Debug.Log($"YZ rotation: {gridCircle.transform.rotation}");
            }
        }

        gridCircle.name = "gridPoint";
        //GridComponent gridComponent = gridCircle.GetComponent<GridComponent>();
        //gridComponent.Position = position;

        /*TextMeshPro tmp = gridCircle.GetComponentInChildren<TextMeshPro>();
        tmp.text = $"[{gridPoint.X}, {gridPoint.Y}]";*/

        //SaveGameObject(gridCircle);
        //gridCircle.isStatic = true;
        return gridCircle;
    }

    public static XoverComponent MakeXover(Domain prevDomain, Domain nextDomain, Transform gc)
    {
        GameObject xover =
                   Instantiate(Xover,
                   Vector3.zero,
                   Quaternion.identity) as GameObject;
        xover.name = "xover";

        NucleotideData prevNucl = prevDomain.GetTailData();
        NucleotideData nextNucl = nextDomain.GetHeadData();
    
        //if (prevDomain.Direction == 1) // forward direction
        //{
        //    prevNucl = prevDomain.GetTailData();
        //    nextNucl = nextDomain.GetHeadData();
        //}
        //else
        //{
        //    prevNucl = prevDomain.GetHeadData();
        //    nextNucl = nextDomain.GetTailData();
        //}
        Vector3 prevPosition = prevNucl.GetPosition();
        Vector3 nextPosition = nextNucl.GetPosition();
        //xover.transform.SetParent(gc); // helps with transformations


        // Position
        xover.transform.position = (nextPosition + prevPosition) / 2.0F;

        // Rotation
        Vector3 dirV = Vector3.Normalize(nextPosition - prevPosition);
        xover.transform.rotation = Quaternion.FromToRotation(Vector3.up, dirV);

        //xover.transform.rotation = Quaternion.FromToRotation(Vector3.up, nextPosition - prevPosition);


        // Scale        
        //Vector3 inverseScale = new Vector3(1f / gc.localScale.x, 1f / gc.localScale.y, 1f / gc.localScale.z);
        //transform.SetParent(gc);

        float dist = Vector3.Distance(nextPosition, prevPosition);
        xover.transform.localScale = new Vector3(
            XOVER_RAD,
            (dist),
            XOVER_RAD
        );        //Debug.Log(string.Format("Finished drawing xover: {0}", xover.transform.localScale));

        return xover.GetComponent<XoverComponent>();
    }

    public static GameObject MakeLoopout(Domain prevDomain, Domain nextDomain)
    {
        GameObject xover =
                   Instantiate(Xover,
                   Vector3.zero,
                   Quaternion.identity);
        xover.name = "xover";

        NucleotideData prevNucl = prevDomain.GetTailData();
        NucleotideData nextNucl = nextDomain.GetHeadData();

        Vector3 prevPosition = prevNucl.GetPosition();
        Vector3 nextPosition = nextNucl.GetPosition();

        // Position
        xover.transform.position = (nextPosition + prevPosition) / 2.0F;

        // Rotation
        Vector3 dirV = Vector3.Normalize(nextPosition - prevPosition);
        xover.transform.rotation = Quaternion.FromToRotation(Vector3.up, dirV);

        // Scale        
        float dist = Vector3.Distance(nextPosition, prevPosition);
        xover.transform.localScale = new Vector3(LOOPOUT_RAD, dist, 0.4f);
        return xover;
    }

    public static HelixComponent MakeHelixCylinder(Helix helix, Vector3 startPos, Vector3 endPos, Color32 color)
    {
        GameObject cylinder = Instantiate(HelixCylinder,
                              Vector3.zero,
                              Quaternion.identity);
        cylinder.name = "helixCylinder";

        var helixComponent = cylinder.GetComponent<HelixComponent>();
        helixComponent.Helix = helix;
        Vector3 cylDefaultOrientation = new Vector3(0, 1, 0);

        // Position
        cylinder.transform.position = (startPos + endPos) / 2.0F;

        // Rotation
        Vector3 dirV = Vector3.Normalize(endPos - startPos);
        cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, dirV);

        // Scale        
        float dist = Vector3.Distance(endPos, startPos);
        cylinder.transform.localScale = new Vector3(Utils.RADIUS * 2, dist / 2, Utils.RADIUS * 2);
        cylinder.GetComponent<Renderer>().material.SetColor("_Color", color);
        return helixComponent;
    }
}

