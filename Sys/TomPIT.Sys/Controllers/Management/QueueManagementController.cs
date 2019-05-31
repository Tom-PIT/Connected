using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Storage;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class QueueManagementController : SysController
	{
		[HttpPost]
		public List<IQueueMessage> Dequeue()
		{
			var body = FromBody();

			var count = body.Required<int>("count");

			return DataModel.Queue.Dequeue(count);
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
