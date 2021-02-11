using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.Cdn.Printing
{
	internal class PrintJob : DispatcherJob<IQueueMessage>
	{
		private TimeoutTask _timeout = null;
		public PrintJob(IDispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var message = Serializer.Deserialize<PrintQueueMessage>(item.Message);
			var job = MiddlewareDescriptor.Current.Tenant.GetService<IPrintingService>().Select(message.Id);

			if (job == null)
			{
				MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().Complete(item.PopReceipt);

				return;
			}

			_timeout = new TimeoutTask(() =>
			{
				MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().Ping(item.PopReceipt);

				return Task.CompletedTask;
			}, TimeSpan.FromMinutes(4), Cancel);

			_timeout.Start();

			try
			{
				Invoke(item, job);
				MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().Complete(item.PopReceipt);
			}
			finally
			{
				_timeout.Stop();
			}
		}

		private void Invoke(IQueueMessage message, IPrintJob job)
		{
			try
			{
				var provider = MiddlewareDescriptor.Current.Tenant.GetService<IDocumentService>().GetProvider(job.Provider);

				if (provider == null)
					throw new RuntimeException($"{SR.ErrPrintingProviderResolve} ({job.Provider})");

				if (Shell.GetService<IRuntimeService>().Platform == Platform.OnPrem)
					provider.Print(job);
				else
				{
					if (string.IsNullOrWhiteSpace(job.Arguments))
						return;

					var args = Serializer.Deserialize<JObject>(job.Arguments);
					var printer = Serializer.Deserialize<Printer>(args.Required<string>("printer"));

					if (printer == null)
						return;

					var report = provider.Create(job);

					if (report != null)
						MiddlewareDescriptor.Current.Tenant.GetService<IPrintingSpoolerManagementService>().Insert(report.MimeType, printer.Name, Convert.ToBase64String(report.Content), job.SerialNumber);
				}
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Cdn",
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = nameof(PrintJob),
					EventId = MiddlewareEvents.Printing
				});

				throw;
			}
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().Error(item.PopReceipt, ex.Message);
		}
	}
}
