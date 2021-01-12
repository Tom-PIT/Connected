using System;
using TomPIT.Storage;

namespace TomPIT.Distributed
{
	public class QueueMessage : IQueueMessage
	{
		public string Id { get; set; }
		public string Message { get; set; }
		public DateTime Created { get; set; }
		public DateTime Expire { get; set; }
		public DateTime NextVisible { get; set; }
		public Guid PopReceipt { get; set; }
		public int DequeueCount { get; set; }
		public string Queue { get; set; }
		public DateTime DequeueTimestamp { get; set; }
		public QueueScope Scope { get; set; }
	}
}
