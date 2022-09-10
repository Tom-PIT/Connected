using System;
using TomPIT.Cdn;
using TomPIT.Storage;
using TomPIT.SysDb.Events;

namespace TomPIT.Sys.Model.Cdn
{
	internal class EventQueueMessage : QueueMessage, IEventQueueMessage
	{
		public EventQueueMessage(IQueueMessage message, IEventDescriptor ev) : base(message)
		{
			Arguments = ev.Arguments;
			Name = ev.Name;
			Callback = ev.Callback;
			MicroService = ev.MicroService;
		}
		public string Arguments { get; set; }

		public string Name { get; set; }

		public string Callback { get; set; }

		public Guid MicroService { get; set; }
	}
}
