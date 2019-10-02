using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.Diagostics;
using TomPIT.Exceptions;
using TomPIT.IoT.Security;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.IoT.Hubs
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class IoTServerHub : Microsoft.AspNetCore.SignalR.Hub
	{
		private static readonly string[] SpecialTransactionParameters = { "microService", "hub", "device", "transaction" };

		public IoTServerHub(IHubContext<IoTServerHub> context)
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
				var hub = device.Device.Configuration();
				var component = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(hub.Component);
				var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);
				var groupName = string.Format("{0}/{1}", ms.Name.ToLowerInvariant(), component.Name.ToLowerInvariant());

				try
				{
					var ctx = new MicroServiceContext(ms);
					var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(ms.Token, device.Device, device.Device.Name, false);

					if (type != null)
					{
						var handler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IIoTDeviceMiddleware>(ctx, type);

						handler.Arguments = e;
						handler.Invoke();

						if (e != null)
							Serializer.Populate(JsonConvert.SerializeObject(e), handler);

						handler.Invoke();

						if (e == null)
							e = new JObject();

						Merge(hub as IIoTHubConfiguration, handler, e);
					}
				}
				catch (Exception ex)
				{
					MiddlewareDescriptor.Current.Tenant.LogError("IoT", ex.Source, ex.Message);
				}

				var changes = MiddlewareDescriptor.Current.Tenant.GetService<IIoTHubService>().SetData(device.Device, e);

				if (changes == null || changes.Count == 0)
					return;

				await Clients.Group(groupName).SendAsync("data", changes);
			}
			catch (Exception ex)
			{
				throw new HubException(ex.Message);
			}
		}

		private void Merge(IIoTHubConfiguration hub, IIoTDeviceMiddleware handler, JObject arguments)
		{
			var schema = MiddlewareDescriptor.Current.Tenant.GetService<IIoTHubService>().SelectSchema(hub);
			var serializedHandler = Serializer.Deserialize<JObject>(handler);

			foreach (var property in serializedHandler.Children())
			{
				if (!(property is JProperty p))
					continue;

				var field = schema.Fields.FirstOrDefault(f => string.Compare(f.Name, p.Name, true) == 0);

				if (field == null)
					continue;

				var existingProperty = arguments.Property(field.Name, StringComparison.OrdinalIgnoreCase);

				if (existingProperty == null)
					arguments.Add(new JObject { field.Name, p.Value });
				else
					existingProperty.Value = p.Value;
			}
		}

		public async void Transaction(JObject e)
		{
			try
			{
				var microService = e.Required<string>("microService");
				var hub = e.Required<string>("hub");
				var transaction = e.Required<string>("transaction");
				var device = e.Required<string>("device");

				var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(microService);

				if (ms == null)
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, microService));

				if (hub.Contains('/'))
				{
					var tokens = hub.Split('/');
					hub = tokens[1].Trim();

					if (string.Compare(tokens[0], microService, true) == 0)
						hub = tokens[1];

					ms.ValidateMicroServiceReference(tokens[0]);
					ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);
				}

				if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, "IoTHub", hub) is IIoTHubConfiguration config))
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrIoTHubNotFound, hub));

				var hubDevice = config.Devices.FirstOrDefault(f => string.Compare(f.Name, device, true) == 0);

				if (hubDevice == null)
					throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrIoTHubDeviceNotFound, hub, device));

				var schema = MiddlewareDescriptor.Current.Tenant.GetService<IIoTHubService>().SelectSchema(hubDevice.Closest<IIoTHubConfiguration>());
				var t = hubDevice.Transactions.FirstOrDefault(f => string.Compare(f.Name, transaction, true) == 0);

				if (t == null)
					t = schema.Transactions.FirstOrDefault(f => string.Compare(f.Name, transaction, true) == 0);

				if (t == null)
					throw new RuntimeException(string.Format("{0} ({1}/{2}/{3})", SR.ErrIoTTransactionNotAllowed, hub, device, transaction));

				var parameters = new JObject
				{
					{"transaction",transaction }
				};

				var ctx = new MicroServiceContext(ms.Token);
				var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(ms.Token, t, t.Name, false);

				if (type != null)
				{
					var handler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IIoTTransactionMiddleware>(ctx, type, Serializer.Serialize(e));

					handler.Invoke();

					Serializer.Merge(e, handler);
				}
				else
				{
					foreach (var i in e.Children())
					{
						if (!(i is JProperty property))
							continue;

						if (IsSpecialName(property.Name))
							continue;

						parameters.Add(property);
					}
				}

				await Clients.User(hubDevice.Id.ToString()).SendAsync("transaction", parameters);
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
