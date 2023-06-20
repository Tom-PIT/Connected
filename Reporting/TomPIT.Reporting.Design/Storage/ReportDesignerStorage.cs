using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DevExpress.XtraReports.UI;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Environment;
using TomPIT.MicroServices.Reporting.Storage;
using TomPIT.Middleware;

namespace TomPIT.MicroServices.Reporting.Design.Storage
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
			var reports = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().QueryComponents(ComponentCategories.Report);
			var r = new Dictionary<string, string>();

			foreach (var report in reports)
			{
				var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(report.MicroService);

				r.Add($"{ms.Name}/{report.Name}", $"{ms.Name}/{report.Name}");
//                r.Add($"{ms.Name}/{report.Name}", $"{report.Name} ({ms.Name})");

            }

            return r;
		}

		public override void SetData(XtraReport report, string url)
		{
			var rep = SelectReport(url);
			var tenant = MiddlewareDescriptor.Current.Tenant;

			using (var ms = new MemoryStream())
			{
				report.SaveLayoutToXml(ms);
				ms.Seek(0, SeekOrigin.Begin);

				tenant.GetService<IDesignService>().Components.Update(rep, Encoding.UTF8.GetString(ms.ToArray()));
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