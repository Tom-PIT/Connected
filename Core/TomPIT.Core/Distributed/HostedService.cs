﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TomPIT.Distributed
{
	public abstract class HostedService : IHostedService
	{
		private Task _executingTask = null;
		private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

		protected bool Initialized { get; private set; }
		public virtual Task StartAsync(CancellationToken cancellationToken)
		{
			_executingTask = ExecuteAsync(_cancel.Token);

			if (_executingTask.IsCompleted)
				return _executingTask;

			return Task.CompletedTask;
		}

		public virtual async Task StopAsync(CancellationToken cancellationToken)
		{
			if (_executingTask == null)
				return;

			try
			{
				_cancel.Cancel();
			}
			finally
			{
				await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
			}
		}

		protected virtual async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			do
			{
				try
				{
					if (!Initialized)
						Initialized = Initialize();

					if (Initialized)
						await Process();
				}
				catch
				{
					//TODO: handle exception
				}

				if (Initialized)
					await Task.Delay(IntervalTimeout, stoppingToken);
				else
					await Task.Delay(1000, stoppingToken);
			}
			while (!stoppingToken.IsCancellationRequested);
		}

		protected virtual bool Initialize()
		{
			return true;
		}

		protected abstract Task Process();

		protected TimeSpan IntervalTimeout { get; set; } = TimeSpan.FromSeconds(5);
	}
}