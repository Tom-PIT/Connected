using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Handlers;
using TomPIT.ComponentModel.Workers;
using TomPIT.Services;
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

			var url = Instance.Connection.CreateUrl("QueueManagement", "Complete");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			Instance.Connection.Post(url, d);
		}

		private void Invoke(IQueueMessage queue, JObject data)
		{
			var component = data.Required<Guid>("component");
			var arguments = data.Optional<string>("arguments", null);
			var configuration = Instance.GetService<IComponentService>().SelectConfiguration(component) as IQueueHandlerConfiguration;

			if (configuration == null)
				Instance.Connection.LogError(nameof(QueueWorkerJob), nameof(Invoke), $"{SR.ErrQueueWorkerNotFound} ({component})");

			var ms = Instance.Connection.GetService<IMicroServiceService>().Select(((IConfiguration)configuration).MicroService(Instance.Connection));
			var ctx = TomPIT.Services.ExecutionContext.Create(Instance.Connection.Url, ms);
			var metricId = ctx.Services.Diagnostic.StartMetric(configuration.Metrics, null);

			try
			{
				var q = new Queue(arguments, configuration);

				q.Invoke();

				ctx.Services.Diagnostic.StopMetric(metricId, Diagnostics.SessionResult.Success, null);
			}
			catch
			{
				ctx.Services.Diagnostic.StopMetric(metricId, Diagnostics.SessionResult.Fail, null);

				throw;
			}
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Connection.LogError(nameof(QueueWorkerJob), ex.Source, ex.Message);

			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			var url = Instance.Connection.CreateUrl("QueueManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			Instance.Connection.Post(url, d);
		}
	}
}
