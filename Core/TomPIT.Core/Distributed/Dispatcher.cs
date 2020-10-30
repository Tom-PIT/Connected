using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace TomPIT.Distributed
{
	public abstract class Dispatcher<T> : IDisposable
	{
		private ConcurrentQueue<T> _items = null;
		private List<DispatcherJob<T>> _workers = null;
		private readonly CancellationToken _cancel;
		private bool _disposed = false;

		protected Dispatcher(CancellationToken cancel, int workerSize)
		{
			_cancel = cancel;
			WorkerSize = workerSize;
		}

		private int WorkerSize { get; }

		protected abstract DispatcherJob<T> CreateWorker(CancellationToken cancel);

		public int Available { get { return Math.Max(0, WorkerSize * 4) - Queue.Count; } }

		public bool Dequeue(out T item)
		{
			return Queue.TryDequeue(out item);
		}

		public void Enqueue(T item)
		{
			Queue.Enqueue(item);

			if (Jobs.Count < WorkerSize)
			{
				var worker = CreateWorker(_cancel);

				worker.Completed += OnCompleted;

				lock (Jobs)
				{
					Jobs.Add(worker);
				}

				worker.Run();
			}
		}

		private void OnCompleted(object sender, EventArgs e)
		{
			try
			{
				if (!(sender is DispatcherJob<T> job))
					return;

				lock (Jobs)
				{
					Jobs.Remove(job);
				}

				job.Dispose();
				job = null;
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
						Jobs.Clear();
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
	}
}

