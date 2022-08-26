using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Exceptions;

namespace TomPIT.Distributed
{
	public abstract class Dispatcher<T> : IDispatcher<T>
	{
		private ConcurrentQueue<T> _items = null;
		private List<DispatcherJob<T>> _workers = null;
		private Lazy<ConcurrentDictionary<string, QueuedDispatcher<T>>> _queuedDispatchers = new Lazy<ConcurrentDictionary<string, QueuedDispatcher<T>>>();
		private bool _disposed = false;
		private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

		protected Dispatcher(int workerSize)
		{
			WorkerSize = workerSize;

			new Task(() => OnScaveging(), Cancel.Token, TaskCreationOptions.LongRunning).Start();
		}

		private CancellationTokenSource Cancel => _cancel;

		private void OnScaveging()
		{
			var token = Cancel.Token;

			while (!Cancel.IsCancellationRequested)
			{
				try
				{
					var disposed = QueuedDispatchers.Where(f => f.Value.Disposed);

					foreach (var disposedDispatcher in disposed)
						QueuedDispatchers.Remove(disposedDispatcher.Key, out QueuedDispatcher<T> _);

					token.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
				}
				catch { }
			}
		}

		private int WorkerSize { get; }

		public abstract DispatcherJob<T> CreateWorker(IDispatcher<T> owner, CancellationToken cancel);

		public int Available => Math.Max(0, WorkerSize * 4) - Queue.Count - QueuedDispatchers.Sum(f => f.Value.Count);

		public bool Dequeue(out T item)
		{
			return Queue.TryDequeue(out item);
		}

		public bool Enqueue(string queue, T item)
		{
			var dispatcher = EnsureDispatcher(queue);

			if (dispatcher == null)
				throw new RuntimeException($"{SR.ErrCannotCreateStackedDispatcher} ({queue})");

			if (dispatcher.Disposed)
				return Enqueue(queue, item);
			else
				return dispatcher.Enqueue(item);
		}

		public bool Enqueue(T item)
		{
			Queue.Enqueue(item);

			if (Jobs.Count < WorkerSize)
			{
				var worker = CreateWorker(this, Cancel.Token);

				worker.Completed += OnCompleted;

				lock (Jobs)
				{
					Jobs.Add(worker);
				}

				worker.Run();
			}

			return true;
		}

		private void OnCompleted(object sender, EventArgs e)
		{
			try
			{
				if (sender is not DispatcherJob<T> job)
					return;

				if (job.Success)
				{
					lock (Jobs)
					{
						Jobs.Remove(job);
					}

					job.Dispose();
					job = null;
				}
				else
					job.Run();
			}
			catch { }
		}

		private ConcurrentQueue<T> Queue
		{
			get
			{
				if (_items == null)
					_items = new ConcurrentQueue<T>();

				return _items;
			}
		}

		private List<DispatcherJob<T>> Jobs
		{
			get
			{
				if (_workers == null)
					_workers = new List<DispatcherJob<T>>();

				return _workers;
			}
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
		}

		private QueuedDispatcher<T> EnsureDispatcher(string stack)
		{
			if (QueuedDispatchers.TryGetValue(stack, out QueuedDispatcher<T> result))
			{
				if (result.Disposed)
				{
					QueuedDispatchers.Remove(stack, out QueuedDispatcher<T> _);

					return EnsureDispatcher(stack);
				}

				return result;
			}

			result = new QueuedDispatcher<T>(this);

			if (!QueuedDispatchers.TryAdd(stack, result))
			{
				if (QueuedDispatchers.TryGetValue(stack, out QueuedDispatcher<T> retryResult))
				{
					if (retryResult.Disposed)
					{
						QueuedDispatchers.Remove(stack, out QueuedDispatcher<T> _);

						return EnsureDispatcher(stack);
					}
				}

				return retryResult;
			}

			return result;
		}

		private ConcurrentDictionary<string, QueuedDispatcher<T>> QueuedDispatchers => _queuedDispatchers.Value;

		public ProcessBehavior Behavior => ProcessBehavior.Parallel;
	}
}

