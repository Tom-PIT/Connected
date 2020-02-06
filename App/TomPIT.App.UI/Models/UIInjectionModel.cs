using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.IoC;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Navigation;

namespace TomPIT.App.Models
{
	public class UIInjectionModel : AjaxModel, IViewModel
	{
		private List<IUIDependencyDescriptor> _views = null;
		public IViewConfiguration ViewConfiguration { get; set; }

		public IModelNavigation Navigation => null;
		public string Title => null;
		public IComponent Component { get; set; }

		public ITempDataProvider TempData { get; }
		private string ViewIdentifier { get; set; }
		private string PartialIdentifier { get; set; }
		private string ViewUrl { get; set; }
		protected override void OnDatabinding()
		{
			ViewIdentifier = Body.Required<string>("__view");
			PartialIdentifier = Body.Optional("partial", string.Empty);
			ViewUrl = Body.Optional("__viewUrl", string.Empty);

			var context = FromIdentifier(ViewIdentifier, MiddlewareDescriptor.Current.Tenant);

			MicroService = context.MicroService;

			var descriptor = ComponentDescriptor.View(this, ViewIdentifier);

			descriptor.Validate();

			Component = descriptor.Component;
			QualifierName = $"{MicroService.Name}/{descriptor.ComponentName}";
			ViewConfiguration = descriptor.Configuration;
			Body.Remove("__view");
			Body.Remove("__component");
		}

		public List<IUIDependencyDescriptor> Views
		{
			get
			{
				if (_views == null)
				{
					if (string.IsNullOrWhiteSpace(PartialIdentifier))
						_views = Tenant.GetService<IUIDependencyInjectionService>().QueryViewDependencies(ViewIdentifier, null);
					else
						_views = Tenant.GetService<IUIDependencyInjectionService>().QueryPartialDependencies(PartialIdentifier, null);

					if (_views == null)
						_views = new List<IUIDependencyDescriptor>();

					_views = _views.OrderBy(f => f.Order).ToList();
				}

				return _views;
			}
		}

		protected override void OnInitializing()
		{
			base.OnInitializing();

			if (string.IsNullOrWhiteSpace(ViewUrl))
				return;

			MiddlewareDescriptor.Current.Tenant.GetService<INavigationService>().MatchRoute(Services.Routing.RelativePath(new Uri(ViewUrl).LocalPath), Controller.Request.RouteValues);

			foreach (var i in Controller.Request.RouteValues)
				Arguments.Add(i.Key, Types.Convert<string>(i.Value));

			foreach (var i in ActionContext.HttpContext.Request.Query)
			{
				if (Arguments.ContainsKey(i.Key))
					continue;

				Arguments.Add(i.Key, i.Value.ToString());
			}
		}
	}
}
