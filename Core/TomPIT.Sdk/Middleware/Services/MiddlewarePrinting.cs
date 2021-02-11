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
			return Print(new MiddlewarePrintArgs
			{
				Report = report,
				Printer = printer,
				Arguments = arguments,
				Provider = provider
			});
		}

		public Guid Print(MiddlewarePrintArgs e)
		{
			var descriptor = ComponentDescriptor.Report(Context, e.Report);

			descriptor.Validate();

			if (string.IsNullOrWhiteSpace(e.User))
			{
				if (Context.Services.Identity.IsAuthenticated)
					e.User = Context.Services.Identity.User.Token.ToString();
			}

			return Context.Tenant.GetService<IPrintingService>().Insert(e.Provider, e.Printer, descriptor.Component.Token, e.Arguments, e.User, e.Category);
		}
		public IPrintJob SelectPrintJob(Guid job)
		{
			return Context.Tenant.GetService<IPrintingService>().Select(job);
		}
	}
}
