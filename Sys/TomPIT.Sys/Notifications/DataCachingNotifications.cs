using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using TomPIT.Caching;
using TomPIT.Notifications;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Notifications
{
	internal static class DataCachingNotifications
	{
		internal static IHubContext<DataCacheHub> Cache { get; set; }

		private static async void Notify<T>(string method, T e)
		{
			var args = new MessageEventArgs<T>(Guid.NewGuid(), e);

			var state = new CacheState
			{
				Method = method,
				Type = args.GetType().TypeName(),
				Content = JsonConvert.SerializeObject(args)
			};

			DataModel.Messages.Insert("datacache", args.Message, JsonConvert.SerializeObject(state), DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(5), SysExtensions.RequestInstanceId);

			if (Cache != null)
			{
				var sender = SysExtensions.RequestConnectionId("datacache");

				if (string.IsNullOrWhiteSpace(sender))
					await Cache.Clients.All.SendAsync(method, args);
				else
					await Cache.Clients.AllExcept(sender).SendAsync(method, args);
			}
		}

		public static void Clear(string key) { Notify(nameof(Clear), new DataCacheEventArgs(key)); }
		public static void Invalidate(string key, List<string> ids) { Notify(nameof(Clear), new DataCacheEventArgs(key, ids)); }
		public static void Remove(string key, List<string> ids) { Notify(nameof(Clear), new DataCacheEventArgs(key, ids)); }
	}
}
