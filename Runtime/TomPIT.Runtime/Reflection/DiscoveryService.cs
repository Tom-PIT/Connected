using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using TomPIT.Annotations.Design;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;
using TomPIT.Design.Serialization;
using TomPIT.Reflection.Manifests;
using TomPIT.Reflection.Manifests.Entities;
using TomPIT.Runtime;
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

		public List<T> Children<T>(IConfiguration configuration) where T : IElement
		{
			var r = new List<T>();

			if (configuration is T)
				r.Add((T)configuration);

			var props = Properties(configuration, false, false);
			var refs = new List<object>
			{
				configuration
			};

			foreach (var i in props)
				Children(configuration, i, r, refs);

			return r;

		}

		private void Children<T>(object instance, List<T> items, List<object> refs) where T : IElement
		{
			var props = Properties(instance, false, false);

			foreach (var i in props)
				Children(instance, i, items, refs);
		}

		private void Children<T>(object configuration, PropertyInfo property, List<T> items, List<object> refs) where T : IElement
		{
			if (property.IsIndexer() || property.IsPrimitive())
				return;

			var value = property.GetValue(configuration);

			if (value == null)
				return;

			if (refs.Contains(value))
				return;

			refs.Add(value);

			if (property.IsCollection())
			{
				if (!(value is IEnumerable en))
					return;

				var enm = en.GetEnumerator();

				while (enm.MoveNext())
				{
					if (enm.Current == null)
						continue;

					if (enm.Current is T)
						items.Add((T)enm.Current);

					Children(enm.Current, items, refs);
				}
			}
			else if (!property.IsPrimitive())
			{
				if (value is T && !items.Contains((T)value))
					items.Add((T)value);

				Children(value, items, refs);
			}
		}

		public List<Guid> Dependencies(IConfiguration configuration)
		{
			var r = new List<Guid>();
			var texts = Children<IText>(configuration);

			foreach (var j in texts)
			{
				if (j.TextBlob == Guid.Empty)
					continue;

				r.Add(j.TextBlob);
			}

			var er = Children<IExternalResourceElement>(configuration);

			foreach (var j in er)
			{
				var items = j.QueryResources();

				if (items == null || items.Count == 0)
					continue;

				foreach (var k in items)
					r.Add(k);
			}

			return r;
		}

		public PropertyInfo[] Properties(object instance, bool writableOnly, bool filterByEnvironment)
		{
			var mode = Shell.GetService<IRuntimeService>().Mode;
			PropertyInfo[] properties = null;

			properties = instance.GetType().GetProperties();

			if (properties == null)
				return null;

			var temp = new List<PropertyInfo>();

			foreach (var i in properties)
			{
				var getMethod = i.GetGetMethod();
				var setMethod = i.GetSetMethod();

				if (writableOnly && setMethod == null)
					continue;

				if (getMethod == null)
					continue;

				if ((getMethod != null && getMethod.IsStatic) || (setMethod != null && setMethod.IsStatic))
					continue;

				if (setMethod != null && !setMethod.IsPublic)
					continue;

				temp.Add(i);
			}

			properties = temp.ToArray();

			if (filterByEnvironment)
			{
				switch (mode)
				{
					case EnvironmentMode.Design:
						return FilterDesignProperties(properties);
					case EnvironmentMode.Runtime:
						return FilterRuntimeProperties(properties);
					default:
						throw new NotSupportedException();
				}
			}

			return properties;
		}

		private PropertyInfo[] FilterDesignProperties(PropertyInfo[] properties)
		{
			var r = new List<PropertyInfo>();

			foreach (var i in properties)
			{
				var env = i.FindAttribute<EnvironmentVisibilityAttribute>();

				if (env == null || ((env.Visibility & EnvironmentMode.Design) == EnvironmentMode.Design))
					r.Add(i);
			}

			return r.ToArray();
		}

		private PropertyInfo[] FilterRuntimeProperties(PropertyInfo[] properties)
		{
			var r = new List<PropertyInfo>();

			foreach (var i in properties)
			{
				var env = i.FindAttribute<EnvironmentVisibilityAttribute>();

				if (env != null && ((env.Visibility & EnvironmentMode.Runtime) == EnvironmentMode.Runtime))
					r.Add(i);
			}

			return r.ToArray();
		}
	}
}
