using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Storage;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Management
{
	public class PrintingManagementController : SysController
	{
		[HttpPost]
		public List<IQueueMessage> Dequeue()
		{
			var body = FromBody();

			var count = body.Required<int>("count");

			return DataModel.Printing.Dequeue(count);
		}

		[HttpPost]
		public void Ping()
		{
			var body = FromBody();

			var popReceipt = body.Required<Guid>("popReceipt");
			var nextVisible = body.Optional("nextVisible", TimeSpan.FromMinutes(4));

			DataModel.Printing.Ping(popReceipt, nextVisible);
		}

		[HttpPost]
		public void Complete()
		{
			var body = FromBody();

			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.Printing.Complete(popReceipt);
		}

		[HttpPost]
		public void Error()
		{
			var body = FromBody();

			var popReceipt = body.Required<Guid>("popReceipt");
			var error = body.Optional<string>("error", null);

			DataModel.Printing.Error(popReceipt, error);
		}
	}
}
