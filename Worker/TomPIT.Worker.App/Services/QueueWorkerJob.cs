using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.Worker.Workers;

namespace TomPIT.Worker.Services
{
	public class QueueWorkerJob : DispatcherJob<IQueueMessage>
	{
		/*
		 * item.Id is not a stable identity across dequeues: the in-memory queue is
		 * periodically flushed to and reloaded from the underlying table, which can
		 * reassign it. (Message, Created) stays constant for the lifetime of a logical
		 * entry, so it's used to detect a duplicate dequeue of an entry already in flight.
		 */
		private static readonly ConcurrentDictionary<(string Message, DateTime Created), byte> InFlight = new();

		/*
		 * Backstop on dequeues, unchanged. DequeueCount is raised on every dequeue, including
		 * re-dequeues of healthy messages that merely waited (queued workers, restarts, skips),
		 * so it must not be the poison criterion.
		 */
		private const int MaxDequeueCount = 30;

		/*
		 * Poison criterion: handler failures observed by this process. With the OnError backoff
		 * (30s, 60s, 120s, 240s, 480s, 600s, 600s) the 8th failure lands ~35 minutes after the
		 * first and the message is dropped there. Held in memory like InFlight, so a restart
		 * starts counting again, which errs on the side of keeping messages.
		 */
		private const int MaxFailures = 8;

		private static readonly ConcurrentDictionary<(string Message, DateTime Created), (int Count, DateTime Last)> Failures = new();

		private readonly IQueueMonitoringService _queueMonitoringService;
		private TimeoutTask _timeout = null;
		public QueueWorkerJob(IDispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
			_queueMonitoringService = Tenant.GetService<IQueueMonitoringService>();
		}

		protected override void DoWork(IQueueMessage item)
		{
			if (item.NextVisible.AddSeconds(-10) <= DateTime.UtcNow)
				return;

			Instance.SysProxy.Management.Queue.Ping(item.PopReceipt, TimeSpan.FromSeconds(120));

			if (item.NextVisible <= DateTime.UtcNow)
				return;

			if (item.DequeueCount > MaxDequeueCount)
			{
				MiddlewareDescriptor.Current.Tenant.LogError($"StaleQueue", $"Queue message {item.Id} has been dequeued more than {MaxDequeueCount} times. Deleting message to prevent infinite processing loop.", nameof(QueueWorkerJob));
				MiddlewareDescriptor.Current.Tenant.LogError($"StaleQueue", Serializer.Serialize(item), nameof(QueueWorkerJob));

				Instance.SysProxy.Management.Queue.Complete(item.PopReceipt);
				_queueMonitoringService?.SignalProcessed();
				return;
			}

			var inFlightKey = (item.Message, item.Created);

			if (!InFlight.TryAdd(inFlightKey, 0))
			{
				MiddlewareDescriptor.Current.Tenant.LogWarning(nameof(QueueWorkerJob), $"Queue entry {item.Id} (PopReceipt: {item.PopReceipt}) is already being processed. Skipping duplicate dequeue.", nameof(QueueWorkerJob));
				return;
			}

			try
			{
				using var queue = new Queue(item);

				/*
				 * TimeoutTask.Stop only cancels the loop, it does not wait for a keepalive ping that is
				 * already in flight. The gate lets the finally block below wait for such a ping and
				 * guarantees no keepalive runs afterwards, so the final Complete or the backoff Ping in
				 * OnError always has the last word on NextVisible.
				 */
				var keepAliveGate = new object();
				var keepAliveStopped = false;

				_timeout = new TimeoutTask(() =>
				{
					lock (keepAliveGate)
					{
						if (!keepAliveStopped)
							Instance.SysProxy.Management.Queue.Ping(item.PopReceipt, TimeSpan.FromSeconds(120));
					}

					return Task.CompletedTask;
				}, TimeSpan.FromSeconds(90), Cancel);

				_timeout.Failed += (s, ex) => MiddlewareDescriptor.Current.Tenant.LogWarning(nameof(QueueWorkerJob), $"Keepalive ping failed for queue entry {item.Id} (PopReceipt: {item.PopReceipt}): {ex.Message}", nameof(QueueWorkerJob));

				_timeout.Start();

				try
				{
					if (!Invoke(queue))
						return;
				}
				finally
				{
					_timeout.Stop();

					lock (keepAliveGate)
					{
						keepAliveStopped = true;
					}

					_timeout.Dispose();
					_timeout = null;
				}

				Failures.TryRemove(inFlightKey, out _);

				/*
				 * The handler has succeeded. A failing Complete is an infrastructure problem, not a handler
				 * failure, so it must not reach OnError and count toward the poison limit.
				 */
				try
				{
					Instance.SysProxy.Management.Queue.Complete(item.PopReceipt);
				}
				catch (Exception completeEx)
				{
					MiddlewareDescriptor.Current.Tenant.LogWarning(nameof(QueueWorkerJob), $"Complete failed for queue entry {item.Id} (PopReceipt: {item.PopReceipt}): {completeEx.Message}", nameof(QueueWorkerJob));
					return;
				}

				_queueMonitoringService?.SignalProcessed();

				MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"{typeof(QueueWorkerJob).FullName,-64}| Completed queue entry: {Serializer.Serialize(item)}");
			}
			finally
			{
				InFlight.TryRemove(inFlightKey, out _);
			}
		}

