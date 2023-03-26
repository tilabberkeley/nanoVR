/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

/// <summary>
/// Interface for undo/redo commands.
/// </summary>
public interface ICommand 
{ 
    public void Undo();
    public void Redo();
}

