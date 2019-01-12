using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class WorkerJob : DispatcherJob<IQueueMessage>, IApiExecutionScope
	{
		private IApi _api = null;

		public WorkerJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		public IApi Api => _api;

		protected override void DoWork(IQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(item, m);

			var url = Instance.Connection.CreateUrl("WorkerManagement", "Complete");
			var d = new JObject
			{
				{"microService", m.Required<Guid>("service") },
				{"popReceipt", item.PopReceipt }
			};

			Instance.Connection.Post(url, d);
		}

		private void Invoke(IQueueMessage queue, JObject data)
		{
			var service = data.Required<Guid>("service");
			var api = data.Required<Guid>("api");
			var operation = data.Required<Guid>("operation");

			var ctx = TomPIT.Services.ExecutionContext.NonHttpContext(Instance.Connection.Url, "Worker", api.AsString(), service.AsString(), string.Empty);

			var configuration = Instance.GetService<IComponentService>().SelectConfiguration(api) as IApi;

			if (configuration == null)
				Instance.Connection.LogError(ctx, SR.LogCategoryWorker, nameof(Invoke), string.Format("{0} ({1}.{2}.{3})", SR.ErrWorkerNotFound, service, api, operation));

			_api = configuration;

			var op = configuration.Operations.FirstOrDefault(f => f.Id == operation);

			ctx.Invoke(configuration.ComponentName(ctx));
		}
	}
}
