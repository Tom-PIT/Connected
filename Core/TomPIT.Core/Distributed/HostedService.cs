using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Distributed
{
	public abstract class HostedService : BackgroundService
	{
		//private Task _executingTask = null;

		protected bool Initialized { get; private set; }
		//public virtual Task StartAsync(CancellationToken cancel)
		//{
		//if (cancel.IsCancellationRequested)
		//	return Task.CompletedTask;

		//_executingTask = ExecuteAsync(cancel);

		//if (_executingTask.IsCompleted)
		//	return _executingTask;

		//return Task.CompletedTask;
		//}

		//public virtual async Task StopAsync(CancellationToken cancel)
		//{
		//	if (_executingTask == null)
		//		return;

		//	await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancel));
		//}

		protected override async Task ExecuteAsync(CancellationToken cancel)
		{
			do
			{
				try
				{
					if (!Initialized)
						Initialized = OnInitialize(cancel);

					if (Initialized)
						await OnExecute(cancel);
				}
				catch (Exception ex)
				{
					//TODO: handle exception
					Debug.WriteLine("Hosted service exception " + ex.ToString());
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

		protected virtual bool OnInitialize(CancellationToken cancel)
		{
			return true;
		}

		protected abstract Task OnExecute(CancellationToken cancel);

		protected TimeSpan IntervalTimeout { get; set; } = TimeSpan.FromSeconds(5);
	}
}
