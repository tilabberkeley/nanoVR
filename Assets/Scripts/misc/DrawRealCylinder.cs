using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawRealCylinder : MonoBehaviour
{

    public static GameObject cyl;
    void Start() {
        cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cyl.layer = LayerMask.NameToLayer ("Ignore Raycast"); 
        cyl.transform.localScale = new Vector3(0, 0, 0);
    }
    
    


    /// <summary>
    /// The method <c>Draw</c> draws a 3D line (cyclinder) between two given 3D points
    /// <param>startV: starting 3D point of line</param>
    /// <param>endV: ending 3D point of line</param>
    /// <param>is_finished: </param>
    /// </summary>
    public static void Draw(Vector3 startV, Vector3 endV) 
    {
        var cylRenderer = cyl.GetComponent<Renderer>();
        cylRenderer.material.SetColor("_Color", Color.red);
        Vector3 cylDefaultOrientation = new Vector3(0,1,0);
    
        // Position
        cyl.transform.position = (endV + startV)/2.0F;

        // Rotation
        Vector3 dirV = Vector3.Normalize(endV - startV);
        Vector3 rotAxisV = dirV + cylDefaultOrientation;
        rotAxisV = Vector3.Normalize(rotAxisV);
        cyl.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);

        // Scale        
        float dist = Vector3.Distance(endV, startV);
        cyl.transform.localScale = new Vector3(0.007f, dist/2, 0.007f);
	}

    public static GameObject DrawReal(Vector3 startV, Vector3 endV) 
    {
        GameObject cyl2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder); 
        cyl2.layer = LayerMask.NameToLayer ("Ignore Raycast");
        var cylRenderer2 = cyl2.GetComponent<Renderer>();
        cylRenderer2.material.SetColor("_Color", Color.green);
        Vector3 cylDefaultOrientation2 = new Vector3(0,1,0);
    
        // Position
        cyl2.transform.position = (endV + startV)/2.0F;

        // Rotation
        Vector3 dirV2 = Vector3.Normalize(endV - startV);
        Vector3 rotAxisV2 = dirV2 + cylDefaultOrientation2;
        rotAxisV2 = Vector3.Normalize(rotAxisV2);
        cyl2.transform.rotation = new Quaternion(rotAxisV2.x, rotAxisV2.y, rotAxisV2.z, 0);

        // Scale        
        float dist2 = Vector3.Distance(endV, startV);
        cyl2.transform.localScale = new Vector3(0.02f, dist2/2, 0.02f);
        return cyl2;
	}

    public static void Clear() {
        cyl.transform.localScale = new Vector3(0, 0, 0);
    }
}
