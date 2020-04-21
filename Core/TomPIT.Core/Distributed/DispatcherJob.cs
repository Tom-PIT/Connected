using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Data;

namespace TomPIT.Distributed
{
	public abstract class DispatcherJob<T>
	{
		private bool _isRunning = false;

		public DispatcherJob(Dispatcher<T> owner, CancellationToken cancel)
		{
			Owner = owner;
			Cancel = cancel;

			owner.Enqueued += OnEnqueued;

			OnEnqueued(this, EventArgs.Empty);
		}

		private async void OnEnqueued(object sender, EventArgs e)
		{
			if (_isRunning)
			{
				await Task.CompletedTask;
				return;
			}

			await Task.Run(async () => await Dequeue(), Cancel);
		}

		protected Dispatcher<T> Owner { get; }
		protected CancellationToken Cancel { get; }

		private Task Dequeue()
		{
			_isRunning = true;

			var token = Cancel;

			while (!token.IsCancellationRequested)
			{
				T item = default;

				try
				{
					if (!Owner.Dequeue(out item))
						break;

					if (item is IPopReceiptRecord)
					{
						var pa = item as IPopReceiptRecord;

						if (pa.NextVisible <= DateTime.UtcNow)
							continue;
					}

					DoWork(item);
				}
				catch (Exception ex)
				{
					try
					{
						OnError(item, ex);
					}
					catch
					{
						//eat exception to avoid process crash
					}
				}

				Task.Delay(1);
			}

			_isRunning = false;

			return Task.CompletedTask;
		}

		protected abstract void DoWork(T item);
		protected abstract void OnError(T item, Exception ex);
	}
}