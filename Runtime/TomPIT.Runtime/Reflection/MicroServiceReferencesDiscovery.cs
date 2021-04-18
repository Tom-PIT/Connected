using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Reflection
{
	internal class MicroServiceReferencesDiscovery : TenantObject, IMicroServiceReferencesDiscovery
	{
		private readonly MicroServiceDependencyGraph _dependencyGraph;

		public MicroServiceReferencesDiscovery(ITenant tenant) : base(tenant)
		{
			_dependencyGraph = new MicroServiceDependencyGraph(Tenant);
		}

		public ImmutableArray<IMicroService> References(Guid microService, bool recursive)
		{
			return DependencyGraph.References(microService, recursive);
		}

		public ImmutableArray<IMicroService> Flatten(Guid microService)
		{
			return References(microService, true);
		}

		public IServiceReferencesConfiguration Select(string microService)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			var component = Tenant.GetService<IComponentService>().SelectComponent(ms.Token, ComponentCategories.Reference, ComponentCategories.ReferenceComponentName);

			if (component == null)
				return null;

			return Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as IServiceReferencesConfiguration;
		}

		public IServiceReferencesConfiguration Select(Guid microService)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			return Select(ms.Name);
		}

		public ImmutableArray<IMicroService> ReferencedBy(Guid microService, bool recursive)
		{
			return DependencyGraph.ReferencedBy(microService, recursive);
		}

		private MicroServiceDependencyGraph DependencyGraph => _dependencyGraph;
	}
}
