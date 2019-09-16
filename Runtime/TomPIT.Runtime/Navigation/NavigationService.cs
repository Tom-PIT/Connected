using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Navigation;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Navigation
{
	internal class NavigationService : ConfigurationRepository<ISiteMapConfiguration>, INavigationService
	{
		private static Lazy<ConcurrentDictionary<string, List<NavigationHandlerDescriptor>>> _handlers = new Lazy<ConcurrentDictionary<string, List<NavigationHandlerDescriptor>>>(() => { return new ConcurrentDictionary<string, List<NavigationHandlerDescriptor>>(StringComparer.OrdinalIgnoreCase); });
		public NavigationService(ITenant tenant) : base(tenant, "sitemap")
		{
		}

		protected override void OnChanged(Guid component)
		{
			RefreshNavigation(component);
		}

		protected override void OnAdded(Guid component)
		{
			RefreshNavigation(component);
		}

		protected override void OnRemoved(Guid component)
		{
			RefreshNavigation(component, true);
		}

		protected override string[] Categories => new string[] { ComponentCategories.SiteMap };

		public ISiteMapContainer QuerySiteMap(params string[] keys)
		{
			Initialize();

			if (keys.Length == 0)
				return null;

			var containers = new List<ISiteMapContainer>();

			foreach (var key in keys)
			{
				var items = LoadSiteMap(key);

				if (items != null && items.Count > 0)
					containers.AddRange(items);
			}

			var r = new SiteMapContainer();

			foreach (var container in containers)
			{
				if (container.Routes != null && container.Routes.Count > 0)
					r.Routes.AddRange(container.Routes);
			}

			return r;
		}

		private List<ISiteMapContainer> LoadSiteMap(string key)
		{
			if (!Handlers.ContainsKey(key))
				return null;

			var r = new List<ISiteMapContainer>();

			if (!Handlers.TryGetValue(key, out List<NavigationHandlerDescriptor> handlers))
				return r;

			foreach (var handler in handlers)
			{
				var ms = Tenant.GetService<IMicroServiceService>().Select(handler.MicroService);
				var instance = Tenant.GetService<ICompilerService>().CreateInstance<ISiteMapHandler>(new MiddlewareContext(Tenant.Url, ms), handler.Handler);
				var containers = instance.Invoke(key);

				if (containers == null || containers.Count == 0)
					continue;

				foreach (var container in containers)
					BindContext(container, instance.Context);

				r.AddRange(containers);
			}

			return r;
		}

		private void BindContext(ISiteMapContainer container, IMiddlewareContext context)
		{
			foreach (var item in container.Routes)
			{
				item.Context = context;

				BindContext(item, context);
			}
		}

		private void BindContext(ISiteMapRoute route, IMiddlewareContext context)
		{
			foreach (var item in route.Routes)
			{
				item.Context = context;

				BindContext(item, context);
			}
		}

		private ConcurrentDictionary<string, List<NavigationHandlerDescriptor>> Handlers => _handlers.Value;

		private void RefreshNavigation(ISiteMapConfiguration configuration, bool removeOny = false)
		{
			var ms = ((IConfiguration)configuration).MicroService();
			var type = Tenant.GetService<ICompilerService>().ResolveType(ms, configuration, configuration.ComponentName());
			var handlerInstance = type.CreateInstance<ISiteMapHandler>();
			handlerInstance.Context = configuration.CreateContext();

			var containers = handlerInstance.Invoke();

			if (containers == null)
				return;

			lock (_handlers.Value)
			{
				foreach (var container in containers)
				{
					if (Handlers.ContainsKey(container.Key))
					{
						if (Handlers[container.Key].FirstOrDefault(f => f.Component == configuration.Component) != null)
							continue;

						var descriptor = new NavigationHandlerDescriptor(ms, configuration.Component, type);

						FillRouteKeys(descriptor, container);

						Handlers[container.Key].Add(descriptor);
					}
					else
					{
						var descriptor = new NavigationHandlerDescriptor(ms, configuration.Component, type);

						FillRouteKeys(descriptor, container);

						Handlers.TryAdd(container.Key, new List<NavigationHandlerDescriptor>
						{
							descriptor
						});
					}
				}
			}
		}

		private void RefreshNavigation(Guid component, bool removeOny = false)
		{
			foreach (var key in Handlers)
			{
				var handlers = key.Value.Where(f => f.Component == component);

				for (var i = handlers.Count() - 1; i >= 0; i--)
					key.Value.Remove(handlers.ElementAt(i));
			}

			if (removeOny)
				return;

			var cmp = Tenant.GetService<IComponentService>().SelectComponent(component);

			if (cmp == null)
				return;

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(cmp.Token) as ISiteMapConfiguration;

			RefreshNavigation(config);
		}

		private void FillRouteKeys(NavigationHandlerDescriptor descriptor, ISiteMapContainer container)
		{
			foreach (var item in container.Routes)
				FillRouteKeys(descriptor, item);
		}

		private void FillRouteKeys(NavigationHandlerDescriptor descriptor, ISiteMapRoute route)
		{
			if (!string.IsNullOrWhiteSpace(route.RouteKey) && !descriptor.RouteKeys.Contains(route.RouteKey.ToLowerInvariant()))
				descriptor.RouteKeys.Add(route.RouteKey.ToLowerInvariant());

			foreach (var item in route.Routes)
			{
				if (!string.IsNullOrWhiteSpace(item.RouteKey) && !descriptor.RouteKeys.Contains(item.RouteKey.ToLowerInvariant()))
					descriptor.RouteKeys.Add(item.RouteKey.ToLowerInvariant());

				FillRouteKeys(descriptor, item);
			}
		}
		public List<IBreadcrumb> QueryBreadcrumbs(string routeKey, RouteValueDictionary parameters)
		{
			Initialize();

			var item = SelectRoute(routeKey);

			if (item == null)
				return null;

			var result = new List<IBreadcrumb>();

			ProcessBreadcrumb(item, result, parameters);

			return result;
		}

		private void ProcessBreadcrumb(ISiteMapRoute item, List<IBreadcrumb> items, RouteValueDictionary parameters)
		{
			var breadCrumb = new Breadcrumb
			{
				Key = item.Template,
				Text = item.Text
			};
			/*
			 * Last breadcrumb should be without a link 
			 * because it points to a currently displayed ui
			 */
			if (items.Count > 0 && !string.IsNullOrWhiteSpace(item.Template))
				breadCrumb.Url = ParseUrl(item.Template, parameters);

			if (items.Count == 0 || !string.IsNullOrWhiteSpace(breadCrumb.Url))
				items.Insert(0, breadCrumb);

			ISiteMapElement parent = null;

			while (parent == null)
			{
				if (item.Parent == null)
					break;

				parent = item.Parent;

				if (parent is ISiteMapElement)
					break;
			}

			if (parent != null)
				ProcessBreadcrumb(parent as ISiteMapRoute, items, parameters);
		}

		public string ParseUrl(string template, RouteValueDictionary parameters)
		{
			if (parameters == null)
			{
				if (Shell.HttpContext != null)
					parameters = Shell.HttpContext.GetRouteData().Values;
				else
					parameters = new RouteValueDictionary();
			}

			var parsedTemplate = TemplateParser.Parse(template);

			var processedSegments = new List<string>();

			foreach (var segment in parsedTemplate.Segments)
			{
				var incomplete = false;

				foreach (var part in segment.Parts)
				{
					if (!part.IsParameter)
						processedSegments.Add(part.Text);
					else
					{
						if (!parameters.ContainsKey(part.Name))
						{
							if (part.IsOptional)
							{
								incomplete = true;
								break;
							}
							else
								return null;
						}
						else
							processedSegments.Add(Types.Convert<string>(parameters[part.Name]));
					}
				}

				if (incomplete)
					break;
			}

			var ctx = new MiddlewareContext();

			return $"{ctx.Services.Routing.RootUrl}/{string.Join('/', processedSegments)}";
		}

		public ISiteMapRoute SelectRoute(string routeKey)
		{
			Initialize();

			foreach (var handler in Handlers)
			{
				foreach (var descriptor in handler.Value)
				{
					if (descriptor.RouteKeys.Contains(routeKey.ToLowerInvariant()))
						return SelectRoute(descriptor, routeKey);
				}
			}

			return null;
		}

		private ISiteMapRoute SelectRoute(NavigationHandlerDescriptor descriptor, string routeKey)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(descriptor.MicroService);
			var handler = Tenant.GetService<ICompilerService>().CreateInstance<ISiteMapHandler>(new MiddlewareContext(Tenant.Url, ms), descriptor.Handler);

			if (handler == null)
				return null;

			var containers = handler.Invoke();

			if (containers == null)
				return null;

			foreach (var container in containers)
				BindContext(container, handler.Context);

			foreach (var container in containers)
			{
				var r = SelectRoute(container, routeKey);

				if (r != null)
					return r;
			}

			return null;
		}

		private ISiteMapRoute SelectRoute(ISiteMapContainer container, string routeKey)
		{
			foreach (var item in container.Routes)
			{
				if (string.Compare(item.RouteKey, routeKey, true) == 0)
					return item;

				var r = SelectRoute(item, routeKey);

				if (r != null)
					return r;
			}

			return null;
		}

		private ISiteMapRoute SelectRoute(ISiteMapRoute route, string routeKey)
		{
			foreach (var item in route.Routes)
			{
				if (string.Compare(item.RouteKey, routeKey, true) == 0)
					return item;

				var r = SelectRoute(item, routeKey);

				if (r != null)
					return r;
			}

			return null;
		}

		public List<string> QueryRouteKeys()
		{
			Initialize();

			var r = new List<string>();

			foreach (var descriptor in Handlers)
			{
				foreach (var handler in descriptor.Value)
				{
					foreach (var key in handler.RouteKeys)
					{
						if (r.Contains(key))
							continue;

						r.Add(key);
					}
				}
			}

			return r;
		}
	}
}
