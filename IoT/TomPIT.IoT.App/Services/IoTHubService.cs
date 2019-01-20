using System;
using System.Linq;
using TomPIT.Caching;
using TomPIT.ComponentModel;
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
	}
}
