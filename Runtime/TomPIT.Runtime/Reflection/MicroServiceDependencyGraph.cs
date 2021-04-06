using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Data;

namespace TomPIT.Reflection
{
	internal class MicroServiceDependencyGraph : DependencyGraph<Guid, IMicroService>
	{
		public MicroServiceDependencyGraph(ITenant tenant) : base(tenant)
		{
			tenant.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			tenant.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;
		}
		private void OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Reference, true) != 0)
				return;

			Remove(e.Component);
		}

		private void OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Reference, true) != 0)
				return;

			Set(e.Component);
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Reference, true) != 0)
				return;

			Set(e.Component);
		}

		protected override void OnInitialize()
		{
			var configurations = Tenant.GetService<IComponentService>().QueryConfigurations(ComponentCategories.Reference);

			foreach (var configuration in configurations)
			{
				if (configuration is not IServiceReferencesConfiguration references)
					continue;

				if (Tenant.GetService<IMicroServiceService>().Select(configuration.MicroService()) is not IMicroService ms)
					continue;

				Set(ms, references);
			}
		}

		private void Set(Guid microService)
		{
			var configuration = Tenant.GetService<IDiscoveryService>().MicroServices.References.Select(microService);

			if (configuration is null)
			{
				Remove(microService);
				return;
			}

			if (Tenant.GetService<IMicroServiceService>().Select(configuration.MicroService()) is not IMicroService ms)
			{
				Remove(microService);
				return;
			}

			Set(ms, configuration);
		}

		private void Set(IMicroService microService, IServiceReferencesConfiguration configuration)
		{
			var referenceList = new Dictionary<Guid, IMicroService>();

			foreach (var ms in configuration.MicroServices)
			{
				if (Tenant.GetService<IMicroServiceService>().Select(ms.MicroService) is IMicroService referenceMicroService)
					referenceList.Add(referenceMicroService.Token, referenceMicroService);
			}

			Set(microService.Token, microService, referenceList.ToImmutableDictionary());
		}
	}
}