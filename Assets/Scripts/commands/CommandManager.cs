/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using System.Collections.Generic;
using UnityEngine;
public static class CommandManager
{
    private static Stack<ICommand> s_undoStack = new Stack<ICommand>();
    private static Stack<ICommand> s_redoStack = new Stack<ICommand>();

    public static void AddCommand(ICommand command)
    {
        s_undoStack.Push(command);
        s_redoStack.Clear();
    }

    public static void Undo()
    {
        if (s_undoStack.Count > 0)
        {
            ICommand command = s_undoStack.Pop();
            if (command != null) { command.Undo(); }
            s_redoStack.Push(command);
        }
    }

    public static void Redo()
    {
        if (s_redoStack.Count > 0)
        {
            ICommand command = s_redoStack.Pop();
            if (command != null) { command.Redo(); }
            s_undoStack.Push(command);
        }
    }
}