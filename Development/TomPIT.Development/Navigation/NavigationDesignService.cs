using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Navigation;
using TomPIT.Connectivity;
using TomPIT.Navigation;
using TomPIT.Services;

namespace TomPIT.Development.Navigation
{
	internal class NavigationDesignService : ServiceBase, INavigationDesignService
	{
		public NavigationDesignService(ISysConnection connection) : base(connection)
		{

		}
		public List<INavigationRouteDescriptor> QueryRouteKeys(Guid microService)
		{
			var configurations = Connection.GetService<IComponentService>().QueryConfigurations(microService, "SiteMap");
			var r = new List<INavigationRouteDescriptor>();

			foreach (var configuration in configurations)
			{
				var containers = QueryContainers(microService, configuration);

				if (containers == null)
					continue;

				foreach (var container in containers)
					FillKeys(container, r);
			}

			return r;
		}

		private void FillKeys(ISiteMapContainer container, List<INavigationRouteDescriptor> items)
		{
			foreach (var item in container.Items)
				FillKeys(item, items);
		}

		private void FillKeys(ISiteMapRoute route, List<INavigationRouteDescriptor> items)
		{
			if (!string.IsNullOrWhiteSpace(route.RouteKey) && items.FirstOrDefault(f => string.Compare(f.RouteKey, route.RouteKey, true) == 0) == null)
				items.Add(new NavigationRouteDescriptor { RouteKey = route.RouteKey, Template = route.Template, Text = route.Text });

			foreach (var item in route.Items)
				FillKeys(item, items);
		}

		public List<string> QuerySiteMapKeys(Guid microService)
		{
			var configurations = Connection.GetService<IComponentService>().QueryConfigurations(microService, "SiteMap");
			var r = new List<string>();

			foreach (var configuration in configurations)
			{
				var containers = QueryContainers(microService, configuration);

				if (containers == null)
					continue;

				foreach (var container in containers)
				{
					if (string.IsNullOrEmpty(container.Key))
						continue;

					if (r.Contains(container.Key))
						continue;

					r.Add(container.Key);
				}
			}

			return r;
		}

		private List<ISiteMapContainer> QueryContainers(Guid microService, IConfiguration configuration)
		{
			if (!(configuration is ISiteMapConfiguration siteMap))
				return null;

			var type = Connection.GetService<ICompilerService>().ResolveType(microService, siteMap, siteMap.ComponentName(Connection));

			if (type == null)
				return null;

			var instance = Connection.CreateProcessHandler<ISiteMapHandler>(microService, type);

			if (instance == null)
				return null;

			var containers = instance.Invoke();

			if (containers == null)
				return containers;

			foreach (var container in containers)
				BindContainer(container, instance.Context);

			return containers;
		}

		private void BindContainer(ISiteMapContainer container, IDataModelContext context)
		{
			foreach (var item in container.Items)
				BindRoute(item, context);
		}

		private void BindRoute(ISiteMapRoute route, IDataModelContext context)
		{
			if (route is ISiteMapContextElement ctx)
				ctx.Context = context;

			foreach (var item in route.Items)
				BindRoute(item, context);
		}
	}
}