using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TomPIT.Distributed
{
	internal class QueuedDispatcher<T> : IDispatcher<T>
	{
		private ConcurrentQueue<T> _items = null;
		private bool _disposed = false;

		public QueuedDispatcher(IDispatcher<T> owner, CancellationToken cancel)
		{
			Worker = owner.CreateWorker(this, cancel);

			Worker.Completed += OnCompleted;
		}

		private DispatcherJob<T> Worker { get; set; }
		private IDispatcher<T> Owner { get; set; }

		public int Count => Queue.Count;

		private ConcurrentQueue<T> Queue
		{
			get
			{
				if (_items == null)
					_items = new ConcurrentQueue<T>();

				return _items;
			}
		}

		public bool Dequeue(out T item)
		{
			return Queue.TryDequeue(out item);
		}

		public bool Enqueue(T item)
		{
			if (Disposed)
				return false;

			Queue.Enqueue(item);

			if (!Worker.IsRunning)
				Worker.Run();

			return true;
		}

		private void OnCompleted(object sender, EventArgs e)
		{
			try
			{
				if (sender is not DispatcherJob<T> job)
					return;

				Dispose();
			}
			catch { }
		}

		public bool Disposed => _disposed;
		public ProcessBehavior Behavior => ProcessBehavior.Queued;
		
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;

				if (disposing)
				{

					try
					{
						if (Worker != null)
						{
							Worker.Dispose();
							Worker = null;
						}
					}
					catch { }
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public DispatcherJob<T> CreateWorker(IDispatcher<T> owner, CancellationToken cancel)
		{
			return Owner.CreateWorker(owner, cancel);
		}

		public bool Enqueue(string queue, T item)
		{
			return Owner.Enqueue(queue, item);
		}
	}
}
