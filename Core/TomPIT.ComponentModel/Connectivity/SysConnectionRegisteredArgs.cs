using System;

namespace TomPIT.Connectivity
{
	public class SysConnectionRegisteredArgs : EventArgs
	{
		public SysConnectionRegisteredArgs(ISysConnection connection)
		{
			Connection = connection;
		}

		public ISysConnection Connection { get; }
	}
}
