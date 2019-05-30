using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.DataAccess.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Reports;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Reporting.Design.Dom;

namespace TomPIT.Reporting.Design.Designers
{
	public class ReportDesigner : DomDesigner<IDomElement>
	{
		private IMicroService _microService = null;
		private IReport _report = null;
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
				if(_microService==null)
					_microService = GetService<IMicroServiceService>().Select(Element.MicroService());

				return _microService;
			}
		}

		public IReport Report
		{
			get
			{
				if (_report == null)
				{
					var element = Element as ReportElement;

					_report = element.Component as IReport;
				}

				return _report;
			}
		}

		public string ReportUrl => $"{MicroService.Name}/{Report.ComponentName(Connection)}";

		public Dictionary<string, JsonDataSource> DataSources
		{
			get
			{
				if (_dataSources == null)
				{
					_dataSources = new Dictionary<string, JsonDataSource>();

					DiscoverOperations(MicroService.Name);

					var references = GetService<IDiscoveryService>().References(MicroService.Token);

					if(references !=null)
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
			var ms = GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return;

			var apis = GetService<IComponentService>().QueryConfigurations(ms.Token, "Api");

			foreach (var api in apis)
			{
				var config = api as IApi;

				foreach (var operation in config.Operations)
				{
					if (!(operation.Discover(Element.Environment.Context) is OperationManifest manifest) || !manifest.IsDataSource || manifest.Schema == null)
						return;

					var ds = new JsonDataSource
					{
						ConnectionName = $"{manifest.MicroService}/{manifest.Name}",
						Name = manifest.Name.Replace("/", "")
					};

					var root = new JsonSchemaNode
					{
						NodeType = JsonNodeType.Object
					};

					var schema = new JsonSchemaNode
					{
						NodeType = JsonNodeType.Array,
						Selected = true,
						Name = manifest.SchemaName
					};

					foreach (var child in manifest.Schema.Children())
					{
						if (child is JProperty property)
							schema.AddChildren(new[] { new JsonSchemaNode(property.Name, true, JsonNodeType.Property, ResolvePropertyType(property.Value)) });
						else if (child is JObject obj)
						{
							//TODO: implement object
						}
						else if (child is JArray array)
						{
							//TODO: implement collection
						}
					}

					root.AddChildren(schema);

					ds.Schema = root;

					_dataSources.Add(ds.Name, ds);
				}
			}
		}

		private Type ResolvePropertyType(JToken value)
		{
			return Types.GetType(value.Value<string>());
		}
	}
}
