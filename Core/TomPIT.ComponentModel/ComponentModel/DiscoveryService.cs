using Newtonsoft.Json;
using System;
using System.Collections;
using System.Reflection;
using TomPIT.Net;

namespace TomPIT.ComponentModel
{
	internal class DiscoveryService : IDiscoveryService
	{
		public DiscoveryService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

		public IElement Find(Guid component, Guid id)
		{
			var config = Server.GetService<IComponentService>().SelectConfiguration(component);

			if (config == null)
				return null;

			return Find(config, id, new ListItems<object>());
		}

		private IElement Find(object instance, Guid id, ListItems<object> referenceTrail)
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
			return References(Server.ResolveMicroServiceName(microService));
		}

		public IServiceReferences References(string microService)
		{
			var ms = Server.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return null;

			var component = Server.GetService<IComponentService>().SelectComponent(ms.Token, "Reference", "References");

			if (component == null)
				return null;

			return Server.GetService<IComponentService>().SelectConfiguration(component.Token) as IServiceReferences;
		}

	}
}
