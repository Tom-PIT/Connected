using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Reporting.Storage
{
	public class ReportRuntimeStorage : ReportStorageWebExtension
	{
		public override bool CanSetData(string url)
		{
			return false;
		}

		public override bool IsValidUrl(string url)
		{
			return false;
		}

		protected IReportConfiguration SelectReport(string url)
		{
			var tenant = MiddlewareDescriptor.Current.Tenant;

			if (tenant == null)
				return null;

			var tokens = url.Split('/');

			if (tokens.Length != 2)
				return null;

			var ms = tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return null;

			var subTokens = tokens[1].Split('?');

			return tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, "Report", subTokens[0]) as IReportConfiguration;
		}
		public override byte[] GetData(string url)
		{
			var config = SelectReport(url);

			if (config == null)
				return null;

			XtraReport r = null;

			if (config.TextBlob != Guid.Empty)
			{
				var tenant = MiddlewareDescriptor.Current.Tenant;
				var content = tenant.GetService<IComponentService>().SelectText(((IConfiguration)config).MicroService(), config);

				if (Shell.GetService<IRuntimeService>().Mode == EnvironmentMode.Design)
				{
					if (!string.IsNullOrWhiteSpace(content))
						return Encoding.UTF8.GetBytes(content);
				}
				else
				{
					r = new XtraReport();

					using var loadStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
				
					r.LoadLayoutFromXml(loadStream);
				}
			}

			if (r == null)
				r = new XtraReport();

			using var stream = new MemoryStream();

            r.Name = url;
			
            r.SaveLayoutToXml(stream);
			
			return stream.ToArray();
		}

		public XtraReport CreateReport(string url)
		{
			var report = SelectReport(url);
			JObject arguments = null;

			if (url.Contains("?"))
				arguments = ReportCreateSession.ParseArguments(url.Split("?")[^1]);

			return new ReportCreateSession
			{
				Component = report.Component,
				Arguments = arguments
			}.CreateReport();
		}

		public override Dictionary<string, string> GetUrls()
		{
			return new Dictionary<string, string>();
		}

		public override void SetData(XtraReport report, string url)
		{
		}

		public override string SetNewData(XtraReport report, string defaultUrl)
		{
			return defaultUrl;
		}
	}
}