using System;
using System.Threading;

namespace TomPIT.Distributed
{
	public enum ProcessBehavior
	{
		Parallel = 1,
		Queued = 2
	}

	public interface IDispatcher<T> : IDisposable
	{
		bool Dequeue(out T item);
		bool Enqueue(T item);
		bool Enqueue(string queue, T item);
		DispatcherJob<T> CreateWorker(IDispatcher<T> owner, CancellationToken cancel);

		ProcessBehavior Behavior { get; }
	}
}
