using System;

namespace TomPIT.Cdn.Events
{
	public class EventAcknowledgeArgs: EventArgs
	{
		public string Client { get; set; }
		public ulong Id { get; set; }
	}
}
