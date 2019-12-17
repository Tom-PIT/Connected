using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.Serialization;
using TomPIT.Reflection.Manifests;
using TomPIT.Reflection.Manifests.Entities;
using TomPIT.Storage;

namespace TomPIT.Reflection
{
	internal class DiscoveryService : ClientRepository<IComponentManifest, Guid>, IDiscoveryService
	{
		public DiscoveryService(ITenant tenant) : base(tenant, "manifest")
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

		public IElement Find(IConfiguration configuration, Guid id)
		{
			return Find(configuration, id, new List<object>());
		}

		public IElement Find(Guid component, Guid id)
		{
			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component);

			if (config == null)
				return null;

			return Find(config, id, new List<object>());
		}

		private IElement Find(object instance, Guid id, List<object> referenceTrail)
		{
			if (instance == null)
				return null;

			if (referenceTrail.Contains(instance))
				return null;

			referenceTrail.Add(instance);

			if (instance is IElement el && el.Id == id)
				return el;

			if (instance.GetType().IsCollection())
			{
				if (!(instance is IEnumerable en))
					return null;

				var enm = en.GetEnumerator();

				while (enm.MoveNext())
				{
					if (enm.Current is IElement element && element.Id == id)
						return element;
					else
					{
						var r = Find(enm.Current, id, referenceTrail);

						if (r != null)
							return r;
					}
				}
			}
			else
			{
				var properties = instance.GetType().GetProperties(BindingFlags.Public
					| BindingFlags.Instance);

				foreach (var i in properties)
				{
					var value = i.GetValue(instance);

					if (value == null || value.GetType().IsTypePrimitive())
						continue;

					if (i.FindAttribute<JsonIgnoreAttribute>() != null)
						continue;

					var r = Find(value, id, referenceTrail);

					if (r != null)
						return r;
				}
			}

			return null;
		}

		public IServiceReferencesConfiguration References(Guid microService)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			return References(ms.Name);
		}
		public IServiceReferencesConfiguration References(string microService)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			var component = Tenant.GetService<IComponentService>().SelectComponent(ms.Token, ComponentCategories.Reference, "References");

			if (component == null)
				return null;

			return Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as IServiceReferencesConfiguration;
		}

		public IComponentManifest Manifest(Guid component)
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

		public IComponentManifest Manifest(string microService, string category, string componentName)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			var component = Tenant.GetService<IComponentService>().SelectComponent(ms.Token, category, componentName);

			if (component == null)
				return null;

			return Manifest(component.Token);
		}

		public List<IComponentManifest> Manifests(Guid microService)
		{
			var components = Tenant.GetService<IComponentService>().QueryComponents(microService);
			var result = new List<IComponentManifest>();

			foreach (var component in components)
			{
				var manifest = Manifest(component.Token);

				if (manifest != null)
					result.Add(manifest);
			}

			return result;
		}

		public List<IMicroService> FlattenReferences(Guid microService)
		{
			var r = new List<IMicroService>();

			FlattenReferences(microService, r);

			return r;
		}

		private void FlattenReferences(Guid microService, List<IMicroService> existing)
		{
			var refs = References(microService);

			if (refs == null)
				return;

			foreach (var reference in refs.MicroServices)
			{
				if (string.IsNullOrWhiteSpace(reference.MicroService))
					continue;

				if (existing.FirstOrDefault(f => string.Compare(f.Name, reference.MicroService, true) == 0) != null)
					continue;

				var ms = Tenant.GetService<IMicroServiceService>().Select(reference.MicroService);

				if (ms != null)
				{
					existing.Add(ms);
					FlattenReferences(ms.Token, existing);
				}
			}
		}
	}
}
