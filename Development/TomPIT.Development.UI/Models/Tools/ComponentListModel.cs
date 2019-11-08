using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.Design.Tools;
using TomPIT.Development.Reflection;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Middleware;

namespace TomPIT.Development.Models.Tools
{
	public class ComponentListModel : MiddlewareComponent
	{
		private List<IComponent> _components = null;
		private List<IDevelopmentComponentError> _errors = null;
		private JArray _dataSource = null;
		private ITool _tool = null;
		private List<IAutoFixProvider> _autoFixes = null;
		public ComponentListModel(IMiddlewareContext context, DiscoveryModel discovery) : base(context)
		{
			Discovery = discovery;
		}

		private DiscoveryModel Discovery { get; }

		public List<IComponent> Components
		{
			get
			{
				if (_components == null)
					_components = Context.Tenant.GetService<IComponentDevelopmentService>().Query(Discovery.Services.Select(f => f.Token).ToArray());

				return _components;
			}
		}

		public List<IDevelopmentComponentError> Errors
		{
			get
			{
				if (_errors == null)
					_errors = Context.Tenant.GetService<IDesignerService>().QueryErrors(ErrorCategory.Type);

				return _errors;
			}
		}

		public JArray DataSource
		{
			get
			{
				if (_dataSource == null)
				{
					_dataSource = new JArray();

					Populate(_dataSource);
				}

				return _dataSource;
			}
		}

		private void Populate(JArray ds)
		{
			var microServices = Context.Tenant.GetService<IMicroServiceService>().Query();

			foreach (var component in Components)
			{
				var ms = microServices.FirstOrDefault(f => f.Token == component.MicroService);

				var item = new JObject
				{
					{"name", component.Name },
					{"microService", ms==null?string.Empty:ms.Name },
					{"category", component.Category },
					{"component", component.Token }
				};

				var errors = Errors.Where(f => f.Component == component.Token);

				if (errors.Count() == 0)
					item.Add("state", true);
				else
				{
					item.Add("state", false);
					item.Add("errorCount", errors.Count());

					var errorList = new JArray();

					item.Add("errors", errorList);

					foreach (var error in errors)
					{
						errorList.Add(new JObject
						{
							{ "code" , error.Code },
							{"message", error.Message },
							{"identifier", error.Identifier },
							{"autoFix", ResolveAutoFix(error) }

						});
					}
				}

				ds.Add(item);
			}
		}

		private string ResolveAutoFix(IDevelopmentError error)
		{
			foreach (var provider in AutoFixProviders)
			{
				if (provider.CanFix(this, new AutoFixArgs(Context, error)))
					return provider.Name;
			}

			return null;
		}

		public ITool Tool
		{
			get
			{
				if (_tool == null)
					_tool = Context.Tenant.GetService<IToolsService>().Select("ConfigurationTypeLoader");

				return _tool;
			}
		}

		public object GetData(JObject parameters)
		{
			return new JObject
			{
				{"dataSource", DataSource },
				{"toolStatus", ToolStatus },
				{"errorCount", Errors.Count.ToString("n0") },
			};
		}

		public string ToolStatus
		{
			get
			{
				if (Tool == null)
					return "-";
				else if (Tool.Status != TomPIT.Analysis.ToolStatus.Idle)
					return Tool.Status.ToString();
				else
					return Context.Services.Globalization.FromUtc(Tool.LastRun).ToString("g");
			}
		}

		private List<IAutoFixProvider> AutoFixProviders
		{
			get
			{
				if (_autoFixes == null)
					_autoFixes = Context.Tenant.GetService<IDesignerService>().QueryAutoFixProviders();

				return _autoFixes;
			}
		}
	}
}
