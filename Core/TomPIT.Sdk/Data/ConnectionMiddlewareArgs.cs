using System;

namespace TomPIT.Data
{
	public class ConnectionMiddlewareArgs : EventArgs
	{
		public ConnectionMiddlewareArgs(ConnectionStringContext connectionContext)
		{
			ConnectionContext = connectionContext;
		}
		public ConnectionStringContext ConnectionContext { get; }
	}
}
