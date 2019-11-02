using System;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Services
{
	public abstract class ConfigurationRepository<T> : SynchronizedClientRepository<T, Guid> where T : class, IConfiguration
	{
		public ConfigurationRepository(ITenant tenant, string cacheKey) : base(tenant, cacheKey)
		{
			Tenant.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
			Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			Tenant.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			Tenant.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;
			Tenant.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
		}

		protected abstract string[] Categories { get; }

		protected virtual void OnAdded(Guid microService, Guid component)
		{

		}

		protected virtual void OnChanged(Guid microService, Guid component)
		{

		}

		protected virtual void OnRemoved(Guid microService, Guid component)
		{

		}

		private void OnComponentChanged(ITenant sender, ComponentEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Refresh(e.Component);
			OnChanged(e.MicroService, e.Component);
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			if (!Tenant.IsMicroServiceSupported(e.MicroService))
				return;

			var views = Tenant.GetService<IComponentService>().QueryConfigurations(e.MicroService, string.Join(',', Categories));

			foreach (var i in views)
			{
				Set(i.Component, i as T, TimeSpan.Zero);
				OnChanged(e.MicroService, i.Component);
			}
		}

		private void OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Remove(e.Component);
			OnRemoved(e.MicroService, e.Component);
		}

		private void OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Refresh(e.Component);
			OnAdded(e.MicroService, e.Component);
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Refresh(e.Component);
			OnChanged(e.MicroService, e.Component);
		}

		private bool IsTargetCategory(string category)
		{
			foreach (var targetCategory in Categories)
			{
				if (string.Compare(targetCategory, category, true) == 0)
					return true;
			}

			return false;
		}

		protected override void OnInitializing()
		{
			var configurations = Tenant.GetService<IComponentService>().QueryConfigurations(Shell.GetConfiguration<IClientSys>().ResourceGroups, string.Join(',', Categories));

			foreach (var i in configurations)
			{
				if (i == null)
					continue;

				Set(i.Component, i as T, TimeSpan.Zero);
			}
		}

		protected override void OnInvalidate(Guid id)
		{
			if (Tenant.GetService<IComponentService>().SelectConfiguration(id) is T config)
			{
				Set(config.Component, config, TimeSpan.Zero);
				//				OnAdded(config.MicroService(), id);
			}
			else
			{
				var existing = Get(id);

				Remove(id);
				//			OnRemoved(existing == null ? Guid.Empty : existing.MicroService(), id);
			}
		}
	}
}
