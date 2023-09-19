using System;
using System.Collections.Immutable;

namespace TomPIT.Worker.Services
{
	public record QueueSnapshot(int Processed, int InError, int Enqueued, DateTimeOffset RecordedAt);

	public interface IQueueMonitoringService
	{
		public event EventHandler<QueueSnapshot> OnTimeout;
		public void SignalProcessed();
		public void SignalEnqueued(int count);
		public void SignalError();

		public ImmutableList<QueueSnapshot> GetHistory();
	}
}
