using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Cdn.Printing
{
	internal class PrintingSpoolerService : HostedService
	{
		protected override bool OnInitialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			IntervalTimeout = TimeSpan.FromSeconds(1);

			return true;
		}

		protected override Task OnExecute(CancellationToken cancel)
		{
			if (PrintingHubs.Printing == null)
			{
				IntervalTimeout = TimeSpan.FromSeconds(15);
				return Task.CompletedTask;
			}

			if (IntervalTimeout > TimeSpan.FromSeconds(1))
				IntervalTimeout = TimeSpan.FromSeconds(1);

			var jobs = MiddlewareDescriptor.Current.Tenant.GetService<IPrintingSpoolerManagementService>().Dequeue(128);

			if (cancel.IsCancellationRequested)
				return Task.CompletedTask;

			if (jobs == null)
				return Task.CompletedTask;

			var queue = new List<PrintNotificationDescriptor>();

			foreach (var job in jobs)
			{
				if (cancel.IsCancellationRequested)
					return Task.CompletedTask;

				var message = Serializer.Deserialize<JObject>(job.Message);
				var printer = message.Optional("printer", string.Empty);
				var id = message.Optional("id", Guid.Empty);
				var serialNumber = message.Optional("serialNumber", 0L);

				if (string.IsNullOrWhiteSpace(printer) || id == Guid.Empty)
				{
					MiddlewareDescriptor.Current.Tenant.GetService<IPrintingSpoolerManagementService>().Complete(job.PopReceipt);
					continue;
				}

				queue.Add(new PrintNotificationDescriptor
				{
					Id = id,
					PopReceipt = job.PopReceipt,
					SerialNumber = serialNumber,
					Printer = printer.ToLowerInvariant()
				});
			}

			if(queue.Count > 0)
			{
				var ordered = queue.OrderBy(f => f.SerialNumber);

				foreach(var job in ordered)
					PrintingHubs.Printing.Clients.Group(job.Printer).SendCoreAsync("print", new object[] { job.Id, job.PopReceipt }, cancel).Wait();
			}

			return Task.CompletedTask;
		}
	}
}
