using System;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Distributed
{
	public sealed class TimeoutTask
	{
		private readonly Func<Task> _action;
		private Task _task;
		private readonly TimeSpan _interval;

		public TimeoutTask(Func<Task> scheduledAction, TimeSpan interval, CancellationToken cancel)
		{
			_action = scheduledAction;
			_interval = interval;
			CancelSource = new CancellationTokenSource();

			cancel.Register(() =>
			{
				CancelSource.Cancel();
			});
		}

		private CancellationTokenSource CancelSource { get; }

		public void Start()
		{
			if (IsRunning)
				return;

			_task = Timeout();

			IsRunning = true;
		}

		private bool IsRunning { get; set; }

		public void Stop()
		{
			if (!IsRunning)
				return;
			try
			{
				CancelSource.Cancel();

			}
			catch (OperationCanceledException)
			{

			}
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
					while (!CancelSource.IsCancellationRequested && IsRunning)
					{
						await Task.Delay(_interval, CancelSource.Token).ConfigureAwait(false);
						await _action().ConfigureAwait(false);
					}
				}
				finally
				{
					IsRunning = false;
				}
			}, CancelSource.Token);
		}
	}
}