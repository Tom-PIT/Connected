using System;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Distributed
{
	public sealed class TimeoutTask : IDisposable
	{
		public event EventHandler<Exception> Failed;

		private readonly Func<Task> _pingAction;
		private Task _pingTask;
		private readonly TimeSpan _pingInterval;

		private readonly Func<Task> _lifespanAction;
		private Task _lifespanTask;
		private readonly TimeSpan _lifespan;

		public TimeoutTask(Func<Task> pingAction, TimeSpan pingInterval, Func<Task> lifespanAction, TimeSpan lifespan, CancellationToken cancel)
			: this(pingAction, pingInterval, cancel)
		{
			_lifespanAction = lifespanAction;
			_lifespan = lifespan;
		}
		public TimeoutTask(Func<Task> pingAction, TimeSpan pingInterval, CancellationToken cancel)
		{
			_pingAction = pingAction;
			_pingInterval = pingInterval;

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

			if (_pingInterval > TimeSpan.Zero)
				_pingTask = Timeout();

			if (_lifespan > TimeSpan.Zero)
				_lifespanTask = Lifespan();

			IsRunning = true;
		}

		private bool IsRunning { get; set; }

		public void Stop()
		{
			if (CancelSource.IsCancellationRequested)
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
					while (!CancelSource.IsCancellationRequested || IsRunning)
					{
						if (_pingTask is not null)
						{
							await Task.Delay(_pingInterval, CancelSource.Token).ConfigureAwait(false);

							try
							{
								await _pingAction().ConfigureAwait(false);
							}
							catch (Exception ex) when (ex is not OperationCanceledException)
							{
								/*
								 * A failed ping must not stop the keepalive loop, otherwise the queue
								 * entry silently becomes visible again while it is still being processed.
								 */
								try
								{
									Failed?.Invoke(this, ex);
								}
								catch
								{
									/*
									 * A misbehaving subscriber must not stop the keepalive loop either.
									 */
								}
							}
						}
					}
				}
				catch (TaskCanceledException)
				{
					/*
					 * Do nothing, it is expected to fire when a timeout is cancelled.
					 */
				}
				finally
				{
					IsRunning = false;
				}
			}, CancelSource.Token);
		}

		private Task Lifespan()
		{
			return Task.Run(async () =>
			{
				try
				{
					while (!CancelSource.IsCancellationRequested || IsRunning)
					{
						if (_lifespanTask is not null)
						{
							await Task.Delay(_lifespan, CancelSource.Token).ConfigureAwait(false);

							try
							{
								await _lifespanAction().ConfigureAwait(false);
							}
							catch (Exception ex) when (ex is not OperationCanceledException)
							{
								try
								{
									Failed?.Invoke(this, ex);
								}
								catch
								{
									/*
									 * A misbehaving subscriber must not stop the keepalive loop either.
									 */
								}
							}
						}
					}
				}
				catch (TaskCanceledException)
				{
					/*
					 * Do nothing, it is expected to fire when a timeout is cancelled.
					 */
				}
				finally
				{
					IsRunning = false;
				}
			}, CancelSource.Token);
		}

		public void Dispose()
		{
			Stop();
			CancelSource.Dispose();

			if (_pingTask is not null)
			{
				_pingTask.Dispose();
				_pingTask = null;
			}

			if (_lifespanTask is not null)
			{
				_lifespanTask.Dispose();
				_lifespanTask = null;
			}
		}
	}
}