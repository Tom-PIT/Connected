using Newtonsoft.Json.Linq;
using System;
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

			var configurations = connection.GetService<IComponentService>().QueryConfigurations(Shell.GetConfiguration<IClientSys>().ResourceGroups, "IoTHub");

			foreach (var i in configurations)
			{
				if (i is IIoTHub h)
					Set(h.Component, h, TimeSpan.Zero);
			}

			Data = new HubDataCache(Connection);
		}

		private void OnComponentChanged(ISysConnection sender, ComponentEventArgs e)
		{
			var hub = Connection.GetService<IComponentService>().SelectConfiguration(e.Component) as IIoTHub;

			Remove(e.Component);

			if (hub != null)
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
			var schema = ResolveSchema(device);
		}

		private IIoTSchema ResolveSchema(IIoTDevice device)
		{
			var hub = device.Closest<IIoTHub>();

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
	}
}
