using System;

namespace TomPIT.Design
{
	public class ComponentUpdateArgs : EventArgs
	{
		public ComponentUpdateArgs(bool performLock)
		{
			PerformLock = performLock;
		}

		public bool PerformLock { get; }
	}
}
