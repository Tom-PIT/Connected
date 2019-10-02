using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Storage;
using TomPIT.Worker.Workers;

namespace TomPIT.Worker.Services
{
	public class QueueWorkerJob : DispatcherJob<IQueueMessage>
	{
		public QueueWorkerJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(item, m);

			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("QueueManagement", "Complete");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}

		private void Invoke(IQueueMessage queue, JObject data)
		{
			var component = data.Required<Guid>("component");
			var arguments = data.Optional<string>("arguments", null);
			var configuration = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(component) as IQueueConfiguration;

			if (configuration == null)
				MiddlewareDescriptor.Current.Tenant.LogError(nameof(QueueWorkerJob), nameof(Invoke), $"{SR.ErrQueueWorkerNotFound} ({component})");

			var ctx = new MicroServiceContext(configuration.MicroService());
			var metricId = ctx.Services.Diagnostic.StartMetric(configuration.Metrics, null);
			Queue q = null;

			try
			{
				q = new Queue(arguments, configuration);

				q.Invoke();

				ctx.Services.Diagnostic.StopMetric(metricId, Diagnostics.SessionResult.Success, null);
			}
			catch (ValidationException ex)
			{
				if (q.HandlerInstance.ValidationFailed == Cdn.QueueValidationBehavior.Complete)
				{
					MiddlewareDescriptor.Current.Tenant.LogWarning(ex.Source, ex.Message, LogCategories.Worker);
					return;
				}
				else
					throw ex;
			}
			catch
			{
				ctx.Services.Diagnostic.StopMetric(metricId, Diagnostics.SessionResult.Fail, null);

				throw;
			}
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(nameof(QueueWorkerJob), ex.Source, ex.Message);

			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("QueueManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}
	}
}
