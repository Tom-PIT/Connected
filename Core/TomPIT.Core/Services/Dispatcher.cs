using System.Collections.Concurrent;
using System.Threading;

namespace TomPIT.Services
{
	public abstract class Dispatcher<T>
	{
		private ConcurrentQueue<T> _items = null;
		private ConcurrentBag<DispatcherJob<T>> _workers = null;
		private CancellationTokenSource _cancel = null;

		public Dispatcher(CancellationTokenSource cancel, int workerSize)
		{
			_cancel = cancel;

			for (int i = 0; i < workerSize; i++)
				Jobs.Add(CreateWorker(_cancel));
		}

		protected abstract DispatcherJob<T> CreateWorker(CancellationTokenSource cancel);

		public int Available { get { return Jobs.Count - Queue.Count; } }

		public bool Dequeue(out T item)
		{
			return Queue.TryDequeue(out item);
		}

		public void Enqueue(T item)
		{
			Queue.Enqueue(item);

			foreach (var i in Jobs)
				i.Start();
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

		private ConcurrentBag<DispatcherJob<T>> Jobs
		{
			get
			{
				if (_workers == null)
					_workers = new ConcurrentBag<DispatcherJob<T>>();

				return _workers;
			}
		}
	}
}
