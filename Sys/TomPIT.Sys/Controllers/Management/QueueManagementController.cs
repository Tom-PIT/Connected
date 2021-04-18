using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
	public class QueueManagementController : SysController
	{
		[HttpPost]
		public ImmutableList<IQueueMessage> Dequeue()
		{
			var body = FromBody();

			var count = body.Required<int>("count");

			return DataModel.Queue.Dequeue(count, TimeSpan.FromMinutes(5), QueueScope.Content, null);
		}

		[HttpPost]
		public void Ping()
		{
			var body = FromBody();

			var popReceipt = body.Required<Guid>("popReceipt");
			var nextVisible = body.Optional("nextVisible", TimeSpan.FromMinutes(2));

			DataModel.Queue.Ping(popReceipt, nextVisible);
		}

		[HttpPost]
		public void Complete()
		{
			var body = FromBody();

			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.Queue.Complete(popReceipt);
		}
	}
}
