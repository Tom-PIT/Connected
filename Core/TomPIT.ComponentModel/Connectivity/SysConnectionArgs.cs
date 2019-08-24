using System;

namespace TomPIT.Connectivity
{
	public class SysConnectionArgs : EventArgs
	{
		public SysConnectionArgs(ISysConnection connection)
		{
			Connection = connection;
		}

		public ISysConnection Connection { get; }
	}
}
