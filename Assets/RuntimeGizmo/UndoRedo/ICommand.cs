using System;

namespace CommandUndoRedo
{
	public interface IfCommand
	{
		void Execute();
		void UnExecute();
	}
}
