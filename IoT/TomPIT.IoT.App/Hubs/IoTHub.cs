using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.IoT.Security;
using TomPIT.IoT.Services;

namespace TomPIT.IoT.Hubs
{
	[Authorize]
	public class IoTHub : Microsoft.AspNetCore.SignalR.Hub
	{
		public IoTHub(IHubContext<IoTHub> context)
		{
			IoTHubs.IoT = context;
		}

		public async void Data(JObject e)
		{
			var device = Context.User.Identity as DeviceIdentity;
			var changes = Instance.Connection.GetService<IIoTHubService>().SetData(device.Device, e);

			if (changes == null || changes.Count == 0)
				return;

			await Clients.Others.SendAsync("data", changes);
		}

		public void Transaction(JObject e)
		{
			var microService = e.Required<string>("microService");
			var hub = e.Required<string>("hub");
			var transaction = e.Required<string>("transaction");
			var device = e.Required<string>("device");

			var ms = Instance.Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, microService));

			if (hub.Contains('/'))
			{
				var tokens = hub.Split('/');
				hub = tokens[1].Trim();

				if (string.Compare(tokens[0], microService, true) == 0)
					hub = tokens[1];

				ms.ValidateMicroServiceReference(Instance.Connection, tokens[0]);
				ms = Instance.Connection.GetService<IMicroServiceService>().Select(tokens[0]);
			}

			if (!(Instance.Connection.GetService<IComponentService>().SelectConfiguration(ms.Token, "IoTHub", hub) is IIoTHub config))
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrIoTHubNotFound, hub));

			var hubDevice = config.Devices.FirstOrDefault(f => string.Compare(f.Name, device, true) == 0);

			if (hubDevice == null)
				throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrIoTHubDeviceNotFound, hub, device));

			Clients.User(hubDevice.Id.ToString()).SendAsync("transaction", e);
		}
	}
}
