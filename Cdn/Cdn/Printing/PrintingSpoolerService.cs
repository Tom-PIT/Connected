using System;
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
		protected override bool Initialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			IntervalTimeout = TimeSpan.FromSeconds(1);

			return true;
		}

		protected override Task Process(CancellationToken cancel)
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

			foreach (var job in jobs)
			{
				if (cancel.IsCancellationRequested)
					return Task.CompletedTask;

				var message = Serializer.Deserialize<JObject>(job.Message);
				var printer = message.Optional("printer", string.Empty);
				var id = message.Optional("id", Guid.Empty);

				if (string.IsNullOrWhiteSpace(printer) || id == Guid.Empty)
				{
					MiddlewareDescriptor.Current.Tenant.GetService<IPrintingSpoolerManagementService>().Complete(job.PopReceipt);
					continue;
				}

				PrintingHubs.Printing.Clients.Group(printer.ToLowerInvariant()).SendCoreAsync("print", new object[] { id }, cancel).Wait();
			}

			return Task.CompletedTask;
		}
	}
}
