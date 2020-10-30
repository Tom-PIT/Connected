using System;

namespace TomPIT.IoT
{
	public enum IoTConnectionMethod
	{
		Device = 1,
		Client = 2
	}
	public class IoTConnectionArgs : EventArgs
	{
		public IoTConnectionArgs(string connectionId, IoTConnectionMethod method)
		{
			ConnectionId = connectionId;
			Method = method;
		}

		public string ConnectionId { get; }
		public IoTConnectionMethod Method { get; }
	}
}
