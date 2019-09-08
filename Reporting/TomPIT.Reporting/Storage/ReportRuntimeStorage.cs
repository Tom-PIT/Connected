using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Web;
using DevExpress.DataAccess.Json;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Services;

namespace TomPIT.Reporting.Storage
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

		protected IReport SelectReport(string url)
		{
			var connection = SysExtensions.CurrentConnection();

			if (connection == null)
				return null;

			var tokens = url.Split('/');

			if (tokens.Length != 2)
				return null;

			var ms = connection.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return null;

			var subTokens = tokens[1].Split('?');

			return connection.GetService<IComponentService>().SelectConfiguration(ms.Token, "Report", subTokens[0]) as IReport;
		}
		public override byte[] GetData(string url)
		{
			var config = SelectReport(url);

			if (config == null)
				return null;

			XtraReport r = null;

			if (config.TextBlob != Guid.Empty)
			{
				var connection = SysExtensions.CurrentConnection();
				var content = connection.GetService<IComponentService>().SelectText(((IConfiguration)config).MicroService(connection), config);
				var ms = connection.GetService<IMicroServiceService>().Select(((IConfiguration)config).MicroService(connection));

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

		private void BindDataSources(IMicroService microService, XtraReport report, string url)
		{
			var tokens = url.Split(new char[] { '?' }, 2);

			if (tokens.Length != 2)
				return;

			var queryString = HttpUtility.ParseQueryString(tokens[1]);
			var arguments = new JObject();

			foreach (var s in queryString.Keys)
				arguments.Add(s.ToString(), queryString[s.ToString()]);

			var connection = Shell.GetService<IConnectivityService>().Select();

			if (report.DataSource is JsonDataSource js)
			{
				var dataMember = report.DataMember;

				report.DataSource = CreateDataSource(js, connection, arguments);
				report.DataMember = dataMember;
			}

			foreach(var band in report.Bands)
			{
				if (band is DetailReportBand detail && detail.DataSource != null && detail.DataSource is JsonDataSource djs)
				{
					var dataMember = detail.DataMember;

					detail.DataSource = CreateDataSource(djs, connection, arguments);
					detail.DataMember = dataMember;
				}
			}
		}

		private JsonDataSource CreateDataSource(JsonDataSource dataSource, ISysConnection connection, JObject arguments)
		{
			if (dataSource == null || string.IsNullOrWhiteSpace(dataSource.ConnectionName))
				return null;

			var tokens = dataSource.ConnectionName.Split('/');
			var ms = connection.GetService<IMicroServiceService>().Select(tokens[0]);
			var ctx = ExecutionContext.Create(connection.Url, ms);
			var result = new JsonDataSource();
			var root = new JObject();
			var apiDataSource = ctx.Invoke<object>(dataSource.ConnectionName, arguments);
			var schemaNode = dataSource.Schema.Nodes[0] as JsonSchemaNode;

			if (apiDataSource is JObject)
				root.Add(schemaNode.Name, new JArray { apiDataSource });
			else if (apiDataSource is JArray ja)
				root.Add(schemaNode.Name, ja);
			else if (apiDataSource.GetType().IsCollection())
				root.Add(schemaNode.Name, JsonConvert.DeserializeObject<JArray>( JsonConvert.SerializeObject(apiDataSource)));
			else
				root.Add(schemaNode.Name, new JArray { JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(apiDataSource)) });

			result.JsonSource = new CustomJsonSource(JsonConvert.SerializeObject(root));

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