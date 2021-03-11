using System;
using TomPIT.Cdn;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public class MiddlewarePrintArgs : EventArgs
	{
		[CIP(CIP.ReportProvider)]
		public string Report { get; set; }
		public IPrinter Printer { get; set; }
		public object Arguments { get; set; }
		public string Provider { get; set; }
		public string User { get; set; }
		public string Category { get; set; }
	}
}
