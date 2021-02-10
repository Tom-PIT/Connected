using System;
using TomPIT.Cdn;
using TomPIT.ComponentModel;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewarePrinting : MiddlewareObject, IMiddlewarePrinting
	{
		public Guid PrintReport(string report, IPrinter printer)
		{
			return Print(report, printer, null);
		}

		public Guid PrintReport(string report, IPrinter printer, object arguments)
		{
			return Print(report, printer, arguments, null);
		}

		public Guid PrintReport(string report, IPrinter printer, object arguments, string provider)
		{
			return Print(report, printer, arguments, provider);
		}

		public Guid Print(string report, IPrinter printer)
		{
			return PrintReport(report, printer, null);
		}

		public Guid Print(string report, IPrinter printer, object arguments)
		{
			return PrintReport(report, printer, arguments, null);
		}

		public Guid Print(string report, IPrinter printer, object arguments, string provider)
		{
			return Print(report, printer, arguments, provider, null);
		}

		public Guid Print(string report, IPrinter printer, object arguments, string provider, string user)
		{
			var descriptor = ComponentDescriptor.Report(Context, report);

			descriptor.Validate();

			if (string.IsNullOrWhiteSpace(user))
			{
				if (Context.Services.Identity.IsAuthenticated)
					user = Context.Services.Identity.User.Token.ToString();
			}

			return Context.Tenant.GetService<IPrintingService>().Insert(provider, printer, descriptor.Component.Token, arguments, user);
		}
		public IPrintJob SelectPrintJob(Guid job)
		{
			return Context.Tenant.GetService<IPrintingService>().Select(job);
		}
	}
}
