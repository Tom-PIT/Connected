using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using TomPIT.Annotations;
using TomPIT.Collections;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Navigation;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Navigation
{
	internal class NavigationService : ConfigurationRepository<ISiteMapConfiguration>, INavigationService
	{
		private static Lazy<ConcurrentDictionary<string, List<NavigationHandlerDescriptor>>> _handlers = new Lazy<ConcurrentDictionary<string, List<NavigationHandlerDescriptor>>>(() => { return new ConcurrentDictionary<string, List<NavigationHandlerDescriptor>>(StringComparer.OrdinalIgnoreCase); });
		private static Lazy<ConcurrentDictionary<string, Guid>> _contextPointers = new Lazy<ConcurrentDictionary<string, Guid>>();

		private static readonly HashSet<string> ReservedParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			".",
			"action",
			"controller"
		};
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
			foreach (var navigation in All())
				OnAdded(navigation.MicroService(), navigation.Component);
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

				container.Routes.Clear();
			}

			Site(r);

			return r;
		}

		private List<ISiteMapContainer> LoadSiteMap(string key, List<string> tags)
		{
			var r = new List<ISiteMapContainer>();

			foreach (var handler in Handlers)
			{
				if (string.Compare(handler.Key, key, true) == 0)
				{
					var items = LoadContainerRoutes(handler.Value, key, tags);

					if (items != null && items.Count > 0)
						r.AddRange(items);
				}
				else
				{
					foreach (var descriptor in handler.Value)
					{
						if (!descriptor.RouteKeys.Contains(key.ToLowerInvariant()))
							continue;

						var items = new List<ISiteMapContainer>();

						LoadDescriptorRoutes(descriptor, key, null, items);

						foreach (var item in items)
						{
							FilterContainer(item, key);

							r.Add(item);
						}
					}
				}
			}

			if (r.Count < 2)
				return r;

			var first = r[0];

			for (var i = 1; i < r.Count; i++)
				first.Routes.AddRange(r[i].Routes);

			return new List<ISiteMapContainer> { first };
		}

		private void FilterContainer(ISiteMapContainer container, string routeKey)
		{
			var targetRoute = FindRoute(container.Routes, routeKey);

			container.Routes.Clear();

			if (targetRoute != null)
			{
				container.Routes.AddRange(targetRoute.Routes);

				targetRoute.Routes.Clear();
			}
		}

		private ISiteMapRoute FindRoute(ConnectedList<ISiteMapRoute, ISiteMapContainer> routes, string routeKey)
		{
			foreach (var route in routes)
			{
				if (string.Compare(route.RouteKey, routeKey, true) == 0)
					return route;

				var childTarget = FindRoute(route.Routes, routeKey);

				if (childTarget != null)
					return childTarget;
			}

			return null;
		}

		private ISiteMapRoute FindRoute(ConnectedList<ISiteMapRoute, ISiteMapRoute> routes, string routeKey)
		{
			foreach (var route in routes)
			{
				if (string.Compare(route.RouteKey, routeKey, true) == 0)
					return route;

				var childTarget = FindRoute(route.Routes, routeKey);

				if (childTarget != null)
					return childTarget;
			}

			return null;
		}

		private List<ISiteMapContainer> LoadContainerRoutes(List<NavigationHandlerDescriptor> handlers, string key, List<string> tags)
		{
			var r = new List<ISiteMapContainer>();

			foreach (var descriptor in handlers)
				LoadDescriptorRoutes(descriptor, key, tags, r);

			return r;
		}

		private void LoadDescriptorRoutes(NavigationHandlerDescriptor handler, string key, List<string> tags, List<ISiteMapContainer> items)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(handler.MicroService);
			var ctx = new MicroServiceContext(ms, Tenant.Url);
			var instance = Tenant.GetService<ICompilerService>().CreateInstance<ISiteMapMiddleware>(ctx, handler.Handler);
			var containers = instance.Invoke(key);

			if (containers == null || containers.Count == 0)
				return;

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
				Site(container);

			items.AddRange(containers);
		}

		private void Site(ISiteMapContainer container)
		{
			foreach (var item in container.Routes)
			{
				ReflectionExtensions.SetPropertyValue(item, nameof(item.Parent), container);

				Site(item);
			}
		}

		private void Site(ISiteMapRoute route)
		{
			foreach (var item in route.Routes)
			{
				if (item.Parent == null)
					ReflectionExtensions.SetPropertyValue(item, nameof(item.Parent), route);

				Site(item);
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

			if (Tenant.GetService<ICompilerService>().ResolveType(ms, configuration, configuration.ComponentName(), false) is not Type type)
				return;

			if (type.CreateInstance<ISiteMapMiddleware>() is not ISiteMapMiddleware handlerInstance)
				return;

			using var ctx = configuration.CreateContext();
			handlerInstance.SetContext(ctx);

			var containers = handlerInstance.Invoke();

			RegisterHandlers(ms, type, configuration, containers);
			RegisterNavigationContexts(configuration.Component, handlerInstance.QueryContexts());
		}

		private void RegisterHandlers(Guid microService, Type handlerType, ISiteMapConfiguration configuration, List<ISiteMapContainer> containers)
		{
			if (containers is null)
				return;

			foreach (var container in containers)
			{
				if (string.IsNullOrWhiteSpace(container.Key))
				{
					MiddlewareDescriptor.Current.Tenant.LogWarning(nameof(NavigationService), $"{SR.WrnContainerKeyNull} ({container.GetType().ShortName()})", LogCategories.Navigation);
					continue;
				}

				Handlers.TryAdd(container.Key, new List<NavigationHandlerDescriptor>());

				if (!Handlers.TryGetValue(container.Key, out List<NavigationHandlerDescriptor> items) || items.ToImmutableList().FirstOrDefault(f => f.Component == configuration.Component) is not null)
					continue;

				var descriptor = new NavigationHandlerDescriptor(microService, configuration.Component, handlerType);

				FillDescriptor(descriptor, container);

				if (Handlers.TryGetValue(container.Key, out items))
				{
					lock (items)
						items.Add(descriptor);
				}
			}
		}

		private void RegisterNavigationContexts(Guid component, List<INavigationContext> items)
		{
			if (items is null)
				return;

			foreach (var context in items)
			{
				if (string.IsNullOrWhiteSpace(context.Key))
					continue;

				ContextPointers.TryAdd(context.Key, component);
			}
		}

		private void RefreshNavigation(Guid component, bool removeOny = false)
		{
			foreach (var handler in Handlers)
			{
				foreach (var item in handler.Value.Where(f => f.Component == component).ToImmutableArray())
					handler.Value.Remove(item);
			}

			foreach (var pointer in ContextPointers.ToImmutableDictionary())
			{
				if (pointer.Value == component)
					ContextPointers.TryRemove(pointer.Key, out _);
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
		public List<IBreadcrumb> QueryBreadcrumbs(string routeKey, RouteValueDictionary parameters, BreadcrumbLinkBehavior linkBehavior)
		{
			Initialize();

			var item = SelectRoute(routeKey);

			if (item == null)
				return null;

			var result = new List<IBreadcrumb>();

			ProcessBreadcrumb(item, result, parameters, linkBehavior);

			return result;
		}
		public List<IBreadcrumb> QueryBreadcrumbs(string routeKey, RouteValueDictionary parameters)
		{
			return QueryBreadcrumbs(routeKey, parameters, BreadcrumbLinkBehavior.IgnoreLastRoute);
		}

		private void ProcessBreadcrumb(ISiteMapElement item, List<IBreadcrumb> items, RouteValueDictionary parameters, BreadcrumbLinkBehavior linkBehavior)
		{
			Breadcrumb breadCrumb = null;

			if (item is ISiteMapRoute route)
				breadCrumb = ProcessRouteBreadcrumb(route, item, items, parameters, linkBehavior);
			else if (item is ISiteMapRouteContainer routeContainer)
				breadCrumb = ProcessContainerBreadcrumb(routeContainer, item, items, parameters, linkBehavior);
			else
				return;

			if (item is ISiteMapContainer container)
			{
				if (!string.IsNullOrWhiteSpace(container.SpeculativeRouteKey))
				{
					if (SelectRoute(container.SpeculativeRouteKey) is ISiteMapRoute speculativeRoute)
						breadCrumb.Url = speculativeRoute.WithNavigationContext(ParseUrl(speculativeRoute.Template, MergeParameters(parameters, item)));
				}
				else if (container is ISiteMapRouteContainer routeContainer && !string.IsNullOrWhiteSpace(routeContainer.Template))
					breadCrumb.Url = routeContainer.WithNavigationContext(ParseUrl(routeContainer.Template, MergeParameters(parameters, item)));
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
				ProcessBreadcrumb(parent, items, parameters, linkBehavior);
		}

		private Breadcrumb ProcessRouteBreadcrumb(ISiteMapRoute route, ISiteMapElement item, List<IBreadcrumb> items, RouteValueDictionary parameters, BreadcrumbLinkBehavior linkBehavior)
		{
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
			if (linkBehavior == BreadcrumbLinkBehavior.All || (items.Count > 0 && route != null && !string.IsNullOrWhiteSpace(route.Template)))
				breadCrumb.Url = route.WithNavigationContext(ParseUrl(route.Template, MergeParameters(parameters, item)));

			return breadCrumb;
		}

		private Breadcrumb ProcessContainerBreadcrumb(ISiteMapRouteContainer route, ISiteMapElement item, List<IBreadcrumb> items, RouteValueDictionary parameters, BreadcrumbLinkBehavior linkBehavior)
		{
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
			if (linkBehavior == BreadcrumbLinkBehavior.All || (items.Count > 0 && route != null && !string.IsNullOrWhiteSpace(route.Template)))
				breadCrumb.Url = route.WithNavigationContext(ParseUrl(route.Template, MergeParameters(parameters, item)));

			return breadCrumb;
		}

		private RouteValueDictionary MergeParameters(RouteValueDictionary parameters, ISiteMapElement element)
		{
			var origin = parameters;

			if (origin == null)
			{
				if (Shell.HttpContext != null)
					origin = Shell.HttpContext.GetRouteData().Values;
				else
					origin = new RouteValueDictionary();
			}

			origin = origin.Merge(element);

			CleanParameters(origin);

			return origin;
		}
		public string ParseUrl(string template, RouteValueDictionary parameters)
		{
			return ParseUrl(template, parameters, false);
		}
		public string ParseUrl(string template, RouteValueDictionary parameters, bool allowQueryString)
		{
			if (parameters == null)
			{
				if (Shell.HttpContext != null)
					parameters = Shell.HttpContext.GetRouteData().Values;
				else
					parameters = new RouteValueDictionary();
			}

			CleanParameters(parameters);

			var parsedTemplate = TemplateParser.Parse(template);
			var processedSegments = new List<string>();
			var usedParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
						{
							usedParameters.Add(part.Name);
							processedSegments.Add(Types.Convert<string>(parameters[part.Name]));
						}
					}
				}

				if (incomplete)
					break;
			}

			using var ctx = new MiddlewareContext();

			if (allowQueryString)
				return $"{ctx.Services.Routing.RootUrl}/{string.Join('/', processedSegments)}{ParseQueryString(parameters, usedParameters)}";
			else
				return $"{ctx.Services.Routing.RootUrl}/{string.Join('/', processedSegments)}";
		}

		private static string ParseQueryString(RouteValueDictionary parameters, HashSet<string> usedParameters)
		{
			if (!parameters.Any() || usedParameters.Count >= parameters.Count)
				return string.Empty;

			var queryBuilder = new StringBuilder();

			foreach (var parameter in parameters)
			{
				if (ReservedParameters.Contains(parameter.Key))
					continue;

				if (!usedParameters.Contains(parameter.Key))
				{
					if (queryBuilder.Length > 0)
						queryBuilder.Append('&');

					queryBuilder.Append($"{HttpUtility.UrlEncode(parameter.Key.ToLowerInvariant())}={HttpUtility.UrlEncode(Types.Convert<string>(parameter.Value))}");
				}
			}

			if (queryBuilder.Length > 0)
				queryBuilder.Insert(0, '?');

			return queryBuilder.ToString();
		}

		public ISiteMapRoute MatchRoute(string url, RouteValueDictionary parameters)
		{
			Initialize();

			CleanParameters(parameters);

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
			var ctx = new MicroServiceContext(ms, Tenant.Url);
			var handler = Tenant.GetService<ICompilerService>().CreateInstance<ISiteMapMiddleware>(ctx, descriptor.Handler);

			if (handler == null)
				return null;

			var containers = handler.Invoke();

			if (containers == null)
				return null;

			foreach (var container in containers)
				Site(container);

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
			var ctx = new MicroServiceContext(ms, Tenant.Url);
			var handler = Tenant.GetService<ICompilerService>().CreateInstance<ISiteMapMiddleware>(ctx, descriptor.Handler);

			if (handler == null)
				return null;

			var containers = handler.Invoke();

			if (containers == null)
				return null;

			foreach (var container in containers)
				Site(container);

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

		private static void CleanParameters(RouteValueDictionary parameters)
		{
			if (parameters == null || !parameters.Any())
				return;

			var toRemove = new List<KeyValuePair<string, object>>();
			var toChange = new List<KeyValuePair<string, object>>();

			foreach (var parameter in parameters)
			{
				if (parameter.Value is INullableProperty nullable)
				{
					if (nullable.IsNull)
						toRemove.Add(parameter);
					else
						toChange.Add(parameter);
				}
			}

			foreach (var parameter in toRemove)
				parameters.Remove(parameter.Key);

			foreach (var parameter in toChange)
			{
				parameters.Remove(parameter.Key);
				parameters.Add(parameter.Key, ((INullableProperty)parameter.Value).MappedValue);
			}
		}

		public INavigationContext SelectNavigationContext(string key)
		{
			Initialize();

			if (!ContextPointers.TryGetValue(key, out Guid component))
				return null;

			if (Tenant.GetService<IComponentService>().SelectConfiguration(component) is not ISiteMapConfiguration configuration)
				return null;

			using var context = new MicroServiceContext(configuration.MicroService());

			if (configuration.Middleware(context) is not Type middlewareType)
				return null;

			if (context.CreateMiddleware<ISiteMapMiddleware>(middlewareType) is not ISiteMapMiddleware middleware)
				return null;

			return middleware.QueryContexts().First(f => string.Compare(f.Key, key, true) == 0);
		}

		private ConcurrentDictionary<string, Guid> ContextPointers => _contextPointers.Value;
	}
}
