using System;
using System.Collections.Generic;

namespace CommandUndoRedo
{
	public class CommandGroup : IfCommand
	{
		List<IfCommand> commands = new List<IfCommand>();

		public CommandGroup() {}
		public CommandGroup(List<IfCommand> commands)
		{
			this.commands.AddRange(commands);
		}

		public void Set(List<IfCommand> commands)
		{
			this.commands = commands;
		}

		public void Add(IfCommand command)
		{
			commands.Add(command);
		}

		public void Remove(IfCommand command)
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
