using System;
using TomPIT.Cdn;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewarePrinting
	{
		Guid PrintReport([CIP(CIP.ReportProvider)]string report, IPrinter printer);
		Guid PrintReport([CIP(CIP.ReportProvider)]string report, IPrinter printer, object arguments);
		Guid PrintReport([CIP(CIP.ReportProvider)]string report, IPrinter printer, object arguments, string provider);

		IPrintJob SelectPrintJob(Guid job);
	}
}
