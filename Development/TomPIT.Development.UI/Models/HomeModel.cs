using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Development.Models.Tools;
using TomPIT.Development.Reflection;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Models;

namespace TomPIT.Development.Models
{
	public class HomeModel : DevelopmentModel
	{
		private DiscoveryModel _discovery = null;

		protected override void OnInitializing(ModelInitializeParams p)
		{
			Title = SR.DevelopmentEnvironment;
		}

		public string UrlIde(IUrlHelper helper, string microServiceUrl)
		{
			return helper.RouteUrl("ide", new
			{
				microService = microServiceUrl
			});
		}

		public DiscoveryModel Discovery
		{
			get
			{
				if (_discovery == null)
					_discovery = new DiscoveryModel(this);

				return _discovery;
			}
		}

		public string ResolveTemplate(IMicroService microService)
		{
			if (microService.Template == Guid.Empty)
				return SR.MicroserviceNoTemplate;

			var template = Tenant.GetService<IMicroServiceTemplateService>().Select(microService.Template);

			if (template == null)
				return SR.ErrMicroServiceTemplateNotFound;

			return template.Name;
		}

		public void RunTool(string name)
		{
			Tenant.GetService<IToolsService>().Run(name);
		}

		public object GetData(string tool, JObject parameters)
		{
			if (string.Compare(tool, "ComponentList", true) == 0)
				return new ComponentListModel(this, Discovery).GetData(parameters);
			else
				return null;
		}

		public void AutoFix(string provider, Guid error)
		{
			Tenant.GetService<IDesignerService>().AutoFix(provider, error);
		}
	}
}
