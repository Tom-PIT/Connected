using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.Design;
using TomPIT.Reporting.Storage;

namespace TomPIT.Reporting.Design.Storage
{
	internal class ReportDesignerStorage : ReportRuntimeStorage
	{
		public override bool CanSetData(string url)
		{
			return IsValidUrl(url);
		}

		public override bool IsValidUrl(string url)
		{
			return SelectReport(url) != null;
		}

		public override Dictionary<string, string> GetUrls()
		{
			return new Dictionary<string, string>();
		}

		public override void SetData(XtraReport report, string url)
		{
			var rep = SelectReport(url);
			var connection = SysExtensions.CurrentConnection();

			using (var ms = new MemoryStream())
			{
				report.SaveLayoutToXml(ms);
				ms.Seek(0, SeekOrigin.Begin);

				connection.GetService<IComponentDevelopmentService>().Update(rep, Encoding.UTF8.GetString(ms.ToArray()));
			}
		}

		public override string SetNewData(XtraReport report, string defaultUrl)
		{
			// Stores the specified report using a new URL. 
			// The IsValidUrl and CanSetData methods are never called before this method. 
			// You can validate and correct the specified URL directly in the SetNewData method implementation 
			// and return the resulting URL used to save a report in your storage.
			SetData(report, defaultUrl);
			return defaultUrl;
		}
	}
}