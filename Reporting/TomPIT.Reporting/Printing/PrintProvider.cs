using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
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

		public IPrintExportDescriptor Export(IPrintJob job)
		{
			var report = CreateReport(job);

			if (report == null)
				return null;

			using var ms = new MemoryStream();

			report.SaveLayoutToXml(ms);

			ms.Seek(0, SeekOrigin.Begin);

			return new PrintExportDescriptor
			{
				Content = ms.ToArray(),
				MimeType = "devexpress/report"
			};
		}

		public void Print(IPrintJob job)
		{
			var report = CreateReport(job);

			if (report == null)
				return;

			var print = new PrintToolBase(report.PrintingSystem);

			print.Print();
		}

		private XtraReport CreateReport(IPrintJob job)
		{
			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(job.Component) is IReportConfiguration descriptor))
				return null;

			var args = Serializer.Deserialize<JObject>(job.Arguments);
			var printer = Serializer.Deserialize<Printer>(args.Required<string>("printer"));
			var arguments = Serializer.Deserialize<JObject>(args.Optional("arguments", string.Empty));
			var report = new ReportRuntimeStorage().CreateReport(job.Component, arguments);

			report.PrinterName = printer.Name;
			report.CreateDocument();

			return report;
		}
	}
}
