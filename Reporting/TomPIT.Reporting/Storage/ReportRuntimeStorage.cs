using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DevExpress.DataAccess.Json;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using Newtonsoft.Json;
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

					//BindDataSources(r, url);
				}
			}

			if (r == null)
				r = new XtraReport();

			using (MemoryStream stream = new MemoryStream())
			{
				r.SaveLayoutToXml(stream);

				return stream.ToArray();
			}
		}

		public XtraReport CreateReport(string url)
		{
			var report = SelectReport(url);
			JObject arguments = null;

			if (url.Contains("?"))
				arguments = ParseArguments(url.Split("?")[^1]);

			return CreateReport(report, arguments);
		}

		private XtraReport CreateReport(IReportConfiguration config, object arguments)
		{
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

				BindDataSources(r, url, null);
			}

			if (r == null)
				r = new XtraReport();

			r.RequestParameters = false;

			foreach (var parameter in r.Parameters)
				parameter.Visible = false;

			var subreports = r.AllControls<XRSubreport>();

			foreach (var subreport in subreports)
				subreport.BeforePrint += OnBindSubreportDataSources;

			return r;
		}

		private void OnBindSubreportDataSources(object sender, System.Drawing.Printing.PrintEventArgs e)
		{
			var subReport = sender as XRSubreport;

			subReport.ApplyParameterBindings();
			/*
			 * this is workaround for the issue related to parameter bindings.
			 * it seems devexpress doesn't know how to bind parameters from json data source
			 * so we're gonna do it manually.
			 */
			var bindings = subReport.ParameterBindings;
			var newBindings = new List<ParameterBinding>();

			foreach (var binding in bindings)
			{
				ReportParameterTag tag = null;

				if (binding.Parameter != null && !(binding.Parameter.Tag is ReportParameterTag))
				{
					newBindings.Add(binding);
					continue;
				}

				tag = binding.Parameter?.Tag as ReportParameterTag;

				if (subReport.ReportSource.DataSource == null)
					continue;

				if (tag == null)
				{
					tag = new ReportParameterTag
					{
						DataSource = binding.DataSource as JsonDataSource,
						DataMember = binding.DataMember
					};
				}

				var entity = tag.DataMember.Split('.')[0];
				var property = tag.DataMember.Split('.')[1];
				var index = subReport.Report.CurrentRowIndex;
				var en = tag.DataSource.GetEnumerator();

				if (!en.MoveNext())
					continue;

				var pi = en.Current.GetType().GetProperty(entity);

				if (pi == null)
					continue;

				if (!(pi.GetValue(en.Current) is IList list))
					continue;

				if (list.Count - 1 < index)
					continue;

				var item = list[index];
				pi = item.GetType().GetProperty(property);

				if (pi == null)
					continue;

				newBindings.Add(new ParameterBinding(binding.ParameterName, new DevExpress.XtraReports.Parameters.Parameter
				{
					Tag = tag,
					Name = binding.ParameterName,
					Type = pi.PropertyType,
					Value = pi.GetValue(item),
				}));
			}

			subReport.ParameterBindings.Clear();

			foreach (var binding in newBindings)
				subReport.ParameterBindings.Add(binding);

			BindDataSources(subReport.ReportSource, subReport.ReportSourceUrl, subReport.ParameterBindings);
		}
		public XtraReport CreateReport(Guid component, object arguments)
		{
			var config = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(component) as IReportConfiguration;

			if (config == null)
				return null;

			return CreateReport(config, arguments);
		}

		private void BindDataSources(XtraReport report, string url, ParameterBindingCollection bindings)
		{
			var tokens = url.Split(new char[] { '?' }, 2);
			JObject arguments = null;

			if (tokens.Length == 2)
				arguments = ParseArguments(tokens[1]);

			foreach (var parameter in report.Parameters)
			{
				if (arguments == null)
					arguments = new JObject();

				if (arguments.ContainsKey(parameter.Name))
					arguments[parameter.Name] = new JValue(parameter.Value);
				else
					arguments.Add(parameter.Name, new JValue(parameter.Value));
			}

			if (bindings != null)
			{
				foreach (var binding in bindings)
				{
					if (arguments == null)
						arguments = new JObject();

					if (arguments.ContainsKey(binding.ParameterName))
						arguments[binding.ParameterName] = new JValue(binding.Parameter?.Value);
					else
						arguments.Add(binding.ParameterName, new JValue(binding.Parameter?.Value));
				}
			}

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
			if (dataSource.Site == null)
			{
				dataSource.Site = new DataSourceSite(dataSource.ConnectionName);
				dataSource.ConnectionName = null;
			}

			var connection = (dataSource.Site as DataSourceSite).Connection;

			if (dataSource == null || string.IsNullOrWhiteSpace(dataMember))
				return dataSource;

			var descriptor = ComponentDescriptor.Api(MicroServiceContext.FromIdentifier(connection, tenant), connection);

			descriptor.Validate();

			var ds = descriptor.Context.Interop.Invoke<object, JObject>(connection, arguments);

			if (ds == null)
				return null;

			var serializedDs = string.Empty;

			if (!ds.GetType().IsCollection())
			{
				var list = new List<object>
				{
					ds
				};

				serializedDs = $"{{{dataMember}:{JsonConvert.SerializeObject(list)}}}";
			}
			else
				serializedDs = $"{{{dataMember}:{JsonConvert.SerializeObject(ds)}}}";

			dataSource.Schema = null;
			dataSource.JsonSource = new CustomJsonSource(serializedDs);

			dataSource.Fill();

			return dataSource;
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