/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */

using System.Collections.Generic;

public static class CommandManager
{
    private static Stack<ICommand> s_undoStack = new Stack<ICommand>();
    private static Stack<ICommand> s_redoStack = new Stack<ICommand>();

    public static void AddCommand(ICommand command)
    {
        s_undoStack.Push(command);
    }

    public static void Undo()
    {
        while (s_undoStack.Count > 0)
        {
            ICommand command = s_undoStack.Pop();
            if (command != null) { command.Undo(); }
            s_redoStack.Push(command);
        }
    }

    public static void Redo()
    {
        while (s_redoStack.Count > 0)
        {
            ICommand command = s_redoStack.Pop();
            if (command != null) { command.Redo(); }
            s_undoStack.Push(command);
        }
    }
}