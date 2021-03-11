using System;
using System.ComponentModel;
using System.Threading;
using TomPIT.Data;

namespace TomPIT.Distributed
{
	public abstract class DispatcherJob<T> : IDisposable
	{
		private bool _disposed = false;
		private BackgroundWorker _worker = null;

		public event EventHandler Completed;
		public DispatcherJob(IDispatcher<T> owner, CancellationToken cancel)
		{
			Owner = owner;
			Cancel = cancel;

			cancel.Register(() =>
			{
				Worker.CancelAsync();
			});

		}

		public void Run()
		{
			Worker.RunWorkerAsync();
		}

		public Guid Id => Guid.NewGuid();
		public bool IsRunning => Worker.IsBusy;
		public string Stack { get; set; }
		private BackgroundWorker Worker
		{
			get
			{
				if (_worker == null)
				{
					_worker = new BackgroundWorker();
					_worker.RunWorkerCompleted += OnCompleted;
					_worker.DoWork += Dequeue;
				}

				return _worker;
			}
		}

		private void OnCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Completed?.Invoke(this, EventArgs.Empty);
		}

		protected IDispatcher<T> Owner { get; }
		protected CancellationToken Cancel { get; }

		private void Dequeue(object sender, DoWorkEventArgs e)
		{
			T item = default;

			try
			{
				while (Owner.Dequeue(out item))
				{
					if (item is IPopReceiptRecord pr && pr.NextVisible <= DateTime.UtcNow)
						continue;

					DoWork(item);
				}
			}
			catch (Exception ex)
			{
				try
				{
					OnError(item, ex);
				}
				catch
				{
				}
			}
		}

		protected abstract void DoWork(T item);
		protected abstract void OnError(T item, Exception ex);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (_worker != null)
			{
				try
				{
					_worker.Dispose();
					_worker = null;
				}
				catch
				{

				}
			}

			if (disposing)
				OnDisposing();

			_disposed = true;
		}

		protected virtual void OnDisposing()
		{

		}
	}
}