		private bool Invoke(Queue queue)
		{
			try
			{
				if (!queue.Invoke(Owner.Behavior))
				{
					Owner.Enqueue($"{queue.QueueName}_{queue.Message.BufferKey}", queue.Message);
					return false;
				}

				return true;
			}
			catch (ValidationException ex)
			{
				if (queue.HandlerInstance.ValidationFailed == Cdn.QueueValidationBehavior.Complete)
				{
					MiddlewareDescriptor.Current.Tenant.LogWarning(ex.Source, ex.Message, LogCategories.Worker);
					return true;
				}
				else
					throw;
			}
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			/*
			 * Shutdown cancellation is not a handler failure: don't count it and don't push the message
			 * back with a backoff, it should simply be picked up again after the restart.
			 */
			if (Cancel.IsCancellationRequested || ex is OperationCanceledException)
				return;

			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, $"{ex.Message} | {Truncate(item.Message, 256)}", nameof(QueueWorkerJob));

			MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"{typeof(QueueWorkerJob).FullName,-64}| Error processing entry: {Serializer.Serialize(item)} => {ex}");

			var failureKey = (item.Message, item.Created);
			var failures = Failures.AddOrUpdate(failureKey, _ => (1, DateTime.UtcNow), (_, e) => (e.Count + 1, DateTime.UtcNow)).Count;

			PruneFailures();

			try
			{
				if (failures >= MaxFailures)
				{
					MiddlewareDescriptor.Current.Tenant.LogError($"StaleQueue", $"Queue message {item.Id} failed {failures} times. Deleting message to prevent infinite processing loop.", nameof(QueueWorkerJob));
					MiddlewareDescriptor.Current.Tenant.LogError($"StaleQueue", Serializer.Serialize(item), nameof(QueueWorkerJob));

					Instance.SysProxy.Management.Queue.Complete(item.PopReceipt);
					Failures.TryRemove(failureKey, out _);
				}
				else
					Instance.SysProxy.Management.Queue.Ping(item.PopReceipt, GetRetryDelay(item, failures));
			}
			catch (Exception pingEx)
			{
				MiddlewareDescriptor.Current.Tenant.LogWarning(nameof(QueueWorkerJob), $"Retry delay or poison removal was not applied for queue entry {item.Id} (PopReceipt: {item.PopReceipt}): {pingEx.Message}", nameof(QueueWorkerJob));
			}

			_queueMonitoringService?.SignalError();
		}

		/* Failure entries of messages that vanished (expired, completed elsewhere) must not pile up. */
		private static void PruneFailures()
		{
			if (Failures.Count < 100)
				return;

			/* A live message fails at least every ~10 min (backoff cap), so 1 h of silence means it is gone. */
			var cutoff = DateTime.UtcNow.AddHours(-1);

			foreach (var entry in Failures)
			{
				if (entry.Value.Last < cutoff)
					Failures.TryRemove(entry.Key, out _);
			}
		}

		private static string Truncate(string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;

			return value.Length <= maxLength ? value : value[..maxLength] + "...";
		}

		/*
		 * Exponential backoff: 30s, 60s, 120s, 240s, 480s, then capped at 10 min.
		 * Keeps a failing message from sorting to the front of the queue on every poll.
		 * The delay never extends past the message's Expire, so a short-lived message still
		 * gets a final retry instead of being silently dropped while it waits.
		 */
		private static TimeSpan GetRetryDelay(IQueueMessage item, int failures)
		{
			var exponent = Math.Clamp(failures - 1, 0, 5);
			var delay = TimeSpan.FromSeconds(Math.Min(30 * Math.Pow(2, exponent), 600));
			var remaining = item.Expire - DateTime.UtcNow;

			if (remaining < delay)
				delay = remaining;

			return delay < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) : delay;
		}
	}
}
