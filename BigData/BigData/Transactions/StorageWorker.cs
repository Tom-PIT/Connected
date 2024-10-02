using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.BigData.Partitions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Diagnostics;
using TomPIT.Distributed;

namespace TomPIT.BigData.Transactions
{
	internal class StorageWorker : IDisposable
	{
		private bool _disposed = false;
		private BackgroundWorker _worker = null;
		private readonly Guid _id = Guid.NewGuid();

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

		public Guid Id => _id;
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
			Dump(null, "completed");
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
					{
						Dump(item, "expired");
						continue;
					}

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
				Dump(item, "timeout");

				Tenant.GetService<ITransactionService>().Ping(item.Message.PopReceipt, TimeSpan.FromMinutes(10));

				return Task.CompletedTask;
			}, TimeSpan.FromMinutes(5), Cancel);

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
			Dump(item, $"ex: {ex.Message}");

			Tenant.LogError(ex.Source, ex.Message, nameof(StorageJob));
			Tenant.GetService<ITransactionService>().Ping(item.Message.PopReceipt, TimeSpan.FromSeconds(5));
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
					Dump(item, $"{updater.LockedItems} locked items");

					if (updater.UpdateRowCount == 0)
					{
						Dump(item, "no  rows to update");

						Tenant.GetService<ITransactionService>().Ping(item.Message.PopReceipt, TimeSpan.FromSeconds(1));

						return;
					}
					else
					{
						Dump(item, $"{updater.UpdateRowCount} rows to update");

						var config = Tenant.GetService<IComponentService>().SelectConfiguration(item.Block.Partition) as IPartitionConfiguration;

						Tenant.GetService<ITransactionService>().Complete(item.Message.PopReceipt, item.Block.Token);
						Tenant.GetService<ITransactionService>().Prepare(config, updater.LockedItems);
					}
				}
			}
			finally
			{
				UpdaterPool.Release(item.Block.Partition);
			}
		}

		private void ValidateSchema(ITransactionBlock block)
		{
			Tenant.GetService<IPartitionService>().ValidateSchema(block.Partition);
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

		private void Dump(StorageWorkerItem item, string text)
		{
			if (item is null)
				Tenant.GetService<ILoggingService>().Dump($"StorageWorker, Partition:{Partition}, {text}.");
			else
				Tenant.GetService<ILoggingService>().Dump($"StorageWorker, Partition:{Partition}, Transaction:{item.Block.Transaction}, Block:{item.Block.Token},  {text}.");
		}
	}
}
