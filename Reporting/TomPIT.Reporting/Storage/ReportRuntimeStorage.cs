using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DevExpress.DataAccess.Json;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Serialization;

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
				var ms = tenant.GetService<IMicroServiceService>().Select(((IConfiguration)config).MicroService());

				if (Shell.GetService<IRuntimeService>().Mode == EnvironmentMode.Design)
				{
					if (!string.IsNullOrWhiteSpace(content))
						return Encoding.UTF8.GetBytes(content);
				}
				else
				{
					r = new XtraReport();

					using (var loadStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
					{
						r.LoadLayoutFromXml(loadStream);
					}

					BindDataSources(ms, r, url);
				}
			}

			if (r == null)
				r = new XtraReport();

			using (MemoryStream stream = new MemoryStream())
			{
				r.RequestParameters = false;

				foreach (var parameter in r.Parameters)
					parameter.Visible = false;

				r.SaveLayoutToXml(stream);

				return stream.ToArray();
			}
		}

		public XtraReport CreateReport(Guid component, object arguments)
		{
			var config = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(component) as IReportConfiguration;

			if (config == null)
				return null;

			var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(config.MicroService());

			XtraReport r = null;
			var url = $"{ms.Name}/{config.ComponentName()}";

			if (arguments != null)
				url += $"?{Convert.ToBase64String(Encoding.UTF8.GetBytes(Serializer.Serialize(arguments)))}";

			if (config.TextBlob != Guid.Empty)
			{
				var tenant = MiddlewareDescriptor.Current.Tenant;
				var content = tenant.GetService<IComponentService>().SelectText(((IConfiguration)config).MicroService(), config);

				r = new XtraReport();

				using var loadStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

				r.LoadLayoutFromXml(loadStream);

				BindDataSources(ms, r, url);
			}

			if (r == null)
				r = new XtraReport();

			r.RequestParameters = false;

			foreach (var parameter in r.Parameters)
				parameter.Visible = false;

			return r;
		}

		private void BindDataSources(IMicroService microService, XtraReport report, string url)
		{
			var tokens = url.Split(new char[] { '?' }, 2);
			JObject arguments = null;

			if (tokens.Length == 2)
				arguments = ParseArguments(tokens[1]);

			var connection = Shell.GetService<IConnectivityService>().SelectDefaultTenant();

			if (report.DataSource is JsonDataSource js)
			{
				var dataMember = report.DataMember;

				report.DataSource = CreateDataSource(js, dataMember, connection, arguments);
				report.DataMember = dataMember;
			}

			foreach (var band in report.Bands)
			{
				if (band is DetailReportBand detail && detail.DataSource != null && detail.DataSource is JsonDataSource djs)
				{
					var dataMember = detail.DataMember;

					detail.DataSource = CreateDataSource(djs, dataMember, connection, arguments);
					detail.DataMember = dataMember;
				}
			}
		}

		private JObject ParseArguments(string queryString)
		{
			return Serializer.Deserialize<JObject>(Encoding.UTF8.GetString(Convert.FromBase64String(queryString)));

		}
		private JsonDataSource CreateDataSource(JsonDataSource dataSource, string dataMember, ITenant tenant, JObject arguments)
		{
			if (dataSource == null || string.IsNullOrWhiteSpace(dataSource.ConnectionName))
				return null;

			var descriptor = ComponentDescriptor.Api(MicroServiceContext.FromIdentifier(dataSource.ConnectionName, tenant), dataSource.ConnectionName);

			descriptor.Validate();

			var ds = descriptor.Context.Interop.Invoke<object, JObject>(dataSource.ConnectionName, arguments);

			if (ds == null)
				return null;
			string serializedDs = string.Empty;

			if (!ds.GetType().IsCollection())
			{
				var list = new List<object>
				{
					ds
				};

				serializedDs = $"{{{dataMember}:{Serializer.Serialize(list)}}}";
			}
			else
				serializedDs = $"{{{dataMember}:{Serializer.Serialize(ds)}}}";

			var result = new JsonDataSource
			{
				JsonSource = new CustomJsonSource(serializedDs)
			};

			var y = result;

			result.Fill();

			return result;
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