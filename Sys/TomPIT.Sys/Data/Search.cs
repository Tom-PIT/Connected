﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Search;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Data
{
	internal class Search
	{
		private const string Queue = "search";

		public Guid Insert(Guid microService, string name, string e)
		{
			var id = Guid.NewGuid();
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var message = new JObject
			{
				{ "id",id}
			};

			Shell.GetService<IDatabaseService>().Proxy.Search.Insert(ms, name, id, DateTime.UtcNow, e);
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(Queue, JsonConvert.SerializeObject(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

			return id;
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.DequeueSystem(Queue, count, TimeSpan.FromMinutes(5));
		}

		public IIndexRequest Select(Guid id)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Search.Select(id);
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(popReceipt);

			if (m == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(popReceipt);

			if (m == null)
				return;

			var e = Resolve(m);

			if (e != null)
				Shell.GetService<IDatabaseService>().Proxy.Search.Delete(e);

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(popReceipt);
		}

		private IIndexRequest Resolve(IQueueMessage message)
		{
			var d = JsonConvert.DeserializeObject(message.Message) as JObject;

			var id = d.Required<Guid>("id");

			return Shell.GetService<IDatabaseService>().Proxy.Search.Select(id);
		}

		public ICatalogState SelectState(Guid catalog)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Search.SelectState(catalog);
		}

		public void UpdateState(Guid catalog, CatalogStateStatus status)
		{
			var state = SelectState(catalog);

			if (state == null)
				throw new SysException(SR.ErrSearchCatalogStateNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Search.UpdateState(catalog, status);
		}

		public void DeleteState(Guid catalog)
		{
			var state = SelectState(catalog);

			if (state == null)
				throw new SysException(SR.ErrSearchCatalogStateNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Search.DeleteState(catalog);
		}

		public void InvalidateState(Guid catalog)
		{
			Shell.GetService<IDatabaseService>().Proxy.Search.InvalidateState(catalog);
		}
	}
}