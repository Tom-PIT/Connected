using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
		private readonly IQueueMonitoringService _queueMonitoringService;
		private TimeoutTask _timeout = null;
		public QueueWorkerJob(IDispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
			_queueMonitoringService = Tenant.GetService<IQueueMonitoringService>();
		}

		protected override void DoWork(IQueueMessage item)
		{
			using var queue = new Queue(item);

			_timeout = new TimeoutTask(() =>
			{
				MiddlewareDescriptor.Current.Tenant.Post(MiddlewareDescriptor.Current.Tenant.CreateUrl("QueueManagement", "Ping"), new
				{
					item.PopReceipt
				});

				return Task.CompletedTask;
			}, TimeSpan.FromSeconds(90), Cancel);


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

			MiddlewareDescriptor.Current.Tenant.Post(MiddlewareDescriptor.Current.Tenant.CreateUrl("QueueManagement", "Complete"), new
			{
				item.PopReceipt
			});

			_queueMonitoringService?.SignalProcessed();

			MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"{typeof(QueueWorkerJob).FullName.PadRight(64)}| Completed queue entry: {Serializer.Serialize(item)}");
		}

		private bool Invoke(Queue queue)
		{
			Queue q = null;

			try
			{
				if (!queue.Invoke(Owner.Behavior))
				{
					Owner.Enqueue($"{q.QueueName}_{queue.Message.BufferKey}", queue.Message);
					return false;
				}

				return true;
			}
			catch (ValidationException ex)
			{
				if (q.HandlerInstance.ValidationFailed == Cdn.QueueValidationBehavior.Complete)
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

			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("QueueManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);

			MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"{typeof(QueueWorkerJob).FullName.PadRight(64)}| Error processing entry: {Serializer.Serialize(item)} => {ex}");

			_queueMonitoringService?.SignalError();
		}
	}
}
