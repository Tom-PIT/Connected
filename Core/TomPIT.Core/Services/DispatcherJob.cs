using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Data;

namespace TomPIT.Services
{
	public abstract class DispatcherJob<T>
	{
		private CancellationTokenSource _cancel;
		private bool _isRunning = false;

		public DispatcherJob(Dispatcher<T> owner, CancellationTokenSource cancel)
		{
			Owner = owner;
			_cancel = cancel;
		}

		public void Start()
		{
			if (_isRunning)
				return;

			new Task(() => Dequeue(), _cancel.Token, TaskCreationOptions.LongRunning).Start();
		}

		protected Dispatcher<T> Owner { get; }

		private void Dequeue()
		{
			_isRunning = true;

			var token = _cancel.Token;

			while (!token.IsCancellationRequested)
			{
				try
				{
					if (!Owner.Dequeue(out T item))
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
					Console.WriteLine(ex.Message);
				}

				token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(1));
			}

			_isRunning = false;
		}

		protected abstract void DoWork(T item);
	}
}