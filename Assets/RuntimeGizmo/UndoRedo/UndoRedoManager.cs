using System;

namespace CommandUndoRedo
{
	public static class UndoRedoManager
	{
		static UndoRedo undoRedo = new UndoRedo();

		public static int maxUndoStored {get {return undoRedo.maxUndoStored;} set {undoRedo.maxUndoStored = value;}}

		public static void Clear()
		{
			undoRedo.Clear();
		}

		public static void Undo()
		{
			undoRedo.Undo();
		}

		public static void Redo()
		{
			undoRedo.Redo();
		}

		public static void Insert(IfCommand command)
		{
			undoRedo.Insert(command);
		}

		public static void Execute(IfCommand command)
		{
			undoRedo.Execute(command);
		}
	}
}
