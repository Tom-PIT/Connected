﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Workers;
using TomPIT.Services;
using TomPIT.Storage;
using TomPIT.Worker.Storage;
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

			var url = Instance.Connection.CreateUrl("WorkerManagement", "Complete");
			var d = new JObject
			{
				{"microService", ms },
				{"popReceipt", item.PopReceipt }
			};

			Instance.Connection.Post(url, d);
		}

		private Guid Invoke(IQueueMessage queue, JObject data)
		{
			var worker = data.Required<Guid>("worker");
			var state = data.Optional("state", Guid.Empty);
			var configuration = Instance.GetService<IComponentService>().SelectConfiguration(worker) as IWorker;

			if (configuration == null)
				Instance.Connection.LogError(SR.LogCategoryWorker, nameof(Invoke), string.Format("{0} ({1})", SR.ErrWorkerNotFound, worker));

			JObject workerState = null;

			if (state != Guid.Empty)
			{
				var blob = Instance.GetService<IStorageService>().Download(state);

				if (blob != null)
					workerState = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(blob.Content)) as JObject;
			}

			if (workerState == null)
				workerState = new JObject();

			Invoker i = null;
			var ctx = TomPIT.Services.ExecutionContext.NonHttpContext(Instance.Connection.Url, Instance.Connection.GetService<IMicroServiceService>().Select(configuration.MicroService(Instance.Connection)), null);
			var args = new WorkerInvokeArgs(ctx, workerState);

			if (configuration is IHostedWorker)
				i = new Hosted(args, configuration as IHostedWorker);
			else if (configuration is ICollector)
				i = new Collector(args, configuration as ICollector);
			else
				throw new NotSupportedException();

			i.Invoke();

			if (state == Guid.Empty)
			{
				if (workerState.Count > 0)
				{
					var id = Instance.GetService<IStorageService>().Upload(new Blob
					{
						ContentType = "application/json",
						FileName = worker.ToString(),
						MicroService = configuration.MicroService(Instance.Connection),
						PrimaryKey = worker.ToString(),
						Type = BlobTypes.WorkerState
					}, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(workerState)), StoragePolicy.Singleton);


					var url = Instance.Connection.CreateUrl("WorkerManagement", "AttachState");
					var d = new JObject
						{
							{"worker", worker },
							{"state", id }
						};

					Instance.Connection.Post(url, d);
				}
			}
			else
			{
				Instance.GetService<IStorageService>().Upload(new Blob
				{
					ContentType = "application/json",
					FileName = worker.ToString(),
					MicroService = configuration.MicroService(Instance.Connection),
					PrimaryKey = worker.ToString(),
					Type = BlobTypes.WorkerState
				}, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(workerState)), StoragePolicy.Singleton);
			}

			return configuration.MicroService(Instance.Connection);
		}
	}
}