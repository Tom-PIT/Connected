using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using TomPIT.IoT;
using TomPIT.Messaging;
using TomPIT.Reflection;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Notifications
{
	internal static class IoTNotifications
	{
		internal static IHubContext<IoTHub> Cache { get; set; }

		private static async void Notify<T>(string method, T e)
		{
			var args = new MessageEventArgs<T>(Guid.NewGuid(), e);

			var state = new CacheState
			{
				Method = method,
				Type = args.GetType().TypeName(),
				Content = JsonConvert.SerializeObject(args)
			};

			DataModel.Messages.Insert("iot", args.Message, JsonConvert.SerializeObject(state), DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(5), SysExtensions.RequestInstanceId);

			if (Cache != null)
			{
				var sender = SysExtensions.RequestConnectionId("iot");

				if (string.IsNullOrWhiteSpace(sender))
					await Cache.Clients.All.SendAsync(method, args);
				else
					await Cache.Clients.AllExcept(sender).SendAsync(method, args);
			}
		}

		public static void IoTStateChanged(Guid hub, List<IIoTFieldStateModifier> state) { Notify(nameof(IoTStateChanged), new IoTStateChangedArgs(hub, state)); }
	}
}
