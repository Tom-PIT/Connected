using DevExpress.DataAccess.Json;

using Newtonsoft.Json.Linq;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TomPIT.Annotations.Design;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Reports;
using TomPIT.Design;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.MicroServices.Reporting.Design.Dom;
using TomPIT.Middleware.Interop;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Reporting.Design.Designers
{
	public class ReportDesigner : DomDesigner<IDomElement>
	{
		private IMicroService _microService = null;
		private IReportConfiguration _report = null;
		private List<ReportDataSource> _dataSources = null;
		private List<JsonDataSource> _reportDataSources = null;

		public ReportDesigner(IDomElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Report.cshtml";
		public override object ViewModel => this;

		public IMicroService MicroService => _microService ??= Environment.Context.Tenant.GetService<IMicroServiceService>().Select(Element.MicroService());

		public IReportConfiguration Report => _report ??= (Element as ReportElement)?.Component as IReportConfiguration;
		
		public string ReportUrl => $"{MicroService.Name}/{Report.ComponentName()}";

		public List<ReportDataSource> DataSources
		{
			get
			{
				if (_dataSources == null)
				{
					_dataSources = new List<ReportDataSource>();

					ResolveDataSources(MicroService.Name);

					var references = Environment.Context.Tenant.GetService<IDiscoveryService>().MicroServices.References.Select(MicroService.Token);

					if (references != null)
					{
						foreach (var reference in references.MicroServices)
							ResolveDataSources(reference.MicroService);
					}
				}

				return _dataSources;
			}
		}

		public List<JsonDataSource> ReportDataSources
		{
			get
			{
				if (_reportDataSources == null)
				{
					_reportDataSources = new List<JsonDataSource>();

					foreach (var api in Report.Apis)
					{
						var ds = CreateDataSource(api);

						if (ds != null)
							_reportDataSources.Add(ds);
					}
				}

				return _reportDataSources;
			}
		}

		private void ResolveDataSources(string microService)
		{
			if (string.IsNullOrWhiteSpace(microService))
				return;

			var ms = Environment.Context.Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return;

			_dataSources.Add(new ReportDataSource
			{
				Id = ms.Token.ToString(),
				Text = ms.Name
			});

			var apis = Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(ms.Token, ComponentCategories.Api);

			foreach (var api in apis)
			{
				var config = Environment.Context.Tenant.GetService<IComponentService>().SelectConfiguration(api.Token) as IApiConfiguration;

				_dataSources.Add(new ReportDataSource
				{
					Parent = ms.Token.ToString(),
					Id = api.Token.ToString(),
					Text = api.Name
				});

				foreach (var operation in config.Operations)
				{
					_dataSources.Add(new ReportDataSource
					{
						Parent = api.Token.ToString(),
						Id = operation.Id.ToString(),
						Text = operation.Name,
						IsDataSource = true
					});
				}
			}
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "loadApis", true) == 0)
				return Result.JsonResult(ViewModel, DataSources);
			else if (string.Compare(action, "addApi", true) == 0)
				return AddApi(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult AddApi(JObject data)
		{
			var api = data.Required<Guid>("api");
			var operation = data.Required<Guid>("operation");

			if (!(Environment.Context.Tenant.GetService<IComponentService>().SelectConfiguration(api) is IApiConfiguration config))
				return Result.EmptyResult(ViewModel);

			var microService = Environment.Context.Tenant.GetService<IMicroServiceService>().Select(config.MicroService());
			var op = config.Operations.FirstOrDefault(f => f.Id == operation);

			if (op == null)
				return Result.EmptyResult(ViewModel);

			var url = $"{microService.Name}/{config.ComponentName()}/{op.Name}";

			if (Report.Apis.FirstOrDefault(f => string.Compare(f, url, true) == 0) != null)
				return Result.EmptyResult(ViewModel);

			Report.Apis.Add(url);

			Environment.Context.Tenant.GetService<IDesignService>().Components.Update(Report);

			return Result.SectionResult(ViewModel, EnvironmentSection.Designer);
		}

		private JsonDataSource CreateDataSource(string api)
		{
			var result = new JsonDataSource();

			var descriptor = ComponentDescriptor.Api(Environment.Context, api);

			try
			{
				descriptor.Validate();
			}
			catch
			{
				return null;
			}

			result.ConnectionName = $"{descriptor.MicroService.Name}/{descriptor.Component.Name}/{descriptor.Element}";

			var op = descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			CreateRoot(result);

			result.Name = op.Name;

			var operationType = Environment.Context.Tenant.GetService<ICompilerService>().ResolveType(descriptor.MicroService.Token, op, op.Name);

			if (operationType is null)
				return null;

			var returnType = ResolveOperationReturnType(operationType);

			if (returnType is null)
				return null;

			var schema = new JsonSchemaNode(ResolveSchemaName(returnType), true, JsonNodeType.Array)
			{
				DisplayName = ResolveSchemaName(returnType)
			};

			result.Schema.AddChildren(schema);

			var fields = new List<JsonSchemaNode>();
			var members = typeof(IEnumerable).IsAssignableFrom(returnType) ? returnType.GenericTypeArguments[0].GetProperties() : returnType.GetProperties();

			foreach (var property in members)
			{
				var type = property.PropertyType;

				fields.Add(new JsonSchemaNode(new JsonNode(property.Name, true, JsonNodeType.Property)
				{
					Type = ResolveType(type)
				}));
			}

			schema.AddChildren(fields.ToArray());

			return result;
		}

		private static Type? ResolveOperationReturnType(Type operationType)
		{
			var operationInterface = operationType.GetInterface($"{nameof(IOperation)}`1");

			if (operationInterface is null)
				return null;

			return operationInterface.GetGenericArguments()[0];
		}

		private static string ResolveSchemaName(Type operationReturnType)
		{
			if (!typeof(IEnumerable).IsAssignableFrom(operationReturnType))
				return operationReturnType.Name;

			return operationReturnType.GenericTypeArguments[0].Name;
		}

		private static Type ResolveType(Type descriptor)
		{
			if (TypeExtensions.GetType(descriptor.TypeName()) is Type resolved)
				return resolved;

			return typeof(string);
		}		

		private void CreateRoot(JsonDataSource ds)
		{
			var root = new JsonSchemaNode
			{
				NodeType = JsonNodeType.Object
			};

			ds.Schema = root;
		}
	}
}
