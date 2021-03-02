using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Storage;
using TomPIT.Sys.Model;
using TomPIT.SysDb.Events;

namespace TomPIT.Sys.Controllers.Management
{
	public class EventManagementController : SysController
	{
		[HttpPost]
		public ImmutableList<IQueueMessage> Dequeue()
		{
			var body = FromBody();
			var count = body.Required<int>("count");

			return DataModel.Events.Dequeue(count);
		}

		[HttpGet]
		public IEventDescriptor Select(Guid id)
		{
			return DataModel.Events.Select(id);
		}

		[HttpPost]
		public void Complete()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.Events.Complete(popReceipt);
		}

		[HttpPost]
		public void Ping()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");
			var nextVisible = body.Optional("nextVisible", TimeSpan.FromSeconds(5));

			DataModel.Events.Ping(popReceipt, nextVisible);
		}
	}
}
