using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.Connectivity;
using TomPIT.IoT.Hubs;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime.Configuration;

namespace TomPIT.IoT.Services
{
	internal class IoTHubService : ClientRepository<IIoTHubConfiguration, Guid>, IIoTHubService
	{
		public IoTHubService(ITenant tenant) : base(tenant, "iothubs")
		{
			Tenant.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
			Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			Tenant.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;
			Tenant.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;

			var configurations = tenant.GetService<IComponentService>().QueryConfigurations(Shell.GetConfiguration<IClientSys>().ResourceGroups, ComponentCategories.IoTHub);

			foreach (var i in configurations)
			{
				if (i is IIoTHubConfiguration h)
					Set(h.Component, h, TimeSpan.Zero);
			}

			Data = new HubDataCache(Tenant);
		}

		private void OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.IoTHub, true) == 0)
				Reload(e.Component);
		}

		private void OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.IoTHub, true) == 0)
				Remove(e.Component);
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.IoTHub, true) == 0)
				Reload(e.Component);
		}

		private void OnComponentChanged(ITenant sender, ComponentEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.IoTHub, true) == 0)
				Reload(e.Component);
		}

		private void Reload(Guid component)
		{
			Remove(component);

			if (Tenant.GetService<IComponentService>().SelectConfiguration(component) is IIoTHubConfiguration hub)
				Set(hub.Component, hub, TimeSpan.Zero);
		}

		public JObject SetData(string device, object data)
		{
			var descriptor = ComponentDescriptor.IoTHub(new MiddlewareContext(), device);
			var state = Tenant.GetService<IIoTService>().SelectState(descriptor.Component.Token);
			var changed = new List<IIoTFieldStateModifier>();

			var properties = ConfigurationExtensions.GetMiddlewareProperties(data.GetType(), false);

			lock (state)
			{
				foreach (var property in properties)
				{
					if (!property.PropertyType.IsTypePrimitive() || property.PropertyType.IsCollection())
						continue;

					var value = property.GetValue(data);
					var field = state.FirstOrDefault(f => string.Compare(f.Device, device, true) == 0 && string.Compare(f.Field, property.Name, true) == 0);

					if (field != null)
					{
						object existingValue = null;

						if (string.IsNullOrWhiteSpace(field.Value))
							existingValue = TypeExtensions.DefaultValue(property.PropertyType);
						else
						{
							if (!Types.TryConvertInvariant(field.Value, out existingValue, property.PropertyType))
								existingValue = null;
						}

						if (Types.Compare(value, existingValue))
							continue;
					}

					changed.Add(new IoTFieldStateModifier
					{
						Field = property.Name,
						Value = Types.Convert<string>(value, CultureInfo.InvariantCulture),
						RawValue = value,
						Device = device
					});
				}
			}

			if (changed.Count == 0)
				return null;

			return Data.Update(descriptor.Component.Token, changed);
		}

		private HubDataCache Data { get; }

		public void FlushChanges()
		{
			Data.Flush();
		}
	}
}
