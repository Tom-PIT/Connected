using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Services
{
	public abstract class ConfigurationAwareService<T> : SynchronizedClientRepository<T, Guid> where T : class, IConfiguration
	{
		public ConfigurationAwareService(ISysConnection connection, string cacheKey) : base(connection, cacheKey)
		{
			connection.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
			connection.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			connection.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;
			connection.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;
			connection.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
		}

		protected abstract string[] Categories { get; }

		protected virtual void OnAdded(Guid component)
		{

		}

		protected virtual void OnChanged(Guid component)
		{

		}

		protected virtual void OnRemoved(Guid component)
		{

		}

		private void OnComponentChanged(ISysConnection sender, ComponentEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Refresh(e.Component);
			OnChanged(e.Component);
		}

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			if (!Connection.IsMicroServiceSupported(e.MicroService))
				return;

			var views = Connection.GetService<IComponentService>().QueryConfigurations(e.MicroService, string.Join(',', Categories));

			foreach (var i in views)
			{
				Set(i.Component, i as T, TimeSpan.Zero);
				OnChanged(i.Component);
			}
		}

		private void OnConfigurationRemoved(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Remove(e.Component);
			OnRemoved(e.Component);
		}

		private void OnConfigurationAdded(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Refresh(e.Component);
			OnAdded(e.Component);
		}

		private void OnConfigurationChanged(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (!IsTargetCategory(e.Category))
				return;

			Refresh(e.Component);
			OnChanged(e.Component);
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
			var configurations = Connection.GetService<IComponentService>().QueryConfigurations(Shell.GetConfiguration<IClientSys>().ResourceGroups, string.Join(',', Categories));

			foreach (var i in configurations)
			{
				if (i == null)
					continue;

				Set(i.Component, i as T, TimeSpan.Zero);
				OnChanged(i.Component);
			}
		}

		protected override void OnInvalidate(Guid id)
		{
			if (Connection.GetService<IComponentService>().SelectConfiguration(id) is T config)
			{
				Set(config.Component, config, TimeSpan.Zero);
				OnChanged(id);
			}
			else
			{
				Remove(id);
				OnRemoved(id);
			}
		}
	}
}
