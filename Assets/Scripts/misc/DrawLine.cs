/*using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawLine : MonoBehaviour
{
    [SerializeField] private XRNode _xrNode;
    private List<InputDevice> _devices = new List<InputDevice>();
    private InputDevice _device;
    [SerializeField] public XRRayInteractor rightRayInteractor;
    public static Vector3 s_pos = new Vector3();
    public static Vector3 s_norm = new Vector3();
    public static int s_index = 0;
    public static bool s_validTarget = false;
    public static RaycastHit s_hit;
    bool triggerReleased = true;
    bool primaryReleased = true;
    bool gripReleased = false;
    Vector3 startPoint;
    Vector3 endPoint;
    bool firstPointDrawn = false;
    bool secondPointDrawn = false;
    List<Line> lines = new List<Line>();
    public static GameObject s_cyl;

    void Start() 
    {
        s_cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder); 
        s_cyl.transform.localScale = new Vector3(0, 0, 0);
    }

    void GetDevice() 
    {
        InputDevices.GetDevicesAtXRNode(_xrNode, _devices);
        _device = _devices[0];
    }

    void OnEnable() 
    {
        if (!_device.isValid) 
        {
            GetDevice();
        }
    }

    /// Update is called once per frame
    void Update() 
    {
        if (!GlobalVariables.s_lineTogOn)
        {
            return;
        }

        if (!_device.isValid)
        {
            GetDevice();
        }

        bool triggerValue;
        if (!firstPointDrawn 
            && triggerReleased 
            && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue 
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit)) 
        {
            triggerReleased = false;
            firstPointDrawn = true;
            startPoint = s_hit.collider.bounds.center; 
        }
        if (!secondPointDrawn 
            && triggerReleased 
            && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue 
            && rightRayInteractor.TryGetCurrent3DRaycastHit(out s_hit))
        {
            triggerReleased = false;
            secondPointDrawn = true;
            endPoint = s_hit.collider.bounds.center; 
        }
        if (!firstPointDrawn
            && triggerReleased
            && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue) 
        {
            triggerReleased = false;
            firstPointDrawn = true;
            Vector3 direction = transform.rotation * Vector3.forward;
            startPoint = transform.position + direction * 0.07f ;
            DrawPoint.MakeSphere(startPoint, "startPoint");
        }

        if (!secondPointDrawn 
            && triggerReleased 
            && _device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue)
            && triggerValue) 
        {
            triggerReleased = false;
            secondPointDrawn = true;
            Vector3 direction = transform.rotation * Vector3.forward;
            endPoint = transform.position + direction * 0.07f; 
        }

        if (!(_device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)) 
        {
            triggerReleased = true;
        }

        if (firstPointDrawn) 
        {
            Vector3 direction = transform.rotation * Vector3.forward;
            Vector3 currPoint = transform.position + direction * 0.07f ;
            DrawRealCylinder.Draw(startPoint, currPoint);
        } 
        if (secondPointDrawn) 
        {
            DrawRealCylinder.Clear();
            CreateLineObject();
        }


        // if primary pressed, stop rendering and create line object
        bool primaryValue;
        if (primaryReleased && _device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryValue) && primaryValue) 
        {
            primaryReleased = false;
        }

        bool gripValue;
        if (gripReleased && _device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue) && gripValue) 
        {
            gripReleased = false;
            GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere2.transform.position = new Vector3(0, 1.5f, 0);
            
            //Edit(transform.position);
        }
    }

    void CreateLineObject() 
    {
        firstPointDrawn = false;
        secondPointDrawn = false;
        Line line = new Line(lines.Count + 1, startPoint, endPoint);
        lines.Add(line);
    }
  

    // void Edit(Vector3 handPosition) 
    // {
    //     // GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //     // sphere2.transform.position = new Vector3(5, 10, 5);
    //     // var sphereRenderer2 = sphere2.GetComponent<Renderer>();
    //     // sphereRenderer2.material.SetColor("_Color", Color.green);
    //     for (int i = 0; i < GlobalVariables.origamis.Count; i++) 
    //     {
    //         // sphereRenderer2.material.SetColor("_Color", Color.blue);
    //         if (typeof(Line).IsInstanceOfType(GlobalVariables.origamis[i])) 
    //         {
    //             Line line = (Line) GlobalVariables.origamis[i];
    //             //GlobalVariables.pointerList = new List<Pointer>(line.getPoints());
    //             //GlobalVariables.origamis.Remove(line);
              
    //             if (line.getPoints().Count > 0) {
    //                 // sphereRenderer2.material.SetColor("_Color", Color.red);
    //             }
    //             for (int j = 0; j < line.getPoints().Count; j++) 
    //             {
                    
    //                 if (Math.Abs(line.getPoints()[j].getX() - handPosition.x) < 1 && Math.Abs(line.getPoints()[j].getY() - handPosition.y) < 1 && Math.Abs(line.getPoints()[j].getZ() - handPosition.z) < 1) 
    //                 {
    //                     line.getPoints().Remove(GlobalVariables.pointerList[j]);
    //                     line.getPoints().Insert(j, new Pointer(handPosition.x, handPosition.y, handPosition.z));
    //                     if (j != 0)
    //                     {
    //                         DrawRealCylinder(line.getPoints()[j - 1].getPosition(), line.getPoints()[j].getPosition());
    //                     }
    //                     if (j != line.getPoints().Count - 1) 
    //                     {
    //                         DrawRealCylinder(line.getPoints()[j].getPosition(), line.getPoints()[j + 1].getPosition());
    //                     }
    //                     //CreateLineObject();
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    // }
}
*/