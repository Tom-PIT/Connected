using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TomPIT.Cdn.Events
{
	internal static class EventClients
	{
		private static ConcurrentDictionary<string, List<EventClient>> _clients = null;

		static EventClients()
		{
			_clients = new ConcurrentDictionary<string, List<EventClient>>();
		}

		private static ConcurrentDictionary<string, List<EventClient>> Clients => _clients;

		public static void AddOrUpdate(EventClient client)
		{
			if (!Clients.TryGetValue(client.EventName.ToLowerInvariant(), out List<EventClient> clients))
			{
				clients = new List<EventClient>();

				if (!Clients.TryAdd(client.EventName.ToLowerInvariant(), clients))
					Clients.TryGetValue(client.EventName.ToLowerInvariant(), out clients);
			}

			var existing = clients.ToImmutableList();

			foreach (var c in existing)
			{
				if (c is null)
					continue;

				if (c.CompareTo(client) == 0)
				{
					c.ConnectionId = client.ConnectionId;
					c.RetentionDeadline = DateTime.MinValue;

					return;
				}
			}

			lock (clients)
				clients.Add(client);
		}

		public static void Clean()
		{
			foreach (var client in Clients)
			{
				var items = client.Value.Where(f => f is not null && f.RetentionDeadline != DateTime.MinValue && f.RetentionDeadline <= DateTime.UtcNow).ToImmutableList();

				lock (client.Value)
				{
					foreach (var item in items)
						client.Value.Remove(item);
				}

				if (!client.Value.Any())
					RemoveClient(client.Key);
			}
		}
		public static void Remove(string connectionId)
		{
			foreach (var eventList in Clients)
			{
				var items = eventList.Value.ToImmutableArray();

				foreach (var item in items)
				{
					if (item is null)
						continue;

					if (string.Compare(item.ConnectionId, connectionId, true) == 0)
					{
						if (item.Behavior == EventSubscriptionBehavior.FireForget)
						{
							lock (eventList.Value)
								eventList.Value.Remove(item);
						}
						else
							item.RetentionDeadline = DateTime.UtcNow.AddMinutes(5);
					}
				}

				if (!eventList.Value.Any())
					RemoveClient(eventList.Key);
			}
		}

		public static ImmutableList<EventClient> Query(string eventName)
		{
			if (!Clients.TryGetValue(eventName.ToLowerInvariant(), out List<EventClient> result))
				return null;

			return result.Where(f => f is not null).ToImmutableList();
		}

		public static void Remove(string connectionId, string eventName)
		{
			foreach (var eventList in Clients)
			{
				var items = eventList.Value.ToImmutableList();

				foreach (var item in items)
				{
					if (item is null)
						continue;

					if (string.Compare(item.ConnectionId, connectionId, true) == 0
						&& string.Compare(item.EventName, eventName, true) == 0)
					{
						lock (eventList.Value)
							eventList.Value.Remove(item);
					}
				}

				if (!eventList.Value.Any(f => f is not null))
					RemoveClient(eventList.Key);
			}
		}

		private static void RemoveClient(string key)
		{
			if (!Clients.TryGetValue(key, out List<EventClient> items))
				return;

			if (items.Any())
				return;

			/*
			 * check if someone has registered in the meantime
			 */
			if (Clients.TryRemove(key, out items))
			{
				if (items.Any(f => f is not null))
					Clients.TryAdd(key, items);
			}
		}
	}
}
