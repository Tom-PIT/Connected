using DevExpress.XtraPrinting;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.MicroServices.Reporting.Storage;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.MicroServices.Reporting.Printing
{
	internal class PrintProvider : IPrintingProvider
	{
		public string Name => "DevEx";
		public void Print(IPrintJob job)
		{
			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(job.Component) is IReportConfiguration descriptor))
				return;

			var args = Serializer.Deserialize<JObject>(job.Arguments);
			var printer = Serializer.Deserialize<Printer>(args.Required<string>("printer"));
			var arguments = Serializer.Deserialize<JObject>(args.Optional("arguments", string.Empty));
			var report = new ReportRuntimeStorage().CreateReport(job.Component, arguments);

			report.PrinterName = printer.Name;
			report.CreateDocument();

			var print = new PrintToolBase(report.PrintingSystem);

			print.Print();
		}
	}
}
