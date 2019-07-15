using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TomPIT.Sys.Controllers.Management
{
	public class SearchManagementController : SysController
	{
		[HttpPost]
		public List<IQueueMessage> Dequeue()
		{
			var body = FromBody();

			var count = body.Required<int>("count");

			var r = new List<IQueueMessage>();

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

			DataModel.Events.Ping(popReceipt);
		}
	}
}
