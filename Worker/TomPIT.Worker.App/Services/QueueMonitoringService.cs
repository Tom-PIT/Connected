using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;

namespace TomPIT.Worker.Services
{
	public class QueueMonitoringService : IQueueMonitoringService, IDisposable
	{
		private readonly Timer _timer;
		private int _processed = 0;
		private int _inError = 0;
		private int _enqueued = 0;

		private readonly object _timeoutLock = new();

		private readonly ConcurrentQueue<QueueSnapshot> _history = new();

		public event EventHandler<QueueSnapshot> OnTimeout;

		public QueueMonitoringService()
		{
			_timer = new Timer(new TimerCallback((e) => ProcessData()), null, 1000, 1000);
		}

		private void ProcessData()
		{
			lock (_timeoutLock)
			{
				var state = new QueueSnapshot(Processed: _processed, InError: _inError, Enqueued: _enqueued, RecordedAt: DateTimeOffset.UtcNow);

				_history.Enqueue(state);

				try
				{
					OnTimeout?.Invoke(this, state);
				}
				catch { }

				ResetCounters();

				if (_history.Count > 3000)
				{
					while (!_history.TryDequeue(out _)) { }
				}
			}
		}

		private void ResetCounters()
		{
			lock (_timeoutLock)
			{
				_processed = 0;
				_inError = 0;
				_enqueued = 0;
			}
		}

		public ImmutableList<QueueSnapshot> GetHistory()
		{
			lock (_timeoutLock)
			{
				return _history.ToImmutableList();
			}
		}

		public void SignalError()
		{
			lock (_timeoutLock)
			{
				_inError++;
			}
		}

		public void SignalProcessed()
		{
			lock (_timeoutLock)
			{
				_processed++;
			}
		}

		public void SignalEnqueued(int enqueued = 1)
		{
			lock (_timeoutLock)
			{
				_enqueued += enqueued;
			}
		}

		public void Dispose()
		{
			_timer?.Dispose();
		}
	}
}
