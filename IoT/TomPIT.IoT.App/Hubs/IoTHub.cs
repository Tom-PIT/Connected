﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.IoT.Security;
using TomPIT.IoT.Services;

namespace TomPIT.IoT.Hubs
{
	[Authorize]
	public class IoTHub : Microsoft.AspNetCore.SignalR.Hub
	{
		private static readonly string[] SpecialTransactionParameters = { "microService", "hub", "device", "transaction" };

		public IoTHub(IHubContext<IoTHub> context)
		{
			IoTHubs.IoT = context;
		}

		public override Task OnConnectedAsync()
		{
			var qs = Context.GetHttpContext().Request.Query;

			var ms = string.Empty;
			var hub = string.Empty;

			if (qs.ContainsKey("microService"))
				ms = qs["microService"];

			if (qs.ContainsKey("hub"))
				hub = qs["hub"];

			if (!string.IsNullOrWhiteSpace(ms) && !string.IsNullOrWhiteSpace(hub))
				Groups.AddToGroupAsync(Context.ConnectionId, string.Format("{0}/{1}", ms.ToLowerInvariant(), hub.ToLowerInvariant()));

			return Task.CompletedTask;
		}

		public async void Data(JObject e)
		{
			var device = Context.User.Identity as DeviceIdentity;
			try
			{
				var changes = Instance.Connection.GetService<IIoTHubService>().SetData(device.Device, e);

				if (changes == null || changes.Count == 0)
					return;

				var hub = device.Device.Configuration();
				var component = Instance.Connection.GetService<IComponentService>().SelectComponent(hub.Component);
				var ms = Instance.Connection.GetService<IMicroServiceService>().Select(component.MicroService);
				var groupName = string.Format("{0}/{1}", ms.Name.ToLowerInvariant(), component.Name.ToLowerInvariant());

				await Clients.Group(groupName).SendAsync("data", changes);
			}
			catch (Exception ex)
			{
				throw new HubException(ex.Message);
			}
		}

		public void Transaction(JObject e)
		{
			try
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

				var schema = Instance.Connection.GetService<IIoTHubService>().SelectSchema(hubDevice.Closest<IIoTHub>());
				var t = hubDevice.Transactions.FirstOrDefault(f => string.Compare(f.Name, transaction, true) == 0);

				if (t == null)
					t = schema.Transactions.FirstOrDefault(f => string.Compare(f.Name, transaction, true) == 0);

				if (t == null)
					throw new RuntimeException(string.Format("{0} ({1}/{2}/{3})", SR.ErrIoTTransactionNotAllowed, hub, device, transaction));

				var parameters = new JObject
				{
					{"transaction",transaction }
				};

				foreach (var i in e)
				{
					if (IsSpecialName(i.Key))
						continue;

					var def = t.Parameters.FirstOrDefault(f => string.Compare(f.Name, i.Key, true) == 0);

					if (def == null)
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrIoTParameterNotAllowed, transaction, i.Key));
				}

				foreach (var i in t.Parameters)
				{
					var prop = e.Property(i.Name, StringComparison.OrdinalIgnoreCase);

					if (prop == null)
					{
						if (!i.IsNullable)
							throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrIoTParameterExpected, transaction, i.Name));

						continue;
					}

					if (!(prop.Value is JValue value))
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrIoTParameterValueExpected, transaction, i.Name));

					var dt = Types.ToType(i.DataType);

					if (!Types.TryConvertInvariant(value.Value, out object v, dt))
						throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrIoTConversionError, i.Name, dt.ToFriendlyName()));

					parameters.Add(i.Name, Types.Convert<string>(v, CultureInfo.InvariantCulture));
				}

				Clients.User(hubDevice.Id.ToString()).SendAsync("transaction", parameters);
			}
			catch (Exception ex)
			{
				throw new HubException(ex.Message);
			}
		}

		private bool IsSpecialName(string parameterName)
		{
			foreach (var i in SpecialTransactionParameters)
			{
				if (string.Compare(i.ToLowerInvariant(), parameterName.ToLowerInvariant(), true) == 0)
					return true;
			}

			return false;
		}
	}
}