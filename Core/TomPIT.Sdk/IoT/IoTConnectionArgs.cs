using System;

namespace TomPIT.IoT
{
	public class IoTConnectionArgs : EventArgs
	{
		public IoTConnectionArgs(string connectionId, string method)
		{
			ConnectionId = connectionId;
			Method = method;
		}

		public string ConnectionId { get; }
		public string Method { get; }
	}
}
