using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.Serialization;
using TomPIT.Reflection.Manifests;
using TomPIT.Reflection.Manifests.Entities;
using TomPIT.Storage;

namespace TomPIT.Reflection
{
	internal class ManifestDiscovery : ClientRepository<IComponentManifest, Guid>, IManifestDiscovery
	{
		public ManifestDiscovery(ITenant tenant) : base(tenant, "manifest")
		{
			Tenant.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
			Tenant.GetService<IComponentService>().ComponentRemoved += OnComponentRemoved;
			Tenant.GetService<IComponentService>().ComponentAdded += OnComponentAdded;
			Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
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
			return Get(component,
	(f) =>
	{
		var c = Tenant.GetService<IComponentService>().SelectComponent(component);

		if (c == null)
			return null;

		var ms = Tenant.GetService<IMicroServiceService>().Select(c.MicroService);

		if (ms == null)
			return null;

		var existing = Tenant.GetService<IStorageService>().Download(ms.Token, BlobTypes.Manifest, ms.ResourceGroup, $"manifest{component}");
		IComponentManifest result = null;

		if (existing == null)
		{
			result = c.Manifest();
			SaveManifest(c, result);
		}
		else
		{
			try
			{
				result = Tenant.GetService<ISerializationService>().Deserialize(existing.Content, typeof(ComponentManifest)) as IComponentManifest;
			}
			catch
			{
				result = c.Manifest();
				SaveManifest(c, result);
			}
		}

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

		private void SaveManifest(IComponent component, IComponentManifest manifest)
		{
			Tenant.GetService<IStorageService>().Upload(new Blob
			{
				ContentType = Blob.ContentTypeJson,
				FileName = $"{manifest.Name}.json",
				MicroService = component.MicroService,
				PrimaryKey = $"manifest{component.Token}",
				Type = BlobTypes.Manifest,
				ResourceGroup = component.ResourceGroup()
			}, Tenant.GetService<ISerializationService>().Serialize(manifest), StoragePolicy.Singleton);
		}
	}
}
