using System;
using TomPIT.Storage;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	internal class QueueMessage : IQueueMessage, IQueueMessageModifier
	{
		public string Id { get; set; }

		public string Message { get; set; }

		public DateTime Created { get; set; }

		public DateTime Expire { get; set; }

		public DateTime NextVisible { get; set; }

		public Guid PopReceipt { get; set; }

		public int DequeueCount { get; set; }

		public string Queue { get; set; }

		public QueueScope Scope { get; set; }

		public DateTime DequeueTimestamp { get; set; }

		public string BufferKey { get; set; }

		public void Modify(DateTime nextVisible, DateTime dequeueTimestamp, int dequeueCount, Guid popReceipt)
		{
			NextVisible = nextVisible;
			DequeueTimestamp = dequeueTimestamp;
			DequeueCount = dequeueCount;
			PopReceipt = popReceipt;
		}
	}
}
