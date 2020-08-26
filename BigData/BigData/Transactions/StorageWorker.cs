using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.BigData.Partitions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.BigData.Transactions
{
	internal class StorageWorker : IDisposable
	{
		private bool _disposed = false;
		private BackgroundWorker _worker = null;
		private TimeoutTask _timeout = null;

		public event EventHandler Completed;
		public StorageWorker(Guid partition, CancellationToken cancel)
		{
			Partition = partition;
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

		public Guid Partition { get; }
		private CancellationToken Cancel { get; }

		private void Dequeue(object sender, DoWorkEventArgs e)
		{
			StorageWorkerItem item = null;

			try
			{
				while (StoragePool.Dequeue(Partition, out item))
				{
					if (item.Message.NextVisible <= DateTime.UtcNow)
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

		private void DoWork(StorageWorkerItem item)
		{
			var timeout = new TimeoutTask(() =>
			{
				MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Ping(item.Message.PopReceipt, TimeSpan.FromMinutes(1));
				return Task.CompletedTask;
			}, TimeSpan.FromSeconds(45), Cancel);

			timeout.Start();

			try
			{
				Invoke(item);
			}
			finally
			{
				timeout.Stop();
				timeout = null;
			}

		}
		private void OnError(StorageWorkerItem item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, nameof(StorageJob));
			MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Ping(item.Message.PopReceipt, TimeSpan.FromSeconds(5));
		}

		private void Invoke(StorageWorkerItem item)
		{
			ValidateSchema(item.Block);

			try
			{
				var updater = new Updater(item.Block);

				updater.Execute();

				if (updater.LockedItems != null && updater.LockedItems.Count > 0)
				{
					if (updater.UpdateRowCount == 0)
					{
						MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Ping(item.Message.PopReceipt, TimeSpan.FromSeconds(1));
						return;
					}
					else
					{
						var config = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(item.Block.Partition) as IPartitionConfiguration;

						MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Prepare(config, updater.LockedItems);
					}
				}

				MiddlewareDescriptor.Current.Tenant.GetService<ITransactionService>().Complete(item.Message.PopReceipt, item.Block.Token);
			}
			finally
			{
				UpdaterPool.Release(item.Block.Partition);
			}
		}

		private void ValidateSchema(ITransactionBlock block)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().ValidateSchema(block.Partition);
		}

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
