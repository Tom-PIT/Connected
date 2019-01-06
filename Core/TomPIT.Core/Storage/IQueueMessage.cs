using System;

namespace TomPIT.Storage
{
	public interface IQueueMessage
	{
		string Id { get; }
		string Message { get; }
		DateTime Created { get; }
		DateTime Expire { get; }
		DateTime NextVisible { get; }
		Guid PopReceipt { get; }
		int DequeueCount { get; }
		string Queue { get; }
		DateTime DequeueTimestamp { get; }
	}
}
