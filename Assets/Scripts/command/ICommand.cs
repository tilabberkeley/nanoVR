/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using UnityEngine;
/// <summary>
/// Interface for undo/redo commands.
/// </summary>
public interface ICommand 
{
    public void Do();
    public void Undo();
    public void Redo();
}

