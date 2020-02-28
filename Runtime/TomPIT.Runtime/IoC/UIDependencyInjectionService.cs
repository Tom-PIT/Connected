using System.Collections.Generic;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Connectivity;
using TomPIT.Serialization;
using TomPIT.Services;

namespace TomPIT.IoC
{
	internal class UIDependencyInjectionService : ConfigurationRepository<IUIDependencyInjectionConfiguration>, IUIDependencyInjectionService
	{
		public UIDependencyInjectionService(ITenant tenant) : base(tenant, "uidependency")
		{
		}

		protected override string[] Categories => new string[] { ComponentCategories.UIDependencyInjection };

		public List<IUIDependencyDescriptor> QueryPartialDependencies(string partial, object arguments)
		{
			if (string.IsNullOrWhiteSpace(partial))
				return null;

			var targets = new List<IPartialDependency>();

			foreach (var config in All())
			{
				foreach (var dependency in config.Injections)
				{
					if (string.IsNullOrWhiteSpace(dependency.Name))
						continue;

					if (dependency is IPartialDependency partialDependency)
					{
						if (string.Compare(partialDependency.Partial, partial, true) == 0)
							targets.Add(partialDependency);
					}
				}
			}

			return CreateDependencies(targets.ToList<IUIDependency>(), arguments);
		}

		public List<IUIDependencyDescriptor> QueryViewDependencies(string view, object arguments)
		{
			if (string.IsNullOrWhiteSpace(view))
				return null;

			var targets = new List<IViewDependency>();

			foreach (var config in All())
			{
				foreach (var dependency in config.Injections)
				{
					if (string.IsNullOrWhiteSpace(dependency.Name))
						continue;

					if (dependency is IViewDependency viewDependency)
					{
						if (string.Compare(viewDependency.View, view, true) == 0)
							targets.Add(viewDependency);
					}
				}
			}

			return CreateDependencies(targets.ToList<IUIDependency>(), arguments);
		}

		public List<IUIDependencyDescriptor> QueryMasterDependencies(string master, object arguments, MasterDependencyKind kind)
		{
			var targets = new List<IMasterDependency>();

			foreach (var config in All())
			{
				foreach (var dependency in config.Injections)
				{
					if (string.IsNullOrWhiteSpace(dependency.Name))
						continue;

					if (dependency is IMasterDependency masterDependency && masterDependency.Kind == kind)
					{
						if (string.IsNullOrWhiteSpace(master) || string.Compare(masterDependency.Master, master, true) == 0)
							targets.Add(masterDependency);
					}
				}
			}

			return CreateDependencies(targets.ToList<IUIDependency>(), arguments);
		}

		private List<IUIDependencyDescriptor> CreateDependencies(List<IUIDependency> dependencies, object arguments)
		{
			if (dependencies.Count == 0)
				return null;

			var result = new List<IUIDependencyDescriptor>();

			foreach (var dependency in dependencies)
			{
				var scriptType = Tenant.GetService<ICompilerService>().ResolveType(dependency.Configuration().MicroService(), dependency, dependency.Name, false);

				if (scriptType == null)
					continue;

				var args = arguments == null ? null : Serializer.Serialize(arguments);
				var middleware = Tenant.GetService<ICompilerService>().CreateInstance<IUIDependencyInjectionMiddleware>(dependency, args, scriptType.Name);

				if (middleware == null)
					continue;

				var items = middleware.Invoke();

				if (items != null)
					result.AddRange(items);
			}

			return result;

		}
	}
}
