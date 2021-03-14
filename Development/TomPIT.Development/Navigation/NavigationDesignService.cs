using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Collections;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Navigation;
using TomPIT.Connectivity;
using TomPIT.Ide;
using TomPIT.Middleware;
using TomPIT.Navigation;

namespace TomPIT.Development.Navigation
{
	internal class NavigationDesignService : TenantObject, INavigationDesignService
	{
		public NavigationDesignService(ITenant tenant) : base(tenant)
		{

		}
		public ImmutableList<INavigationRouteDescriptor> QueryRouteKeys(Guid microService)
		{
			var configurations = Tenant.GetService<IComponentService>().QueryConfigurations(microService, ComponentCategories.SiteMap);
			var r = new HashSet<INavigationRouteDescriptor>(new NavigationRouteComparer());

			foreach (var configuration in configurations)
			{
				var containers = QueryContainers(microService, configuration);

				if (containers == null)
					continue;

				foreach (var container in containers)
					FillKeys(container, r);
			}

			return r.ToImmutableList();
		}

		private void FillKeys(ISiteMapContainer container, HashSet<INavigationRouteDescriptor> items)
		{
			foreach (var item in container.Routes)
			{
				if (container is ISiteMapRouteContainer routeContainer)
					items.Add(new NavigationRouteDescriptor { RouteKey = routeContainer.RouteKey, Template = routeContainer.Template, Text = routeContainer.Text });

				FillKeys(item, items);
			}
		}

		private void FillKeys(ISiteMapRoute route, HashSet<INavigationRouteDescriptor> items)
		{
			if (!string.IsNullOrWhiteSpace(route.RouteKey) && items.FirstOrDefault(f => string.Compare(f.RouteKey, route.RouteKey, true) == 0) == null)
				items.Add(new NavigationRouteDescriptor { RouteKey = route.RouteKey, Template = route.Template, Text = route.Text });

			foreach (var item in route.Routes)
				FillKeys(item, items);
		}

		public ImmutableList<string> QuerySiteMapKeys(Guid microService)
		{
			var configurations = Tenant.GetService<IComponentService>().QueryConfigurations(microService, "SiteMap");
			var r = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var configuration in configurations)
			{
				var containers = QueryContainers(microService, configuration);

				if (containers == null)
					continue;

				foreach (var container in containers)
				{
					if (string.IsNullOrEmpty(container.Key))
						continue;

					r.Add(container.Key);

					QuerySiteMapKeys(container.Routes, r);
				}
			}

			return r.ToImmutableList();
		}

		private void QuerySiteMapKeys(ConnectedList<ISiteMapRoute, ISiteMapContainer> routes, HashSet<string> items)
		{
			if (routes == null)
				return;

			foreach(var route in routes)
			{
				if (string.IsNullOrWhiteSpace(route.RouteKey))
					continue;

				items.Add(route.RouteKey);

				QuerySiteMapKeys(route.Routes, items);
			}
		}

		private void QuerySiteMapKeys(ConnectedList<ISiteMapRoute, ISiteMapRoute> routes, HashSet<string> items)
		{
			if (routes == null)
				return;

			foreach (var route in routes)
			{
				if (string.IsNullOrWhiteSpace(route.RouteKey))
					continue;

				items.Add(route.RouteKey);

				QuerySiteMapKeys(route.Routes, items);
			}
		}

		private List<ISiteMapContainer> QueryContainers(Guid microService, IConfiguration configuration)
		{
			if (!(configuration is ISiteMapConfiguration siteMap))
				return null;

			var type = Tenant.GetService<ICompilerService>().ResolveType(microService, siteMap, siteMap.ComponentName());

			if (type == null)
				return null;

			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);
			var instance = Tenant.GetService<ICompilerService>().CreateInstance<ISiteMapHandler>(new MicroServiceContext(ms, Tenant.Url), type);

			if (instance == null)
				return null;

			var containers = instance.Invoke();

			if (containers == null)
				return containers;

			foreach (var container in containers)
				BindContainer(container, instance.Context);

			return containers;
		}

		private void BindContainer(ISiteMapContainer container, IMiddlewareContext context)
		{
			foreach (var item in container.Routes)
				BindRoute(item, context);
		}

		private void BindRoute(ISiteMapRoute route, IMiddlewareContext context)
		{
			route.SetContext(context);

			foreach (var item in route.Routes)
				BindRoute(item, context);
		}
	}
}