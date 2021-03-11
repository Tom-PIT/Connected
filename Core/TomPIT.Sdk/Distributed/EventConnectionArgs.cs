using System;

namespace TomPIT.Distributed
{
	public class EventConnectionArgs : EventArgs
	{
		public EventConnectionArgs(string connectionId, object proxy)
		{
			ConnectionId = connectionId;
			Proxy = proxy;
		}

		public string ConnectionId { get; }
		public object Proxy { get; }
	}
}
