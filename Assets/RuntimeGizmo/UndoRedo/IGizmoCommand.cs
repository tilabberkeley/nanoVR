using System;

namespace CommandUndoRedo
{
	public interface IGizmoCommand
	{
		void Execute();
		void UnExecute();
	}
}
