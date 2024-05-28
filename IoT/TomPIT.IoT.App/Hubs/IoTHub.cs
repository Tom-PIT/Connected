using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.IoT.Hubs
{
	//[Authorize(AuthenticationSchemes = "TomPIT")]
	public class IoTServerHub : Hub
	{
		private const string Device = "device";
		private const string Hub = "hub";

		public async Task Register(List<IoTHubDevice> devices)
		{
			try
			{
				foreach (var device in devices)
					AuthorizeHub($"{device.MicroService}/{device.Hub}", IoTConnectionMethod.Device);

				foreach (var device in devices)
					await Groups.AddToGroupAsync(Context.ConnectionId, device.ToString().ToLowerInvariant());

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		public async Task Unregister(List<IoTHubDevice> devices)
		{
			try
			{
				foreach (var device in devices)
					await Groups.RemoveFromGroupAsync(Context.ConnectionId, device.ToString().ToLowerInvariant());

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		public async Task Add(List<HubSubscription> subscriptions)
		{
			try
			{
				foreach (var subscription in subscriptions)
					AuthorizeHub(subscription.ToString(), IoTConnectionMethod.Client);

				foreach (var subscription in subscriptions)
					await Groups.AddToGroupAsync(Context.ConnectionId, subscription.ToString().ToLowerInvariant());

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		public async Task Remove(List<HubSubscription> subscriptions)
		{
			try
			{
				foreach (var subscription in subscriptions)
					await Groups.RemoveFromGroupAsync(Context.ConnectionId, subscription.ToString().ToLowerInvariant());

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		private void AuthorizeHub(string identifier, IoTConnectionMethod method)
		{
			var ctx = new MiddlewareContext();
			var descriptor = ComponentDescriptor.IoTHub(ctx, identifier);

			descriptor.Validate();

			if (descriptor.Configuration == null)
				throw new NotFoundException($"{SR.ErrCannotFindConfiguration} ({identifier})");

			var type = ctx.Tenant.GetService<ICompilerService>().ResolveType(descriptor.MicroService.Token, descriptor.Configuration, descriptor.ComponentName);
		
			var instance = ctx.Tenant.GetService<ICompilerService>().CreateInstance<IMiddlewareComponent>(descriptor.Context, type);

			var itf = instance.GetType().GetInterface(typeof(IIoTHubMiddleware<>).FullName);
			var args = new IoTConnectionArgs(Context.ConnectionId, method);

			itf.InvokeMember("Authorize", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, instance, new object[] { args });

			ctx.Dispose();
			instance.Context.Dispose();
			instance.Dispose();
		}

		public async Task Data(JObject e)
		{
			try
			{
				using var processor = new DataProcessor(e);
				var schema = processor.Process();
				processor.Commit();
				var changes = MiddlewareDescriptor.Current.Tenant.GetService<IIoTHubService>().SetData(processor.DeviceName, schema);

				if (changes == null || changes.Count == 0)
					return;

				var data = new JObject
				{
					{Hub.ToLowerInvariant(), $"{processor.Descriptor.MicroServiceName}/{processor.Descriptor.ComponentName}" },
					{Device.ToLowerInvariant(), processor.Descriptor.Element },
					{"data", changes }
				};

				await Clients.Group(processor.Group.ToLowerInvariant()).SendAsync("data", data);
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		public static async Task Invoke(JObject e, IHubClients<IClientProxy> clients) 
		{
			try
			{
				using var processor = new TransactionProcessor(e);

				processor.Process();

				var args = new JObject
				{
					{"device", processor.DeviceName },
					{"transaction", processor.Transaction },
					{"arguments",  processor.TransactionArguments}
				};

				await clients?.Group(processor.DeviceName.ToLowerInvariant()).SendAsync("transaction", args);
			}
			catch (Exception ex)
			{
				if (clients is IHubCallerClients callerClients)
					await callerClients.Caller.SendAsync("exception", ex.Message);
			}
		}

		public async Task Invoke(JObject e)
		{
			await Invoke(e, Clients);
		}
	}
}
