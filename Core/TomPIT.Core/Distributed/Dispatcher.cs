using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using TomPIT.Exceptions;

namespace TomPIT.Distributed
{
	public abstract class Dispatcher<T> : IDispatcher<T>
	{
		private bool _disposed = false;
		private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

		protected Dispatcher(int workerSize)
		{
			WorkerSize = workerSize;
			Queue = new();
			Jobs = new();
			QueuedDispatchers = new();
		}

		private CancellationTokenSource Cancel => _cancel;
		private int WorkerSize { get; }
		private ConcurrentQueue<T> Queue { get; }
		private List<DispatcherJob<T>> Jobs { get; }
		public int Available => Math.Max(0, WorkerSize * 4 - Queue.Count - QueuedDispatchers.Sum(f => f.Value.Count));
		private ConcurrentDictionary<string, QueuedDispatcher<T>> QueuedDispatchers { get; }
		public ProcessBehavior Behavior => ProcessBehavior.Parallel;
		public abstract DispatcherJob<T> CreateWorker(IDispatcher<T> owner, CancellationToken cancel);
		public bool Dequeue(out T item)
		{
			return Queue.TryDequeue(out item);
		}

		public bool Enqueue(string queue, T item)
		{
			if (EnsureDispatcher(queue) is not QueuedDispatcher<T> dispatcher)
				throw new RuntimeException($"{SR.ErrCannotCreateStackedDispatcher} ({queue})");

			return dispatcher.Enqueue(item);
		}

		public bool Enqueue(T item)
		{
			Queue.Enqueue(item);

			lock (Jobs)
			{
				var jobs = Jobs.Count;
				var items = Queue.Count;

				if (jobs > items && jobs <= WorkerSize)
					return true;
			}

			CreateWorker();

			return true;
		}

		private void CreateWorker()
		{
			var worker = CreateWorker(this, Cancel.Token);

			worker.Completed += OnCompleted;

			lock (Jobs)
			{
				Jobs.Add(worker);
			}

			worker.Run();
		}

		private void OnCompleted(object sender, EventArgs e)
		{
			if (sender is not DispatcherJob<T> job)
				return;

			try
			{
				if (Queue.IsEmpty)
					DisposeJob(job);
				else
					job.Run();
			}
			catch (Exception ex)
			{
				DisposeJob(job);

				Debug.WriteLine(ex.Message, "Dispatcher Completed Exception");
			}
		}

		private void DisposeJob(DispatcherJob<T> job)
		{
			lock (Jobs)
			{
				Jobs.Remove(job);
			}

			job.Completed -= OnCompleted;
			job.Dispose();
			job = null;

			if (Jobs.Count == 0 && !Queue.IsEmpty)
				CreateWorker();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{

					try
					{
						Cancel.Cancel();
						Queue.Clear();

						foreach (var job in Jobs)
							job.Dispose();

						Jobs.Clear();

						foreach (var dispatcher in QueuedDispatchers)
							dispatcher.Value.Dispose();

						QueuedDispatchers.Clear();
						Cancel.Dispose();
					}
					catch { }
				}

				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private QueuedDispatcher<T> EnsureDispatcher(string stack)
		{
			if (QueuedDispatchers.TryGetValue(stack, out QueuedDispatcher<T> result))
				return result;

			result = new QueuedDispatcher<T>(this, stack);

			result.Completed += OnQueuedCompleted;

			if (!QueuedDispatchers.TryAdd(stack, result))
			{
				if (QueuedDispatchers.TryGetValue(stack, out QueuedDispatcher<T> retryResult))
					return retryResult;
				else
					return null;
			}

			return result;
		}

		private void OnQueuedCompleted(object sender, EventArgs e)
		{
			var dispatcher = sender as QueuedDispatcher<T>;

			if (dispatcher.Count > 0)
				return;

			QueuedDispatchers.Remove(dispatcher.QueueName, out _);

			try
			{
				dispatcher.Dispose();
			}
			catch { }
		}
	}
}

