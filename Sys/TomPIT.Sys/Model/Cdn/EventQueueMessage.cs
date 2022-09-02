using System;
using TomPIT.Cdn;
using TomPIT.Storage;
using TomPIT.SysDb.Events;

namespace TomPIT.Sys.Model.Cdn
{
	internal class EventQueueMessage : QueueMessage, IEventQueueMessage
	{
		public EventQueueMessage(IQueueMessage message, IEventDescriptor ev)
		{
			BufferKey = message.BufferKey;
			Created = message.Created;
			DequeueCount = message.DequeueCount;
			DequeueTimestamp = message.DequeueTimestamp;
			Expire = message.Expire;
			Id = message.Id;
			Message = message.Message;
			NextVisible = message.NextVisible;
			PopReceipt = message.PopReceipt;
			Queue = message.Queue;
			Scope = message.Scope;
			Arguments = ev.Arguments;
			Name = ev.Name;
			Callback = ev.Callback;
			MicroService = ev.MicroService;
		}
		public string Arguments {get;set;}

		public string Name {get;set;}

		public string Callback {get;set;}

		public Guid MicroService {get;set;}
	}
}
