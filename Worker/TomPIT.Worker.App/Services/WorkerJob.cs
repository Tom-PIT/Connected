using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Storage;
using TomPIT.Worker.Workers;

namespace TomPIT.Worker.Services
{
	internal class WorkerJob : DispatcherJob<IQueueMessage>
	{
		private TimeoutTask _timeout = null;

		public WorkerJob(IDispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		private JObject Message { get; set; }
		private Guid Worker { get; set; }
		private IWorkerConfiguration Configuration { get; set; }
		protected override void DoWork(IQueueMessage item)
		{
			if (!Initialize(item))
			{
				if (Configuration != null)
					Proxy.Complete(Configuration.MicroService(), item.PopReceipt);

				return;
			}

			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			_timeout = new TimeoutTask(() =>
			{
				Proxy.Ping(Configuration.MicroService(), item.PopReceipt);

				return Task.CompletedTask;
			}, TimeSpan.FromSeconds(45), Cancel);


			_timeout.Start();
			Guid ms;

			try
			{
				ms = Invoke(item, m);
			}
			finally
			{
				_timeout.Stop();
				_timeout = null;
			}

			Proxy.Complete(Configuration.MicroService(), item.PopReceipt);
		}

		private bool Initialize(IQueueMessage message)
		{
			try
			{
				Message = JsonConvert.DeserializeObject(message.Message) as JObject;
				Worker = Message.Required<Guid>("worker");

				if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(Worker) is IWorkerConfiguration configuration))
					return false;

				Configuration = configuration;

				return true;
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError(nameof(Invoke), ex.Message, LogCategories.Worker);

				return false;
			}
		}

		private Guid Invoke(IQueueMessage queue, JObject data)
		{
			var state = Message.Optional("state", Guid.Empty);
			var workerState = string.Empty;

			if (state != Guid.Empty)
			{
				var blob = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Download(state);

				if (blob != null)
					workerState = Encoding.UTF8.GetString(blob.Content);
			}

			Invoker i = null;
			using var ctx = new MicroServiceContext(Configuration.MicroService());
			var metricId = ctx.Services.Diagnostic.StartMetric(Configuration.Metrics, null);

			try
			{
				if (Configuration is IHostedWorkerConfiguration hc)
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
							FileName = Worker.ToString(),
							MicroService = Configuration.MicroService(),
							PrimaryKey = Worker.ToString(),
							Type = BlobTypes.WorkerState
						}, Encoding.UTF8.GetBytes(i.State), StoragePolicy.Singleton);

						Proxy.AttachState(Worker, id);
					}
				}
				else
				{
					MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Upload(new Blob
					{
						ContentType = "application/json",
						FileName = Worker.ToString(),
						MicroService = Configuration.MicroService(),
						PrimaryKey = Worker.ToString(),
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

			return Configuration.MicroService();
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, nameof(WorkerJob));

			if (Configuration != null)
				Proxy.Error(Configuration.MicroService(), item.PopReceipt);
		}

		private IWorkerProxyService Proxy => MiddlewareDescriptor.Current.Tenant.GetService<IWorkerProxyService>();

		protected override void OnDisposing()
		{
			if (_timeout != null)
			{
				_timeout.Stop();
				_timeout = null;
			}
		}
	}
}
