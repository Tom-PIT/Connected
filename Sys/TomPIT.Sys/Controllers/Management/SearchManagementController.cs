using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Search;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
	public class SearchManagementController : SysController
	{
		[HttpPost]
		public ImmutableList<IQueueMessage> Dequeue()
		{
			var body = FromBody();
			var count = body.Required<int>("count");

			return DataModel.Search.Dequeue(count);
		}

		[HttpGet]
		public IIndexRequest Select(Guid id)
		{
			return DataModel.Search.Select(id);
		}

		[HttpPost]
		public void Complete()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.Search.Complete(popReceipt);
		}

		[HttpPost]
		public void Ping()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");
			var nextVisible = TimeSpan.FromSeconds(body.Required<int>("nextVisible"));

			DataModel.Search.Ping(popReceipt, nextVisible);
		}

		[HttpPost]
		public ICatalogState SelectState()
		{
			var body = FromBody();
			var catalog = body.Required<Guid>("catalog");

			return DataModel.Search.SelectState(catalog);
		}

		[HttpPost]
		public void UpdateState()
		{
			var body = FromBody();
			var catalog = body.Required<Guid>("catalog");
			var status = body.Required<CatalogStateStatus>("status");

			DataModel.Search.UpdateState(catalog, status);
		}

		[HttpPost]
		public void DeleteState()
		{
			var body = FromBody();
			var catalog = body.Required<Guid>("catalog");

			DataModel.Search.DeleteState(catalog);
		}

		[HttpPost]
		public void InvalidateState()
		{
			var body = FromBody();
			var catalog = body.Required<Guid>("catalog");

			DataModel.Search.InvalidateState(catalog);
		}
	}
}
