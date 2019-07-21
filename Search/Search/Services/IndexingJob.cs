using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Data;
using TomPIT.Search.Indexing;
using TomPIT.Services;
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

			_timeout = new TimeoutTask(()=>
			{
				Instance.GetService<IIndexingService>().Ping(Message.PopReceipt, 300);
				return Task.CompletedTask;
			},TimeSpan.FromMinutes(4));

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
			var url = Instance.Connection.CreateUrl("SearchManagement", "Select")
				.AddParameter("id", id);

			var request = Instance.Connection.Get<IndexRequest>(url);

			if (request == null)
				return;

			if (!(Instance.GetService<IComponentService>().SelectConfiguration(request.MicroService, "SearchCatalog", request.Catalog) is ISearchCatalog configuration))
				return;

			Indexer indexer = null;

			try
			{
				var arguments = Types.Deserialize<JObject>(request.Arguments);
				var verb = arguments.Required<SearchVerb>("verb");
				var args = arguments.Optional("arguments", string.Empty);

				indexer = new Indexer(configuration, queue, request, verb, args);
			}
			catch(Exception ex)
			{
				Instance.Connection.LogError("Search", nameof(IndexingJob), ex.Message);
				return;
			}

			indexer.Index();

			if (!indexer.Success)
				Instance.GetService<IIndexingService>().Ping(queue.PopReceipt, 15);
		}


		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Connection.LogError(nameof(IndexingJob), ex.Source, ex.Message);

			var url = Instance.Connection.CreateUrl("SearchManagement", "Ping");
			var d = new JObject
				{
					{"popReceipt", item.PopReceipt },
					{"nextVisible", 30 }
				};

			Instance.Connection.Post(url, d);
		}
	}
}