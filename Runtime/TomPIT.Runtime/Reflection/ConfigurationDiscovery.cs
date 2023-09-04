using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;

namespace TomPIT.Reflection
{
	internal class ConfigurationDiscovery : TenantObject, IConfigurationDiscovery
	{
		public ConfigurationDiscovery(ITenant tenant) : base(tenant)
		{
		}

		public IElement Find(Guid component, Guid id)
		{
			return Find(component, id, SearchMode.Element);
		}

		public IElement Find(Guid component, Guid blob, SearchMode mode)
		{
			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component);

			if (config is null)
				return null;

			return Find(config, blob, mode, new List<object>());
		}

		public IText Find(string path)
		{
			return new TextElementResolver(path).Resolve();
		}

		public IElement Find(IConfiguration configuration, Guid id)
		{
			return Find(configuration, id, SearchMode.Element, new List<object>());
		}

		private IElement Find(object instance, Guid id, SearchMode mode, List<object> referenceTrail)
		{
			if (instance == null)
				return null;

			if (referenceTrail.Contains(instance))
				return null;

			referenceTrail.Add(instance);

			switch (mode)
			{
				case SearchMode.Element:
					if (instance is IElement el && el.Id == id)
						return el;
					break;
				case SearchMode.Blob:
					if (instance is IText text && text.TextBlob == id)
						return text;
					else if (instance is IUploadResource upload && upload.Blob == id)
						return upload;
					break;
				default:
					throw new NotSupportedException();
			}

			if (instance.GetType().IsCollection())
			{
				if (instance is not IEnumerable en)
					return null;

				var enm = en.GetEnumerator();

				while (enm.MoveNext())
				{
					if (enm.Current is IElement element && element.Id == id)
						return element;
					else
					{
						var r = Find(enm.Current, id, mode, referenceTrail);

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

					var r = Find(value, id, mode, referenceTrail);

					if (r != null)
						return r;
				}
			}

			return null;
		}
		public ImmutableList<T> Query<T>(IConfiguration configuration) where T : IElement
		{
			var r = new List<T>();

			if (configuration is T t)
				r.Add(t);

			var props = ReflectionExtensions.Properties(configuration, false);
			var refs = new List<object>
			{
				configuration
			};

			foreach (var i in props)
				Query(configuration, i, r, refs);

			return r.ToImmutableList();
		}

		private void Query<T>(object instance, List<T> items, List<object> refs) where T : IElement
		{
			var props = ReflectionExtensions.Properties(instance, false);

			foreach (var i in props)
				Query(instance, i, items, refs);
		}

		private void Query<T>(object configuration, PropertyInfo property, List<T> items, List<object> refs) where T : IElement
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
				if (value is not IEnumerable en)
					return;

				var enm = en.GetEnumerator();

				while (enm.MoveNext())
				{
					if (enm.Current is null)
						continue;

					if (enm.Current is T t)
						items.Add(t);

					Query(enm.Current, items, refs);
				}
			}
			else if (!property.IsPrimitive())
			{
				if (value is T t && !items.Contains(t))
					items.Add(t);

				Query(value, items, refs);
			}
		}

		public ImmutableList<Guid> QueryDependencies(IConfiguration configuration)
		{
			var r = new HashSet<Guid>();
			var texts = Query<IText>(configuration);

			foreach (var j in texts)
			{
				if (j.TextBlob == Guid.Empty)
					continue;

				r.Add(j.TextBlob);
			}

			var er = Query<IExternalResourceElement>(configuration);

			foreach (var j in er)
			{
				var items = j.QueryResources();

				if (items == null || items.Count == 0)
					continue;

				foreach (var k in items)
				{
					if (k != Guid.Empty)
						r.Add(k);
				}
			}

			return r.ToImmutableList();
		}
	}
}
