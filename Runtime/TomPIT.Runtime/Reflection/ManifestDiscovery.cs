using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Reflection
{
	internal class ManifestDiscovery : ClientRepository<IComponentManifest, Guid>, IManifestDiscovery, IManifestDiscoveryNotification
	{
		private ScriptManifestCache _cache;
		public ManifestDiscovery(ITenant tenant) : base(tenant, "manifest")
		{
			Tenant.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
			Tenant.GetService<IComponentService>().ComponentRemoved += OnComponentRemoved;
			Tenant.GetService<IComponentService>().ComponentAdded += OnComponentAdded;
			Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;

			_cache = new ScriptManifestCache(Tenant);
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			Remove(e.Component);
		}

		private void OnComponentAdded(ITenant sender, ComponentEventArgs e)
		{
			Remove(e.Component);
		}

		private void OnComponentRemoved(ITenant sender, ComponentEventArgs e)
		{
			Remove(e.Component);
		}

		private void OnComponentChanged(ITenant sender, ComponentEventArgs e)
		{
			Remove(e.Component);
		}

		public IComponentManifest Select(Guid component)
		{
			return Get(component, (f) =>
				{
					var c = Tenant.GetService<IComponentService>().SelectComponent(component);

					if (c == null)
						return null;

					var ms = Tenant.GetService<IMicroServiceService>().Select(c.MicroService);

					if (ms == null)
						return null;

					var result = c.Manifest();

					Set(component, result, TimeSpan.Zero);

					return result;
				});
		}

		public IComponentManifest Select(string microService, string category, string componentName)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			var component = Tenant.GetService<IComponentService>().SelectComponent(ms.Token, category, componentName);

			if (component == null)
				return null;

			return Select(component.Token);
		}

		public ImmutableList<IComponentManifest> Query(Guid microService)
		{
			var components = Tenant.GetService<IComponentService>().QueryComponents(microService);
			var result = ImmutableList<IComponentManifest>.Empty;

			foreach (var component in components)
			{
				var manifest = Select(component.Token);

				if (manifest != null)
					result.Add(manifest);
			}

			return result;
		}

		public void Invalidate(Guid microService, Guid component, Guid script)
		{
			ManifestCache.Rebuild(microService, component, script);
		}

		public void NotifyChanged(Guid microService, Guid component, Guid script)
		{
			ManifestCache.RemoveManifest(script);
		}

		public IScriptManifest SelectScript(Guid microService, Guid component, Guid id)
		{
			return ManifestCache.Select(microService, component, id);
		}

		public IManifestTypeResolver SelectTypeResolver(IManifestMiddleware manifest)
		{
			if (manifest.Address == null)
				return null;

			return SelectTypeResolver(manifest.Address.MicroService, manifest.Address.Component, manifest.Address.Element);
		}

		public IManifestTypeResolver SelectTypeResolver(Guid microService, Guid component, Guid script)
		{
			return new ManifestTypeResolver(Tenant, microService, component, script);
		}

		public IImmutableList<IScriptManifest> QueryReferences(IScriptManifest e)
		{
			return ManifestCache.QueryReferences(e);
		}

		private ScriptManifestCache ManifestCache => _cache;
	}
}
