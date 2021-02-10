using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DevExpress.DataAccess.Json;
using DevExpress.XtraReports.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security;
using TomPIT.Serialization;

namespace TomPIT.MicroServices.Reporting.Storage
{
	internal class ReportCreateSession
	{
		public Guid Component{ get; set; }
		public object Arguments { get; set; }
		public string User { get; set; }

		public XtraReport CreateReport()
		{
			ResolveUser();

			if (MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(Component) is not IReportConfiguration config)
				return null;

			return CreateReport(config, Arguments, User);
		}

		private void ResolveUser()
		{
			if(!string.IsNullOrWhiteSpace(User))
			{
				if (MiddlewareDescriptor.Current.Tenant.GetService<IUserService>().Select(User) == null)
					User = null;
			}

			if (string.IsNullOrWhiteSpace(User))
				User = MiddlewareDescriptor.Current.UserToken.ToString();
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

		public static JObject ParseArguments(string queryString)
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

			using var ctx = MicroServiceContext.FromIdentifier(connection, tenant);

			if (!string.IsNullOrWhiteSpace(User))
				ctx.Impersonate(User);

			var descriptor = ComponentDescriptor.Api(ctx, connection);

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

		private XtraReport CreateReport(IReportConfiguration config, object arguments, string user)
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

				if (pi.GetValue(en.Current) is not IList list)
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
	}
}
