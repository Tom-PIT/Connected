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
		public IndexingJob(IDispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		private IQueueMessage Message { get; set; }

		protected override void DoWork(IQueueMessage item)
		{
			Message = item;

			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			_timeout = new TimeoutTask(() =>
			{
				MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Ping(Message.PopReceipt, 300);
				return Task.CompletedTask;
			}, TimeSpan.FromMinutes(4), Cancel);

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
			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("SearchManagement", "Select")
				.AddParameter("id", id);

			var request = MiddlewareDescriptor.Current.Tenant.Get<IndexRequest>(url);

			if (request == null)
				return;

			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(request.MicroService, "SearchCatalog", request.Catalog) is ISearchCatalogConfiguration configuration))
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
				MiddlewareDescriptor.Current.Tenant.LogError(nameof(IndexingJob), ex.Message, "Search");
				return;
			}

			indexer.Index(Cancel);

			if (!indexer.Success)
				MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Ping(queue.PopReceipt, 15);
		}


		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, nameof(IndexingJob));

			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("SearchManagement", "Ping");
			var d = new JObject
				{
					{"popReceipt", item.PopReceipt },
					{"nextVisible", 30 }
				};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}
	}
}