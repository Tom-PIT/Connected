using System;
using System.Collections.Generic;
using DevExpress.DataAccess.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Reports;
using TomPIT.Ide;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Reporting.Design.Dom;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Reporting.Design.Designers
{
	public class ReportDesigner : DomDesigner<IDomElement>
	{
		private IMicroService _microService = null;
		private IReportConfiguration _report = null;
		private Dictionary<string, JsonDataSource> _dataSources = null;
		public ReportDesigner(IDomElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Report.cshtml";
		public override object ViewModel => this;

		public IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = Environment.Context.Tenant.GetService<IMicroServiceService>().Select(Element.MicroService());

				return _microService;
			}
		}

		public IReportConfiguration Report
		{
			get
			{
				if (_report == null)
				{
					var element = Element as ReportElement;

					_report = element.Component as IReportConfiguration;
				}

				return _report;
			}
		}

		public string ReportUrl => $"{MicroService.Name}/{Report.ComponentName()}";

		public Dictionary<string, JsonDataSource> DataSources
		{
			get
			{
				if (_dataSources == null)
				{
					_dataSources = new Dictionary<string, JsonDataSource>();

					DiscoverOperations(MicroService.Name);

					var references = Environment.Context.Tenant.GetService<IDiscoveryService>().References(MicroService.Token);

					if (references != null)
					{
						foreach (var reference in references.MicroServices)
							DiscoverOperations(reference.MicroService);
					}
				}

				return _dataSources;
			}
		}

		private void DiscoverOperations(string microService)
		{
			var ms = Environment.Context.Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return;

			var apis = Environment.Context.Tenant.GetService<IComponentService>().QueryConfigurations(ms.Token, "Api");

			foreach (var api in apis)
			{
				var config = api as IApiConfiguration;

				foreach (var operation in config.Operations)
				{
					//if (!(operation.Discover(Element.Environment.Context) is OperationManifest manifest) || !manifest.IsDataSource || manifest.Schema == null)
					//	return;

					//var ds = new JsonDataSource
					//{
					//	ConnectionName = $"{manifest.MicroService}/{manifest.Name}",
					//	Name = manifest.Name.Replace("/", "")
					//};

					//var root = new JsonSchemaNode
					//{
					//	NodeType = JsonNodeType.Object
					//};

					//var schema = new JsonSchemaNode
					//{
					//	NodeType = JsonNodeType.Array,
					//	Selected = true,
					//	Name = manifest.SchemaName
					//};

					//foreach (var child in manifest.Schema.Children())
					//{
					//	if (child is JProperty property)
					//		schema.AddChildren(new[] { new JsonSchemaNode(property.Name, true, JsonNodeType.Property, ResolvePropertyType(property.Value)) });
					//	else if (child is JObject obj)
					//	{
					//		//TODO: implement object
					//	}
					//	else if (child is JArray array)
					//	{
					//		//TODO: implement collection
					//	}
					//}

					//root.AddChildren(schema);

					//ds.Schema = root;

					//_dataSources.Add(ds.Name, ds);
				}
			}
		}

		private Type ResolvePropertyType(JToken value)
		{
			return TypeExtensions.GetType(value.Value<string>());
		}
	}
}
