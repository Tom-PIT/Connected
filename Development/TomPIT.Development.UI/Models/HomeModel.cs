using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Development.Reflection;
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
	}
}
