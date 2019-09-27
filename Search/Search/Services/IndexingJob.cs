using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Search.Indexing;
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.Search.Services
{
	internal class IndexingJob : DispatcherJob<IQueueMessage>
	{
		private TimeoutTask _timeout = null;
		public IndexingJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		private IQueueMessage Message { get; set; }

		protected override void DoWork(IQueueMessage item)
		{
			Message = item;

			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			_timeout = new TimeoutTask(() =>
			{
				Instance.Tenant.GetService<IIndexingService>().Ping(Message.PopReceipt, 300);
				return Task.CompletedTask;
			}, TimeSpan.FromMinutes(4));

			_timeout.Start();

			try
			{
				Invoke(item, m);
			}
			finally
			{
				_timeout.Stop();
			}
		}

		private void Invoke(IQueueMessage queue, JObject data)
		{
			var id = data.Required<Guid>("id");
			var url = Instance.Tenant.CreateUrl("SearchManagement", "Select")
				.AddParameter("id", id);

			var request = Instance.Tenant.Get<IndexRequest>(url);

			if (request == null)
				return;

			if (!(Instance.Tenant.GetService<IComponentService>().SelectConfiguration(request.MicroService, "SearchCatalog", request.Catalog) is ISearchCatalogConfiguration configuration))
				return;

			Indexer indexer = null;

			try
			{
				var arguments = Serializer.Deserialize<JObject>(request.Arguments);
				var verb = arguments.Required<SearchVerb>("verb");
				var args = arguments.Optional("arguments", string.Empty);

				indexer = new Indexer(configuration, queue, request, verb, args);
			}
			catch (Exception ex)
			{
				Instance.Tenant.LogError("Search", nameof(IndexingJob), ex.Message);
				return;
			}

			indexer.Index();

			if (!indexer.Success)
				Instance.Tenant.GetService<IIndexingService>().Ping(queue.PopReceipt, 15);
		}


		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Tenant.LogError(nameof(IndexingJob), ex.Source, ex.Message);

			var url = Instance.Tenant.CreateUrl("SearchManagement", "Ping");
			var d = new JObject
				{
					{"popReceipt", item.PopReceipt },
					{"nextVisible", 30 }
				};

			Instance.Tenant.Post(url, d);
		}
	}
}