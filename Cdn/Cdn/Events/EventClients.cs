﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Collections;

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

            var existing = clients.ToImmutableList(true);

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
                var items = client.Value.Where(f => f is not null && f.RetentionDeadline != DateTime.MinValue && f.RetentionDeadline <= DateTime.UtcNow).ToImmutableList(true);

                lock (client.Value)
                {
                    foreach (var item in items)
                        client.Value.Remove(item);

                    if (!client.Value.Any())
                        RemoveEvent(client.Key);
                }
            }
        }
        public static void Remove(string connectionId)
        {
            foreach (var eventList in Clients)
            {
                var items = eventList.Value.ToImmutableArray(true);

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
                            item.RetentionDeadline = DateTime.UtcNow.AddMinutes(2);
                    }
                }

                if (!eventList.Value.Any())
                    RemoveEvent(eventList.Key);
            }
        }

        public static ImmutableList<EventClient> Query(string eventName)
        {
            if (!Clients.TryGetValue(eventName.ToLowerInvariant(), out List<EventClient> result))
                return null;

            return result.Where(f => f is not null).ToImmutableList(true);
        }

        public static void Remove(string connectionId, string eventName, string recipient)
        {
            foreach (var clientList in Clients)
            {
                var items = clientList.Value.ToImmutableList(true);

                foreach (var item in items)
                {
                    if (item is null)
                        continue;

                    if (string.Compare(item.ConnectionId, connectionId, true) == 0
                        && string.Compare(item.EventName, eventName, true) == 0
                        && string.Compare(item.Recipient, recipient, true) == 0)
                    {
                        lock (clientList.Value)
                        {
                            clientList.Value.Remove(item);

                            if (!clientList.Value.Any(f => f is not null))
                                RemoveEvent(clientList.Key);
                        }
                    }
                }
            }
        }

        private static void RemoveEvent(string key)
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
