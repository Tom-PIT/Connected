using System;
using TomPIT.Cdn;
using TomPIT.ComponentModel;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewarePrinting : MiddlewareObject, IMiddlewarePrinting
	{
		public Guid PrintReport(string report, IPrinter printer)
		{
			return PrintReport(report, printer, null);
		}

		public Guid PrintReport(string report, IPrinter printer, object arguments)
		{
			return PrintReport(report, printer, arguments, null);
		}

		public Guid PrintReport(string report, IPrinter printer, object arguments, string provider)
		{
			var descriptor = ComponentDescriptor.Report(Context, report);

			descriptor.Validate();

			return Context.Tenant.GetService<IPrintingService>().Insert(provider, printer, descriptor.Component.Token, arguments);
		}

		public IPrintJob SelectPrintJob(Guid job)
		{
			return Context.Tenant.GetService<IPrintingService>().Select(job);
		}
	}
}
