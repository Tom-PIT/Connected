using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TomPIT.Cdn.Events
{
	internal static class EventMessagingCache
	{
		private static readonly ConcurrentDictionary<string, List<EventMessage>> _clients;

		static EventMessagingCache()
		{
			_clients = new ConcurrentDictionary<string, List<EventMessage>>();
		}

		private static ConcurrentDictionary<string, List<EventMessage>> Clients => _clients;

		public static void Clean()
		{
			foreach(var client in Clients)
			{
				var items = client.Value.Where(f => f.Expire <= DateTime.UtcNow).ToImmutableList();

				foreach (var item in items)
					client.Value.Remove(item);

				if (client.Value.Count == 0)
					Clients.TryRemove(client.Key, out _);
			}
		}
		public static ImmutableList<EventMessage> Dequeue()
		{
			var result = ImmutableList<EventMessage>.Empty;

			foreach (var client in Clients)
			{
				var items = client.Value.Where(f => f.NextVisible <= DateTime.UtcNow);

				if (items.Any())
					result.AddRange(items);
			}

			foreach (var item in result)
				item.NextVisible = item.NextVisible.AddSeconds(5);

			return result;
		}

		public static void Remove(string connectionId)
		{
			foreach(var client in Clients)
			{
				var items = client.Value.Where(f => string.Compare(f.Connection, connectionId, true) == 0).ToImmutableArray();

				foreach (var item in items)
					client.Value.Remove(item);

				if (client.Value.Count == 0)
					Clients.TryRemove(client.Key, out _);
			}
		}
		public static void Remove(EventAcknowledgeArgs e)
		{
			if (!Clients.TryGetValue(e.Client.ToLowerInvariant(), out List<EventMessage> items))
				return;

			var item = items.FirstOrDefault(f => f.Id == e.Id);

			if (item != null)
				items.Remove(item);

			if (items.Count == 0)
				Clients.TryRemove(e.Client.ToLowerInvariant(), out _);
		}

		public static void Remap(string client, string connection)
		{
			if (!Clients.TryGetValue(client.ToLowerInvariant(), out List<EventMessage> items))
				return;

			foreach (var item in items)
				item.Connection = connection;
		}
		public static void Remove(string client, string eventName)
		{
			if (string.IsNullOrEmpty(client))
				return;

			if (!Clients.TryGetValue(client.ToLowerInvariant(), out List<EventMessage> items))
				return;

			var obsolete = items.Where(f => string.Compare(f.Event, eventName, true) == 0).ToImmutableList();

			foreach (var o in obsolete)
				items.Remove(o);

			if (items.Count == 0)
				Clients.Remove(client.ToLowerInvariant(), out _);
		}

		public static void Add(string client, EventMessage message)
		{
			if(!Clients.TryGetValue(client.ToLowerInvariant(), out List<EventMessage> items))
			{
				items = new List<EventMessage>();

				if (!Clients.TryAdd(client.ToLowerInvariant(), items))
					Clients.TryGetValue(client.ToLowerInvariant(), out items);
			}

			items.Add(message);
		}
	}
}
