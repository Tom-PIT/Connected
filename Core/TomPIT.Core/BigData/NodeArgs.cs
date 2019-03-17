using System;

namespace TomPIT.BigData
{
	public class NodeArgs : EventArgs
	{
		public NodeArgs(Guid node)
		{
			Node = node;
		}

		public Guid Node { get; }
	}
}
