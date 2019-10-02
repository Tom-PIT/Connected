using System;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Storage;
using TomPIT.Worker.Workers;

namespace TomPIT.Worker.Services
{
	internal class WorkerJob : DispatcherJob<IQueueMessage>
	{
		public WorkerJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;
			var ms = Invoke(item, m);

			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("WorkerManagement", "Complete");
			var d = new JObject
			{
				{"microService", ms },
				{"popReceipt", item.PopReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}

		private Guid Invoke(IQueueMessage queue, JObject data)
		{
			var worker = data.Required<Guid>("worker");
			var state = data.Optional("state", Guid.Empty);
			var configuration = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(worker) as IWorkerConfiguration;

			if (configuration == null)
				MiddlewareDescriptor.Current.Tenant.LogError(SR.LogCategoryWorker, nameof(Invoke), string.Format("{0} ({1})", SR.ErrWorkerNotFound, worker));

			var workerState = string.Empty;

			if (state != Guid.Empty)
			{
				var blob = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Download(state);

				if (blob != null)
					workerState = Encoding.UTF8.GetString(blob.Content);
			}

			Invoker i = null;
			var ctx = new MicroServiceContext(configuration.MicroService());
			var metricId = ctx.Services.Diagnostic.StartMetric(configuration.Metrics, null);

			try
			{
				if (configuration is IHostedWorkerConfiguration hc)
					i = new Hosted(hc, workerState);
				else
					throw new NotSupportedException();

				i.Invoke();

				if (state == Guid.Empty)
				{
					if (!string.IsNullOrWhiteSpace(i.State))
					{
						var id = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Upload(new Blob
						{
							ContentType = "application/json",
							FileName = worker.ToString(),
							MicroService = configuration.MicroService(),
							PrimaryKey = worker.ToString(),
							Type = BlobTypes.WorkerState
						}, Encoding.UTF8.GetBytes(i.State), StoragePolicy.Singleton);


						var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("WorkerManagement", "AttachState");
						var d = new JObject
						{
							{"worker", worker },
							{"state", id }
						};

						MiddlewareDescriptor.Current.Tenant.Post(url, d);
					}
				}
				else
				{
					MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Upload(new Blob
					{
						ContentType = "application/json",
						FileName = worker.ToString(),
						MicroService = configuration.MicroService(),
						PrimaryKey = worker.ToString(),
						Type = BlobTypes.WorkerState
					}, Encoding.UTF8.GetBytes(i.State), StoragePolicy.Singleton);
				}

				ctx.Services.Diagnostic.StopMetric(metricId, Diagnostics.SessionResult.Success, null);
			}
			catch
			{
				ctx.Services.Diagnostic.StopMetric(metricId, Diagnostics.SessionResult.Fail, null);

				throw;
			}

			return configuration.MicroService();
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(nameof(WorkerJob), ex.Source, ex.Message);

			var m = JsonConvert.DeserializeObject(item.Message) as JObject;
			var worker = m.Required<Guid>("worker");

			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(worker) is IWorkerConfiguration configuration))
				return;

			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("WorkerManagement", "Error");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt },
				{"microService", configuration.MicroService() }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}
	}
}
