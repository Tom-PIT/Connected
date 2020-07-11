using System;
using TomPIT.Caching;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Runtime;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Runtime
{
	internal class MicroServiceRuntimeService : SynchronizedClientRepository<IRuntimeMiddleware, Guid>, IMicroServiceRuntimeService
	{
		public MicroServiceRuntimeService(ITenant tenant) : base(tenant, "runtimeMiddleware")
		{
			Tenant.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
			Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			Tenant.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			Tenant.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;

			Initialize();
		}

		protected override void OnInitializing()
		{
			var configurations = Tenant.GetService<IComponentService>().QueryConfigurations(Shell.GetConfiguration<IClientSys>().ResourceGroups, ComponentCategories.Runtime);

			foreach (var i in configurations)
				LoadRuntime(i as IRuntimeConfiguration);
		}

		protected override void OnInvalidate(Guid id)
		{
			LoadRuntime(Tenant.GetService<IComponentService>().SelectConfiguration(id) as IRuntimeConfiguration);
		}
		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Runtime, true) != 0)
				return;

			Refresh(e.Component);
		}

		private void OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Runtime, true) != 0)
				return;

			Refresh(e.Component);
		}

		private void OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Runtime, true) != 0)
				return;

			Remove(e.Component);
		}

		private void OnComponentChanged(ITenant sender, ComponentEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Runtime, true) != 0)
				return;

			Refresh(e.Component);
		}

		private void LoadRuntime(IRuntimeConfiguration config)
		{
			if (config == null)
				return;

			var type = Tenant.GetService<ICompilerService>().ResolveType(config.MicroService(), config, config.ComponentName(), false);

			if (type == null)
				return;

			var instance = Tenant.GetService<ICompilerService>().CreateInstance<IRuntimeMiddleware>(new MicroServiceContext(config.MicroService(), Tenant.Url), type);

			if (instance != null)
			{
				instance.Initialize(new RuntimeInitializeArgs(RuntimeService._host));
				Set(config.Component, instance, TimeSpan.Zero);
			}
		}
	}
}
