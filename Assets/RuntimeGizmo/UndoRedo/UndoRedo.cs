using System;
using System.Collections.Generic;

namespace CommandUndoRedo
{
	public class UndoRedo
	{
		public int maxUndoStored {get {return undoCommands.maxLength;} set {SetMaxLength(value);}}

		DropoutStack<IfCommand> undoCommands = new DropoutStack<IfCommand>();
		DropoutStack<IfCommand> redoCommands = new DropoutStack<IfCommand>();

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
				IfCommand command = undoCommands.Pop();
				command.UnExecute();
				redoCommands.Push(command);
			}
		}

		public void Redo()
		{
			if(redoCommands.Count > 0)
			{
				IfCommand command = redoCommands.Pop();
				command.Execute();
				undoCommands.Push(command);
			}
		}

		public void Insert(IfCommand command)
		{
			if(maxUndoStored <= 0) return;

			undoCommands.Push(command);
			redoCommands.Clear();
		}

		public void Execute(IfCommand command)
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
