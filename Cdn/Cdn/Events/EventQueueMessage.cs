using System;
using TomPIT.Distributed;

namespace TomPIT.Cdn.Events
{
	internal class EventQueueMessage : QueueMessage, IEventQueueMessage
	{
		public string Arguments { get; set; }
		public string Name { get; set; }
		public string Callback { get; set; }
		public Guid MicroService { get; set; }
	}
}
