using System;
using UnityEngine;
using static GlobalVariables;

public class Line
{
    private int _id;
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private int _length;
    //private GameObject _startSphere;
    //private GameObject _endSphere;
    private GameObject _cyl;
    // private GameObject _text;

    public Line(int id, Vector3 startPoint, Vector3 endPoint)
    {
        _id = id;
        _startPoint = startPoint;
        _endPoint = endPoint;
        _length = 64;
        DrawLine();
        ChangeRendering();
    }

    public int Id { get; }

    public Vector3 Start { get; }

    public void SetStart(Vector3 p)
    {
        _startPoint = p;
        _length = (int)Math.Round(Vector3.Distance(_startPoint, _endPoint) * 30, 0);
        //EditLine();
    }

    public Vector3 End { get; }
    public void SetEnd(Vector3 p)
    {
        _endPoint = p;
        _length = (int)Math.Round(Vector3.Distance(_startPoint, _endPoint) * 30, 0);
        //EditLine();
    }

    public void DrawLine()
    {
        _cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _cyl.layer = LayerMask.NameToLayer("Ignore Raycast");
        var cylRenderer = _cyl.GetComponent<Renderer>();
        cylRenderer.material.SetColor("_Color", Color.gray);
        Vector3 cylDefaultOrientation = new Vector3(0, 1, 0);

        // Position
        _cyl.transform.position = (_endPoint + _startPoint) / 2.0F;

        // Rotation
        Vector3 dirV = Vector3.Normalize(_endPoint - _startPoint);
        Vector3 rotAxisV = dirV + cylDefaultOrientation;
        rotAxisV = Vector3.Normalize(rotAxisV);
        _cyl.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);

        // Scale        
        float dist = Vector3.Distance(_endPoint, _startPoint);
        _cyl.transform.localScale = new Vector3(0.068f, dist / 2, 0.068f);


        /*
        _startSphere = DrawPoint.MakeSphere(_startPoint, "startPoint" + _id);
        _endSphere = DrawPoint.MakeSphere(_endPoint, "endPoint" + _id);

    
        _text = new GameObject();
        TextMesh uiText = _text.AddComponent<TextMesh>();
        _text.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
        _text.transform.position = new Vector3(_startPoint.x, _startPoint.y, _startPoint.z - 0.05f);
        uiText.fontSize = 100;
        uiText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        uiText.text = _length.ToString();
        uiText.color = Color.black;
        */
    }

    /*
    public void EditLine()
    {
    var cylRenderer = _cyl.GetComponent<Renderer>();
    cylRenderer.material.SetColor("_Color", Color.green);
    Vector3 cylDefaultOrientation = new Vector3(0, 1, 0);

    // Position
    _cyl.transform.position = (_endPoint + _startPoint) / 2.0F;

    // Rotation
    Vector3 dirV = Vector3.Normalize(_endPoint - _startPoint);
    Vector3 rotAxisV = dirV + cylDefaultOrientation;
    rotAxisV = Vector3.Normalize(rotAxisV);
    _cyl.transform.rotation = new Quaternion(rotAxisV.x, rotAxisV.y, rotAxisV.z, 0);

    // Scale        
    float dist = Vector3.Distance(_endPoint, _startPoint);
    _cyl.transform.localScale = new Vector3(0.02f, dist / 2, 0.02f);


    TextMesh uiText = _text.GetComponent<TextMesh>();
    _text.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
    _text.transform.position = new Vector3(_startPoint.x, _startPoint.y, _startPoint.z - 0.05f);
    uiText.fontSize = 100;
    uiText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
    uiText.text = _length.ToString();
    uiText.color = Color.black;
    }
    */

    /// <summary>
    /// Changes rendering of line.
    /// </summary>
    public void ChangeRendering()
    {
        _cyl.SetActive(!s_nucleotideView);
    }
}
