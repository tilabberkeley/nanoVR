using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loop : MonoBehaviour
{
    private List<Pointer> _pointers;
    
    public Loop(List<Pointer> pointers) 
    {
        _pointers = pointers;
    }

    public int GetNumPoints() 
    {
        return _pointers.Count;
    }

    public Pointer GetPoint(int index) 
    {
        return _pointers[index];
    }

    public void SetPoint(int index, Pointer p) 
    {
        _pointers[index] = p;
    }

    // Why is this here?
    public void InsertPoint(int index, Pointer p) 
    {
        _pointers.Insert(index, p);
    }

    public void RemovePoint(Pointer p) 
    {
        _pointers.Remove(p);
    }

}
