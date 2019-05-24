using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.IoT.Services
{
	internal class IoTHubService : ClientRepository<IIoTHub, Guid>, IIoTHubService
	{
		public IoTHubService(ISysConnection connection) : base(connection, "iothubs")
		{
			Connection.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
			Connection.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
			Connection.GetService<IComponentService>().ConfigurationRemoved += OnConfigurationRemoved;
			Connection.GetService<IComponentService>().ConfigurationAdded += OnConfigurationAdded;

			var configurations = connection.GetService<IComponentService>().QueryConfigurations(Shell.GetConfiguration<IClientSys>().ResourceGroups, "IoTHub");

			foreach (var i in configurations)
			{
				if (i is IIoTHub h)
					Set(h.Component, h, TimeSpan.Zero);
			}

			Data = new HubDataCache(Connection);
		}

		private void OnConfigurationAdded(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, "IoTHub", true) == 0)
				Reload(e.Component);
		}

		private void OnConfigurationRemoved(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, "IoTHub", true) == 0)
				Remove(e.Component);
		}

		private void OnConfigurationChanged(ISysConnection sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, "IoTHub", true) == 0)
				Reload(e.Component);
		}

		private void OnComponentChanged(ISysConnection sender, ComponentEventArgs e)
		{
			if (string.Compare(e.Category, "IoTHub", true) == 0)
				Reload(e.Component);
		}

		private void Reload(Guid component)
		{
			Remove(component);

			if (Connection.GetService<IComponentService>().SelectConfiguration(component) is IIoTHub hub)
				Set(hub.Component, hub, TimeSpan.Zero);
		}

		public IIoTDevice SelectDevice(string authenticationToken)
		{
			foreach (var i in All())
			{
				var d = i.Devices.FirstOrDefault(f => string.Compare(f.AuthenticationToken, authenticationToken, true) == 0);

				if (d != null)
					return d;
			}

			return null;
		}

		public JObject SetData(IIoTDevice device, JObject data)
		{
			var schema = SelectSchema(device.Closest<IIoTHub>());
			var hub = device.Configuration().Component;
			ValidateData(data, schema);
			var state = Connection.GetService<IIoTService>().SelectState(hub);
			var changed = new List<IIoTFieldStateModifier>();

			foreach (var i in data)
			{
				var value = i.Value as JValue;
				var field = state.FirstOrDefault(f => string.Compare(f.Field, i.Key, true) == 0);
				var schemaField = schema.Fields.FirstOrDefault(f => string.Compare(f.Name, i.Key, true) == 0);
				var type = Types.ToType(schemaField.DataType);

				if (!Types.TryConvertInvariant(value.Value, out object v, type))
					throw new RuntimeException(string.Format("{0} ({1}, {2})", SR.ErrIoTConversionError, schemaField.Name, type.ToFriendlyName()));

				if (field != null)
				{
					object existingValue = null;

					if (string.IsNullOrWhiteSpace(field.Value))
						existingValue = Types.DefaultValue(type);
					else
					{
						if (!Types.TryConvertInvariant(field.Value, out existingValue, type))
							existingValue = null;
					}

					if (Types.Compare(v, existingValue))
						continue;
				}

				changed.Add(new IoTFieldStateModifier
				{
					Field = schemaField.Name,
					Value = Types.Convert<string>(v, CultureInfo.InvariantCulture)
				});
			}

			if (changed.Count == 0)
				return null;

			return Data.Update(hub, changed);
		}

		private void ValidateData(JObject data, IIoTSchema schema)
		{
			foreach (var i in data)
			{
				var field = schema.Fields.FirstOrDefault(f => string.Compare(f.Name, i.Key, true) == 0);

				if (field == null)
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrIoTSchemaFieldNotDefined, i.Key));

				if (!(i.Value is JValue))
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrIoTExpectedValue, i.Key));
			}
		}

		public IIoTSchema SelectSchema(IIoTHub hub)
		{
			if (string.IsNullOrWhiteSpace(hub.Schema))
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrIoTHubSchemaNotSet, hub.ComponentName(Connection)));

			var ms = hub.MicroService(Connection);
			var schema = hub.Schema;

			if (hub.Schema.Contains('/'))
			{
				var tokens = hub.Schema.Split('/');
				var hubMs = Connection.GetService<IMicroServiceService>().Select(tokens[0]);
				schema = tokens[1];

				if (hubMs == null)
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, tokens[0]));

				if (hubMs.Token != ms)
				{
					var targetMs = Connection.GetService<IMicroServiceService>().Select(ms);

					targetMs.ValidateMicroServiceReference(Connection, hubMs.Name);

					ms = hubMs.Token;
				}
			}

			if (!(Connection.GetService<IComponentService>().SelectConfiguration(ms, "IoTSchema", schema) is IIoTSchema config))
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrIoTSchemaNotFound, schema));

			return config;
		}

		private HubDataCache Data { get; }

		public void FlushChanges()
		{
			Data.Flush();
		}
	}
}
