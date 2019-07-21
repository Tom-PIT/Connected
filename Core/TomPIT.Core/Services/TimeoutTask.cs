using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Services
{
	public sealed class TimeoutTask 
	{
		private readonly Func<Task> _action;
		private Task _task;
		private readonly TimeSpan _interval;
		private CancellationTokenSource _cancellationSource;

		public TimeoutTask(Func<Task> scheduledAction, TimeSpan interval)
		{
			_action = scheduledAction;
			_interval = interval;
		}

		public void Start()
		{
			if (IsRunning)
				return;

			_cancellationSource = new CancellationTokenSource();
			_task = Timeout();

			IsRunning = true;
		}

		private bool IsRunning { get; set; }

		public void Stop()
		{
			try
			{
				if (!IsRunning)
					return;

				_cancellationSource.Cancel();
			}
			catch (OperationCanceledException) { }
			finally
			{
				IsRunning = false;
			}
		}

		private Task Timeout()
		{
			return Task.Run(async () =>
			{
				try
				{
					while (true)
					{
						await Task.Delay(_interval, _cancellationSource.Token).ConfigureAwait(false);
						await _action().ConfigureAwait(false);
					}
				}
				finally
				{
					IsRunning = false;
				}
			}, _cancellationSource.Token);
		}
	}
}