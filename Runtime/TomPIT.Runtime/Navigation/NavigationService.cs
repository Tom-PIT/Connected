using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Navigation;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
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

		protected override void OnChanged(Guid microService, Guid component)
		{
			RefreshNavigation(component);
		}

		protected override void OnAdded(Guid microService, Guid component)
		{
			RefreshNavigation(component);
		}

		protected override void OnRemoved(Guid microService, Guid component)
		{
			RefreshNavigation(component, true);
		}

		protected override void OnInitialized()
		{
			Parallel.ForEach(All(), (i) =>
			{
				OnAdded(i.MicroService(), i.Component);
			});
		}
		protected override string[] Categories => new string[] { ComponentCategories.SiteMap };
		public ISiteMapContainer QuerySiteMap(List<string> keys)
		{
			return QuerySiteMap(keys, null);
		}

		public List<ISiteMapContainer> QueryContainers()
		{
			Initialize();

			var containers = new List<ISiteMapContainer>();
			var keys = QueryKeys();

			foreach (var key in keys)
			{
				var items = LoadSiteMap(key, null);

				if (items != null && items.Count > 0)
					containers.AddRange(items);
			}

			return containers;
		}
		public ISiteMapContainer QuerySiteMap(List<string> keys, List<string> tags)
		{
			Initialize();

			if (keys == null || keys.Count == 0)
				return null;

			var containers = new List<ISiteMapContainer>();

			foreach (var key in keys)
			{
				var items = LoadSiteMap(key, tags);

				if (items != null && items.Count > 0)
					containers.AddRange(items);
			}

			var r = new SiteMapContainer();

			foreach (var container in containers)
			{
				if (string.IsNullOrWhiteSpace(r.Text))
					r.Text = container.Text;

				if (container.Routes != null && container.Routes.Count > 0)
					r.Routes.AddRange(container.Routes);
			}

			return r;
		}

		private List<ISiteMapContainer> LoadSiteMap(string key, List<string> tags)
		{
			if (!Handlers.ContainsKey(key))
				return null;

			var r = new List<ISiteMapContainer>();

			if (!Handlers.TryGetValue(key, out List<NavigationHandlerDescriptor> handlers))
				return r;

			foreach (var handler in handlers)
			{
				var ms = Tenant.GetService<IMicroServiceService>().Select(handler.MicroService);
				var instance = Tenant.GetService<ICompilerService>().CreateInstance<ISiteMapHandler>(new MicroServiceContext(ms, Tenant.Url), handler.Handler);
				var containers = instance.Invoke(key);

				if (containers == null || containers.Count == 0)
					continue;

				if (tags != null && tags.Count > 0)
				{
					for (var i = containers.Count - 1; i >= 0; i--)
					{
						var containerTags = containers[i].Tags;

						if (containerTags == null || containerTags.Length == 0)
						{
							containers.RemoveAt(i);
							continue;
						}

						var containerTagTokens = containerTags.Split(',', ';');

						foreach (var token in containerTagTokens)
						{
							if (!tags.Any(f => string.Compare(f, token, true) == 0))
								containers.RemoveAt(i);
						}
					}
				}

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
				ReflectionExtensions.SetPropertyValue(item, nameof(item.Parent), container);
				item.SetContext(context);

				BindContext(item, context);
			}
		}

		private void BindContext(ISiteMapRoute route, IMiddlewareContext context)
		{
			foreach (var item in route.Routes)
			{
				if (item.Parent == null)
					ReflectionExtensions.SetPropertyValue(item, nameof(item.Parent), route);

				item.SetContext(context);

				BindContext(item, context);
			}
		}

		public List<string> QueryKeys()
		{
			return Handlers.Keys.ToList();
		}

		private ConcurrentDictionary<string, List<NavigationHandlerDescriptor>> Handlers => _handlers.Value;

		private void RefreshNavigation(ISiteMapConfiguration configuration, bool removeOny = false)
		{
			var ms = ((IConfiguration)configuration).MicroService();
			var type = Tenant.GetService<ICompilerService>().ResolveType(ms, configuration, configuration.ComponentName(), false);

			if (type == null)
				return;

			var handlerInstance = type.CreateInstance<ISiteMapHandler>();

			if (handlerInstance == null)
				return;

			handlerInstance.SetContext(configuration.CreateContext());

			var containers = handlerInstance.Invoke();

			if (containers == null)
				return;

			foreach (var container in containers)
			{
				if (string.IsNullOrWhiteSpace(container.Key))
				{
					MiddlewareDescriptor.Current.Tenant.LogWarning(nameof(NavigationService), $"{SR.WrnContainerKeyNull} ({container.Text})", LogCategories.Navigation);
					continue;
				}

				if (string.IsNullOrWhiteSpace(container.Key))
					continue;

				if (!Handlers.ContainsKey(container.Key))
				{
					lock (Handlers)
					{
						if (!Handlers.ContainsKey(container.Key))
							Handlers.TryAdd(container.Key, new List<NavigationHandlerDescriptor>());
					}
				}

				if (Handlers[container.Key].FirstOrDefault(f => f.Component == configuration.Component) != null)
					continue;

				var descriptor = new NavigationHandlerDescriptor(ms, configuration.Component, type);

				FillDescriptor(descriptor, container);

				var list = Handlers[container.Key];

				lock (list)
				{
					list.Add(descriptor);
				}
			}
		}

		private void RefreshNavigation(Guid component, bool removeOny = false)
		{
			foreach (var key in Handlers)
			{
				lock (key.Value)
				{
					var handlers = key.Value.Where(f => f.Component == component);

					for (var i = handlers.Count() - 1; i >= 0; i--)
						key.Value.Remove(handlers.ElementAt(i));
				}
			}

			if (removeOny)
				return;

			var cmp = Tenant.GetService<IComponentService>().SelectComponent(component);

			if (cmp == null)
				return;

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(cmp.Token) as ISiteMapConfiguration;

			try
			{
				RefreshNavigation(config);
			}
			catch (Exception ex)
			{
				Tenant.LogError(nameof(NavigationService), ex.Message, LogCategories.Navigation);
			}
		}

		private void FillDescriptor(NavigationHandlerDescriptor descriptor, ISiteMapContainer container)
		{
			foreach (var item in container.Routes)
			{
				if (container is ISiteMapRouteContainer routeContainer)
				{
					if (!string.IsNullOrWhiteSpace(routeContainer.RouteKey) && !descriptor.RouteKeys.Contains(routeContainer.RouteKey.ToLowerInvariant()))
						descriptor.RouteKeys.Add(routeContainer.RouteKey.ToLowerInvariant());

					if (!string.IsNullOrWhiteSpace(routeContainer.Template) && !descriptor.Templates.Contains(routeContainer.Template.ToLowerInvariant()))
						descriptor.Templates.Add(routeContainer.Template);
				}

				FillDescriptor(descriptor, item);
			}
		}

		private void FillDescriptor(NavigationHandlerDescriptor descriptor, ISiteMapRoute route)
		{
			if (!string.IsNullOrWhiteSpace(route.RouteKey) && !descriptor.RouteKeys.Contains(route.RouteKey.ToLowerInvariant()))
				descriptor.RouteKeys.Add(route.RouteKey.ToLowerInvariant());

			if (!string.IsNullOrWhiteSpace(route.Template) && !descriptor.Templates.Contains(route.Template.ToLowerInvariant()))
				descriptor.Templates.Add(route.Template.ToLowerInvariant());

			foreach (var item in route.Routes)
			{
				if (!string.IsNullOrWhiteSpace(item.RouteKey) && !descriptor.RouteKeys.Contains(item.RouteKey.ToLowerInvariant()))
					descriptor.RouteKeys.Add(item.RouteKey.ToLowerInvariant());

				if (!string.IsNullOrWhiteSpace(item.Template) && !descriptor.Templates.Contains(item.Template.ToLowerInvariant()))
					descriptor.Templates.Add(item.Template.ToLowerInvariant());

				FillDescriptor(descriptor, item);
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

		private void ProcessBreadcrumb(ISiteMapElement item, List<IBreadcrumb> items, RouteValueDictionary parameters)
		{
			var route = item as ISiteMapRoute;

			var breadCrumb = new Breadcrumb
			{
				Text = item.Text
			};

			if (route != null)
				breadCrumb.Key = route.Template;
			/*
			 * Last breadcrumb should be without a link 
			 * because it points to a currently displayed ui
			 */
			if (items.Count > 0 && route != null && !string.IsNullOrWhiteSpace(route.Template))
				breadCrumb.Url = ParseUrl(route.Template, parameters);

			if (item is ISiteMapContainer container)
			{
				if (!string.IsNullOrWhiteSpace(container.SpeculativeRouteKey))
				{
					var speculativeRoute = SelectRoute(container.SpeculativeRouteKey);

					if (speculativeRoute != null)
						breadCrumb.Url = ParseUrl(speculativeRoute.Template, parameters);
				}
				else if (container is ISiteMapRouteContainer routeContainer && !string.IsNullOrWhiteSpace(routeContainer.Template))
					breadCrumb.Url = ParseUrl(routeContainer.Template, parameters);
			}

			if (item.Visible)
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
				ProcessBreadcrumb(parent, items, parameters);
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

		public ISiteMapRoute MatchRoute(string url, RouteValueDictionary parameters)
		{
			Initialize();

			foreach (var handler in Handlers)
			{
				foreach (var descriptor in handler.Value)
				{
					var template = descriptor.Match(url, parameters);

					if (template != null)
						return SelectRouteByTemplate(descriptor, template);
				}
			}

			return null;
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

		private ISiteMapRoute SelectRouteByTemplate(NavigationHandlerDescriptor descriptor, string template)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(descriptor.MicroService);
			var handler = Tenant.GetService<ICompilerService>().CreateInstance<ISiteMapHandler>(new MicroServiceContext(ms, Tenant.Url), descriptor.Handler);

			if (handler == null)
				return null;

			var containers = handler.Invoke();

			if (containers == null)
				return null;

			foreach (var container in containers)
				BindContext(container, handler.Context);

			foreach (var container in containers)
			{
				if (container is ISiteMapRouteContainer routeContainer && string.Compare(routeContainer.Template, template, true) == 0)
				{
					return new SiteMapRoute
					{
						RouteKey = routeContainer.RouteKey,
						Text = routeContainer.Text,
						Template = routeContainer.Template
					};
				}

				var r = SelectRouteByTemplate(container, template);

				if (r != null)
					return r;
			}

			return null;
		}

		private ISiteMapRoute SelectRoute(NavigationHandlerDescriptor descriptor, string routeKey)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(descriptor.MicroService);
			var handler = Tenant.GetService<ICompilerService>().CreateInstance<ISiteMapHandler>(new MicroServiceContext(ms, Tenant.Url), descriptor.Handler);

			if (handler == null)
				return null;

			var containers = handler.Invoke();

			if (containers == null)
				return null;

			foreach (var container in containers)
				BindContext(container, handler.Context);

			foreach (var container in containers)
			{
				if (container is ISiteMapRouteContainer routeContainer && string.Compare(routeContainer.RouteKey, routeKey, true) == 0)
				{
					return new SiteMapRoute
					{
						RouteKey = routeKey,
						Text = routeContainer.Text,
						Template = routeContainer.Template
					};
				}

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

		private ISiteMapRoute SelectRouteByTemplate(ISiteMapContainer container, string template)
		{
			foreach (var item in container.Routes)
			{
				if (string.Compare(item.Template, template, true) == 0)
					return item;

				var r = SelectRouteByTemplate(item, template);

				if (r != null)
					return r;
			}

			return null;
		}

		private ISiteMapRoute SelectRouteByTemplate(ISiteMapRoute route, string template)
		{
			foreach (var item in route.Routes)
			{
				if (string.Compare(item.Template, template, true) == 0)
					return item;

				var r = SelectRoute(item, template);

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
