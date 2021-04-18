using System;
using TomPIT.Cdn;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewarePrinting
	{
		[Obsolete("Use Print method instead.")]
		Guid PrintReport([CIP(CIP.ReportProvider)]string report, IPrinter printer);
		[Obsolete("Use Print method instead.")]
		Guid PrintReport([CIP(CIP.ReportProvider)]string report, IPrinter printer, object arguments);
		[Obsolete("Use Print method instead.")]
		Guid PrintReport([CIP(CIP.ReportProvider)]string report, IPrinter printer, object arguments, string provider);
		Guid Print(MiddlewarePrintArgs e);

		IPrintJob SelectPrintJob(Guid job);
	}
}
