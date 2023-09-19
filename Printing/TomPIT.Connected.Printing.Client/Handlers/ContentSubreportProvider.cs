using DevExpress.XtraReports.Services;
using DevExpress.XtraReports.UI;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TomPIT.Connected.Printing.Client.Printing;

namespace TomPIT.Connected.Printing.Client.Handlers
{
	internal class ContentSubreportProvider : IReportProviderAsync
	{
		private string _content;
		Dictionary<string, MemoryStream> _subreports = new Dictionary<string, MemoryStream>();

		public ContentSubreportProvider(string completeContent)
		{
			_content = completeContent;
			_subreports = ParseSubreports(completeContent);
		}

		public async Task<XtraReport> GetReportAsync(string reportUrl, ReportProviderContext context)
		{
			if (_subreports.GetValueOrDefault(reportUrl, null) is not MemoryStream ms)
				return null;

			try
			{
				//ParseXMLHere
				var report = new XtraReport();
				report.LoadLayoutFromXml(ms);
				report.CreateDocument();

				return report;
			}
			catch (Exception ex)
			{
				Logging.Exception(ex, LoggingLevel.Fatal);
				throw;
			}
		}

		private static Dictionary<string, MemoryStream> ParseSubreports(string content)
		{
			var reports = content.Split("<?xml version=\"1.0\" encoding=\"utf-8\"?>", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			var results = new Dictionary<string, MemoryStream>();

			foreach (var report in reports)
			{
				var parsedReport = XmlReader.Create(new StringReader(report));
				if (!parsedReport.Read())
					continue;

				var reportName = parsedReport.GetAttribute("Name");
				results.Add(reportName, new MemoryStream(Encoding.UTF8.GetBytes(report)));
			}

			return results;
		}
	}
}
