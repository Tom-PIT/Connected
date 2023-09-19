using System;

namespace TomPIT.Storage
{
	public enum QueueScope
	{
		System = 0,
		Content = 1
	}

	public interface IQueueMessage
	{
		long Id { get; }
		string Message { get; }
		DateTime Created { get; }
		DateTime Expire { get; }
		DateTime NextVisible { get; }
		Guid PopReceipt { get; }
		int DequeueCount { get; }
		string Queue { get; }
		QueueScope Scope { get; }
		DateTime DequeueTimestamp { get; }
		string BufferKey { get; }
	}
}
