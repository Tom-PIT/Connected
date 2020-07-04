using System;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using TomPIT.BigData;
using TomPIT.Messaging;
using TomPIT.Reflection;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Notifications
{
	internal static class BigDataNotifications
	{
		internal static IHubContext<BigDataHub> Cache { get; set; }

		private static async void Notify<T>(string method, T e)
		{
			var args = new MessageEventArgs<T>(Guid.NewGuid(), e);

			var state = new CacheState
			{
				Method = method,
				Type = args.GetType().TypeName(),
				Content = JsonConvert.SerializeObject(args)
			};

			DataModel.Messages.Insert("bigdata", args.Message, JsonConvert.SerializeObject(state), DateTime.UtcNow.AddMinutes(1), TimeSpan.FromSeconds(3), SysExtensions.RequestInstanceId);

			if (Cache != null)
			{
				var sender = SysExtensions.RequestConnectionId("bigdata");

				if (string.IsNullOrWhiteSpace(sender))
					await Cache.Clients.All.SendAsync(method, args);
				else
					await Cache.Clients.AllExcept(sender).SendAsync(method, args);
			}
		}

		/*
		 * Big data
		 */
		public static void NodeChanged(Guid token) { Notify(nameof(NodeChanged), new NodeArgs(token)); }
		public static void NodeRemoved(Guid token) { Notify(nameof(NodeRemoved), new NodeArgs(token)); }
		public static void NodeAdded(Guid token) { Notify(nameof(NodeAdded), new NodeArgs(token)); }
		public static void PartitionChanged(Guid configuration) { Notify(nameof(PartitionChanged), new PartitionArgs(configuration)); }
		public static void PartitionRemoved(Guid configuration) { Notify(nameof(PartitionRemoved), new PartitionArgs(configuration)); }
		public static void PartitionAdded(Guid configuration) { Notify(nameof(PartitionAdded), new PartitionArgs(configuration)); }
		public static void PartitionFileChanged(Guid configuration) { Notify(nameof(PartitionFileChanged), new PartitionFileArgs(configuration)); }
		public static void PartitionFileRemoved(Guid configuration) { Notify(nameof(PartitionFileRemoved), new PartitionFileArgs(configuration)); }
		public static void PartitionFileAdded(Guid configuration) { Notify(nameof(PartitionFileAdded), new PartitionFileArgs(configuration)); }
		public static void PartitionFieldStatisticsChanged(Guid file, string fieldName) { Notify(nameof(PartitionFieldStatisticsChanged), new PartitionFieldStatisticArgs(file, fieldName)); }
	}
}
