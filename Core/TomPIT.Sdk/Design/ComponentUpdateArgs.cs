using System;
using TomPIT.Runtime;

namespace TomPIT.Design
{
	public class ComponentUpdateArgs : EventArgs
	{
		public ComponentUpdateArgs(EnvironmentMode mode, bool performLock)
		{
			Mode = mode;
			PerformLock = performLock;
		}

		public EnvironmentMode Mode { get; }
		public bool PerformLock { get; }
	}
}
