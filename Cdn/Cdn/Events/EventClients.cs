using System.Collections.Concurrent;
using System.Collections.Generic;

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
		public static void Add(EventClient client)
		{
			if(!Clients.TryGetValue(client.EventName.ToLowerInvariant(), out List<EventClient> clients))
			{
				clients = new List<EventClient>();

				if (!Clients.TryAdd(client.EventName.ToLowerInvariant(), clients))
					Clients.TryGetValue(client.EventName.ToLowerInvariant(), out clients);
			}

			lock (clients)
			{
				clients.Add(client);
			}
		}

		public static void Remove(string connectionId)
		{
			foreach(var eventList in Clients)
			{
				lock (eventList.Value)
				{
					for(var i = eventList.Value.Count - 1; i>=0; i--)
					{
						if (string.Compare(eventList.Value[i].ConnectionId, connectionId, true) == 0)
							eventList.Value.RemoveAt(i);
					}
				}
			}
		}

		public static List<EventClient> Query(string eventName)
		{
			if (!Clients.TryGetValue(eventName.ToLowerInvariant(), out List<EventClient> result))
				return null;

			return result;
		}

		public static void Remove(string connectionId, string eventName)
		{
			foreach (var eventList in Clients)
			{
				lock (eventList.Value)
				{
					for (var i = eventList.Value.Count - 1; i >= 0; i--)
					{
						if (string.Compare(eventList.Value[i].ConnectionId, connectionId, true) == 0
							&& string.Compare(eventList.Value[i].EventName, eventName, true) == 0)
							eventList.Value.RemoveAt(i);
					}
				}
			}
		}
	}
}
