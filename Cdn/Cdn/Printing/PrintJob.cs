using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.Cdn.Printing
{
	internal class PrintJob : DispatcherJob<IQueueMessage>
	{
		private TimeoutTask _timeout = null;
		public PrintJob(Dispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
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
				var provider = MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().GetProvider(job.Provider);

				if (provider == null)
					throw new RuntimeException($"{SR.ErrPrintingProviderResolve} ({job.Provider})");

				provider.Print(job);
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

				throw ex;
			}
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().Error(item.PopReceipt, ex.Message);
		}
	}
}
