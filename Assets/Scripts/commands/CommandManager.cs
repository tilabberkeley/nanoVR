/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
public static class CommandManager
{
    private static Stack<ICommand> s_undoStack = new Stack<ICommand>();
    private static Stack<ICommand> s_redoStack = new Stack<ICommand>();

    /// <summary>
    /// Adds a command to the undo stack and does the command.
    /// Also clears the redo stack.
    /// </summary>
    /// <param name="command">Command to be added to the undo stakc.</param>
    public static void AddCommand(ICommand command)
    {
        command.Do();
        s_undoStack.Push(command);
        s_redoStack.Clear();
        //CommandUpdate();
    }

    /// <summary>
    /// Does the first command on the undo stack.
    /// </summary>
    public static void Undo()
    {
        if (s_undoStack.Count > 0)
        {
            ICommand command = s_undoStack.Pop();
            command?.Undo();
            s_redoStack.Push(command);
            //CommandUpdate();
        }
    }

    /// <summary>
    /// Does the first command on the redo stack.
    /// </summary>
    public static void Redo()
    {
        if (s_redoStack.Count > 0)
        {
            ICommand command = s_redoStack.Pop();
            command?.Redo();
            s_undoStack.Push(command);
            //CommandUpdate();
        }
    }

    /// <summary>
    /// Whenever a command is done, undone, or redone, CommandUpdate will be called.
    /// </summary>
    private static void CommandUpdate(List<Strand> updatedStrands)
    {
        if (updatedStrands.Count == 0) { return; }
        DrawCrossoverSuggestion.ClearCrossoverSuggestions();
        DrawCrossoverSuggestion.CheckForCrossoverSuggestions();
    }
}