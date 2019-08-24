using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Analysis.Manifest;
using TomPIT.Connectivity;

namespace TomPIT.Analysis
{
	internal class DiscoveryService : IDiscoveryService
	{
		public DiscoveryService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public IElement Find(IConfiguration configuration, Guid id)
		{
			return Find(configuration, id, new List<object>());
		}

		public IElement Find(Guid component, Guid id)
		{
			var config = Connection.GetService<IComponentService>().SelectConfiguration(component);

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

		public IServiceReferences References(Guid microService)
		{
			return References(Connection.ResolveMicroServiceName(microService));
		}

		public IServiceReferences References(string microService)
		{
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			var component = Connection.GetService<IComponentService>().SelectComponent(ms.Token, "Reference", "References");

			if (component == null)
				return null;

			return Connection.GetService<IComponentService>().SelectConfiguration(component.Token) as IServiceReferences;
		}

		public IComponentManifest Manifest(Guid component)
		{
			var c = Connection.GetService<IComponentService>().SelectComponent(component);

			if (c == null)
				return null;

			return c.Manifest(Connection);
		}

		public IComponentManifest Manifest(string microService, string category, string componentName)
		{
			var ms = Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			var c = Connection.GetService<IComponentService>().SelectComponent(ms.Token, category, componentName);

			if (c == null)
				return null;

			return c.Manifest(Connection);
		}

		public List<IComponentManifest> Manifests(Guid microService)
		{
			var components = Connection.GetService<IComponentService>().QueryComponents(microService);
			var result = new List<IComponentManifest>();

			foreach (var component in components)
			{
				var manifest = component.Manifest(Connection);

				if (manifest != null)
					result.Add(manifest);
			}

			return result;
		}
	}
}
