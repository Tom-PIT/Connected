using System;
using System.Collections.Generic;
using DevExpress.DataAccess.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.Ide;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Reporting.Design.Dom;
using TomPIT.Reflection;
using TomPIT.Reflection.Manifests.Entities;

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

					ResolveDataSources(MicroService.Name);

					var references = Environment.Context.Tenant.GetService<IDiscoveryService>().References(MicroService.Token);

					if (references != null)
					{
						foreach (var reference in references.MicroServices)
							ResolveDataSources(reference.MicroService);
					}
				}

				return _dataSources;
			}
		}

		private void ResolveDataSources(string microService)
		{
			if (string.IsNullOrWhiteSpace(microService))
				return;

			var ms = Environment.Context.Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return;

			var apis = Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(ms.Token, ComponentCategories.Api);

			foreach (var api in apis)
			{
				var manifest = Environment.Context.Tenant.GetService<IDiscoveryService>().Manifest(api.Token) as ApiManifest;

				foreach (var operation in manifest.Operations)
				{
					if (operation.ReturnType == null || string.IsNullOrWhiteSpace(operation.ReturnType.Name))
						continue;

					var ds = new JsonDataSource
					{
						ConnectionName = $"{manifest.MicroService}/{manifest.Name}",
						Name = operation.Name
					};

					var root = new JsonSchemaNode
					{
						NodeType = JsonNodeType.Object
					};

					var schema = new JsonSchemaNode
					{
						NodeType = JsonNodeType.Array,
						Selected = true,
						Name = operation.Name
					};

					foreach (var child in operation.ReturnType.Properties)
					{
						//if (child is JProperty property)
						schema.AddChildren(new[] { new JsonSchemaNode(child.Name, true, JsonNodeType.Property, ResolvePropertyType(child.Type)) });
						//else if (child is JObject obj)
						//{
						//	//TODO: implement object
						//}
						//else if (child is JArray array)
						//{
						//	//TODO: implement collection
						//}
					}

					root.AddChildren(schema);

					ds.Schema = root;

					_dataSources.Add(ds.Name, ds);
				}
			}
		}

		private Type ResolvePropertyType(JToken value)
		{
			return TypeExtensions.GetType(value.Value<string>());
		}
	}
}
