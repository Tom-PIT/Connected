using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TomPIT.Distributed
{
	public abstract class HostedService : IHostedService
	{
		private Task _executingTask = null;

		protected bool Initialized { get; private set; }
		public virtual Task StartAsync(CancellationToken cancel)
		{
			if (cancel.IsCancellationRequested)
				return Task.CompletedTask;

			_executingTask = ExecuteAsync(cancel);

			if (_executingTask.IsCompleted)
				return _executingTask;

			return Task.CompletedTask;
		}

		public virtual async Task StopAsync(CancellationToken cancel)
		{
			if (_executingTask == null)
				return;

			await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancel));
		}

		protected virtual async Task ExecuteAsync(CancellationToken cancel)
		{
			do
			{
				try
				{
					if (!Initialized)
						Initialized = Initialize(cancel);

					if (Initialized)
						await Process(cancel);
				}
				catch
				{
					//TODO: handle exception
				}

				if (Initialized && IntervalTimeout == TimeSpan.Zero)
					break;

				if (Initialized)
					await Task.Delay(IntervalTimeout, cancel);
				else
					await Task.Delay(1000, cancel);
			}
			while (!cancel.IsCancellationRequested);
		}

		protected virtual bool Initialize(CancellationToken cancel)
		{
			return true;
		}

		protected abstract Task Process(CancellationToken cancel);

		protected TimeSpan IntervalTimeout { get; set; } = TimeSpan.FromSeconds(5);
	}
}
