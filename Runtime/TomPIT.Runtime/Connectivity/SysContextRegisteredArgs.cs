using System;

namespace TomPIT.Net
{
	public class SysContextRegisteredArgs : EventArgs
	{
		public SysContextRegisteredArgs(ISysContext context)
		{
			Context = context;
		}

		public ISysContext Context { get; }
	}
}
