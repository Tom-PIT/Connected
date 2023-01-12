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
				if (Worker.IsBusy)
					Worker.CancelAsync();
			});

			_worker = new BackgroundWorker
			{
				WorkerSupportsCancellation = true
			};

			_worker.RunWorkerCompleted += OnCompleted;
			_worker.DoWork += Dequeue;
		}

		public bool IsRunning => Worker.IsBusy;
		private int Counter { get; set; }
		private BackgroundWorker Worker => _worker;
		protected IDispatcher<T> Owner { get; }
		protected CancellationToken Cancel { get; }
		private DateTime LastRun { get; set; }
		private T Current { get; set; }
		public void Run()
		{
			Worker.RunWorkerAsync();
		}

		private void OnCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Completed?.Invoke(this, EventArgs.Empty);
		}

		private void Dequeue(object sender, DoWorkEventArgs e)
		{
			DoWork();
		}

		private void DoWork()
		{
			T item = default;

			try
			{
				while (Owner.Dequeue(out item))
				{
					Counter++;
					Current = item;

					if (item is null || item is IPopReceiptRecord pr && pr.NextVisible <= DateTime.UtcNow)
						continue;

					LastRun = DateTime.UtcNow;
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

			if (_worker is not null)
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

		public override string ToString()
		{
			var name = Current is null ? "null" : Current.GetType().GetProperty("Name")?.GetValue(Current).ToString();
			var id = Current is null ? "null" : Current.GetType().GetProperty("Id")?.GetValue(Current).ToString();
			var elapsed = LastRun == DateTime.MinValue ? 0 : DateTime.UtcNow.Subtract(LastRun).TotalMilliseconds;

			return $"name:{name}, id:{id}, count:{Counter}, elapsed:{elapsed}";
		}
	}
}