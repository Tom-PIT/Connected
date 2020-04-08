using System.Collections.Generic;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Connectivity;
using TomPIT.Serialization;
using TomPIT.Services;

namespace TomPIT.IoC
{
	internal class DependencyInjectionService : ConfigurationRepository<IDependencyInjectionConfiguration>, IDependencyInjectionService
	{
		public DependencyInjectionService(ITenant tenant) : base(tenant, "dependencyconfig")
		{
		}

		protected override string[] Categories => new string[] { ComponentCategories.DependencyInjection };

		public List<IApiDependencyInjectionObject> QueryApiDependencies(string api, object arguments)
		{
			if (string.IsNullOrWhiteSpace(api))
				return null;

			var targets = new List<IApiDependency>();

			foreach (var config in All())
			{
				foreach (var dependency in config.Injections)
				{
					if (string.IsNullOrWhiteSpace(dependency.Name))
						continue;

					if (dependency is IApiDependency apiDependency)
					{
						if (string.Compare(apiDependency.Operation, api, true) == 0)
							targets.Add(apiDependency);
					}
				}
			}

			return CreateDependencies<IApiDependencyInjectionObject, IApiDependency>(targets, arguments);
		}

		public List<ISearchDependencyInjectionMiddleware> QuerySearchDependencies(string catalog, object arguments)
		{
			if (string.IsNullOrWhiteSpace(catalog))
				return null;

			var targets = new List<ISearchDependency>();

			foreach (var config in All())
			{
				foreach (var dependency in config.Injections)
				{
					if (string.IsNullOrWhiteSpace(dependency.Name))
						continue;

					if (dependency is ISearchDependency searchDependency)
					{
						if (string.Compare(searchDependency.Catalog, catalog, true) == 0)
							targets.Add(searchDependency);
					}
				}
			}

			return CreateDependencies<ISearchDependencyInjectionMiddleware, ISearchDependency>(targets, arguments);
		}

		public List<ISubscriptionDependencyInjectionMiddleware> QuerySubscriptionDependencies(string subscription, object arguments)
		{
			if (string.IsNullOrWhiteSpace(subscription))
				return null;

			var targets = new List<ISubscriptionDependency>();

			foreach (var config in All())
			{
				foreach (var dependency in config.Injections)
				{
					if (string.IsNullOrWhiteSpace(dependency.Name))
						continue;

					if (dependency is ISubscriptionDependency subDependency)
					{
						if (string.Compare(subDependency.Subscription, subscription, true) == 0)
							targets.Add(subDependency);
					}
				}
			}

			return CreateDependencies<ISubscriptionDependencyInjectionMiddleware, ISubscriptionDependency>(targets, arguments);
		}

		public List<ISubscriptionEventDependencyInjectionMiddleware> QuerySubscriptionEventDependencies(string subscriptionEvent, object arguments)
		{
			if (string.IsNullOrWhiteSpace(subscriptionEvent))
				return null;

			var targets = new List<ISubscriptionEventDependency>();

			foreach (var config in All())
			{
				foreach (var dependency in config.Injections)
				{
					if (string.IsNullOrWhiteSpace(dependency.Name))
						continue;

					if (dependency is ISubscriptionEventDependency subEventDependency)
					{
						if (string.Compare(subEventDependency.Event, subscriptionEvent, true) == 0)
							targets.Add(subEventDependency);
					}
				}
			}

			return CreateDependencies<ISubscriptionEventDependencyInjectionMiddleware, ISubscriptionEventDependency>(targets, arguments);
		}

		private List<R> CreateDependencies<R, C>(List<C> dependencies, object arguments) where R : class where C : IDependency
		{
			if (dependencies.Count == 0)
				return null;

			var result = new List<R>();

			foreach (var dependency in dependencies)
			{
				var scriptType = Tenant.GetService<ICompilerService>().ResolveType(dependency.Configuration().MicroService(), dependency, dependency.Name, false);

				if (scriptType == null)
					continue;

				var args = arguments == null ? null : Serializer.Serialize(arguments);
				var middleware = Tenant.GetService<ICompilerService>().CreateInstance<R>(dependency, args, scriptType.Name);

				if (middleware == null)
					continue;

				result.Add(middleware);
			}

			return result;
		}
	}
}
