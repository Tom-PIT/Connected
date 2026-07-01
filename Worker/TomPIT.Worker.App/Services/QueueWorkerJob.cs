using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

			if (item.DequeueCount > 30)
			{
				MiddlewareDescriptor.Current.Tenant.LogError($"StaleQueue", $"Queue message {item.Id} has been dequeued more than 30 times (15 minutes). Deleting message to prevent infinite processing loop.", nameof(QueueWorkerJob));
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

				_timeout = new TimeoutTask(() =>
				{
					Instance.SysProxy.Management.Queue.Ping(item.PopReceipt, TimeSpan.FromSeconds(120));

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
					_timeout = null;
				}

				Instance.SysProxy.Management.Queue.Complete(item.PopReceipt);

				_queueMonitoringService?.SignalProcessed();

				MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"{typeof(QueueWorkerJob).FullName.PadRight(64)}| Completed queue entry: {Serializer.Serialize(item)}");
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
			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, nameof(QueueWorkerJob));

			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Instance.SysProxy.Management.Queue.Ping(item.PopReceipt, TimeSpan.FromSeconds(30));

			MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"{typeof(QueueWorkerJob).FullName.PadRight(64)}| Error processing entry: {Serializer.Serialize(item)} => {ex}");

			_queueMonitoringService?.SignalError();
		}
	}
}
