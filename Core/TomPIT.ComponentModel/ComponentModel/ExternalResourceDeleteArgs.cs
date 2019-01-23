using System;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	public class ExternalResourceDeleteArgs : EventArgs
	{
		public ExternalResourceDeleteArgs(ISysConnection connection)
		{
			Connection = connection;
		}

		public ISysConnection Connection { get; }
	}
}
