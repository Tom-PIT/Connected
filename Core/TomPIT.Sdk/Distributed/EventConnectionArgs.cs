using System;

namespace TomPIT.Distributed
{
	public class EventConnectionArgs : EventArgs
	{
		public EventConnectionArgs(string connectionId)
		{
			ConnectionId = connectionId;
		}

		public string ConnectionId { get; }
	}
}
