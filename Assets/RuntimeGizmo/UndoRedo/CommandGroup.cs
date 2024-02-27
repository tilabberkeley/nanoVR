using System;
using System.Collections.Generic;

namespace CommandUndoRedo
{
	public class CommandGroup : IGizmoCommand
	{
		List<IGizmoCommand> commands = new List<IGizmoCommand>();

		public CommandGroup() {}
		public CommandGroup(List<IGizmoCommand> commands)
		{
			this.commands.AddRange(commands);
		}

		public void Set(List<IGizmoCommand> commands)
		{
			this.commands = commands;
		}

		public void Add(IGizmoCommand command)
		{
			commands.Add(command);
		}

		public void Remove(IGizmoCommand command)
		{
			commands.Remove(command);
		}

		public void Clear()
		{
			commands.Clear();
		}

		public void Execute()
		{
			for(int i = 0; i < commands.Count; i++)
			{
				commands[i].Execute();
			}
		}

		public void UnExecute()
		{
			for(int i = commands.Count - 1; i >= 0; i--)
			{
				commands[i].UnExecute();
			}
		}
	}
}
