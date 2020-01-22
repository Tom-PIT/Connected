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

		public List<IDependencyInjectionObject> QueryApiDependencies(string api, object arguments)
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

			return CreateDependencies(targets, arguments);
		}

		private List<IDependencyInjectionObject> CreateDependencies(List<IApiDependency> dependencies, object arguments)
		{
			if (dependencies.Count == 0)
				return null;

			var result = new List<IDependencyInjectionObject>();

			foreach (var dependency in dependencies)
			{
				var scriptType = Tenant.GetService<ICompilerService>().ResolveType(dependency.Configuration().MicroService(), dependency, dependency.Name, false);

				if (scriptType == null)
					continue;

				var args = arguments == null ? null : Serializer.Serialize(arguments);
				var middleware = Tenant.GetService<ICompilerService>().CreateInstance<IDependencyInjectionObject>(dependency, args, scriptType.Name);

				if (middleware == null)
					continue;

				result.Add(middleware);
			}

			return result;

		}
	}
}
