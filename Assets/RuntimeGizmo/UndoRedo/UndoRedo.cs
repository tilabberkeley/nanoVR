using System;
using System.Collections.Generic;

namespace CommandUndoRedo
{
	public class UndoRedo
	{
		public int maxUndoStored {get {return undoCommands.maxLength;} set {SetMaxLength(value);}}

		DropoutStack<IGizmoCommand> undoCommands = new DropoutStack<IGizmoCommand>();
		DropoutStack<IGizmoCommand> redoCommands = new DropoutStack<IGizmoCommand>();

		public UndoRedo() {}
		public UndoRedo(int maxUndoStored)
		{
			this.maxUndoStored = maxUndoStored;
		}

		public void Clear()
		{
			undoCommands.Clear();
			redoCommands.Clear();
		}

		public void Undo()
		{
			if(undoCommands.Count > 0)
			{
				IGizmoCommand command = undoCommands.Pop();
				command.UnExecute();
				redoCommands.Push(command);
			}
		}

		public void Redo()
		{
			if(redoCommands.Count > 0)
			{
				IGizmoCommand command = redoCommands.Pop();
				command.Execute();
				undoCommands.Push(command);
			}
		}

		public void Insert(IGizmoCommand command)
		{
			if(maxUndoStored <= 0) return;

			undoCommands.Push(command);
			redoCommands.Clear();
		}

		public void Execute(IGizmoCommand command)
		{
			command.Execute();
			Insert(command);
		}

		void SetMaxLength(int max)
		{
			undoCommands.maxLength = max;
			redoCommands.maxLength = max;
		}
	}
}
