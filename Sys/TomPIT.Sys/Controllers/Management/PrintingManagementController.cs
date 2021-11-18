using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Cdn;
using TomPIT.Storage;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers.Management
{
	public class PrintingManagementController : SysController
	{
		[HttpPost]
		public ImmutableList<IQueueMessage> Dequeue()
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

		[HttpPost]
		public ImmutableList<IQueueMessage> DequeueSpooler()
		{
			var body = FromBody();

			var count = body.Required<int>("count");

			return DataModel.PrintingSpooler.Dequeue(count);
		}

		[HttpPost]
		public void PingSpooler()
		{
			var body = FromBody();

			var popReceipt = body.Required<Guid>("popReceipt");
			var nextVisible = body.Optional("nextVisible", TimeSpan.FromMinutes(4));

			DataModel.PrintingSpooler.Ping(popReceipt, nextVisible);
		}

		[HttpPost]
		public void CompleteSpooler()
		{
			var body = FromBody();

			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.PrintingSpooler.Complete(popReceipt);
		}
		[HttpPost]
		public Guid InsertSpooler()
		{
			var body = FromBody();
			var content = body.Required<string>("content");
			var printer = body.Required<string>("printer");
			var mime = body.Required<string>("mime");
			var identity = body.Optional<Guid>("identity", default);
			var serialNumber = body.Optional("serialNumber", 0L);

			return DataModel.PrintingSpooler.Insert(mime, printer, content, serialNumber, identity);
		}

		[HttpPost]
		public IPrintSpoolerJob SelectSpooler()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.PrintingSpooler.Select(token);
		}

		[HttpPost]
		public void DeleteSpooler()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			DataModel.PrintingSpooler.Delete(token);
		}
	}
}
