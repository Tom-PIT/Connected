using Org.BouncyCastle.Cms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using TomPIT.Environment;
using TomPIT.UI.Theming.Utils;

namespace TomPIT.Cdn.Events
{
    internal static class EventMessagingCache
    {
        private static readonly ConcurrentDictionary<string, EventMessages> _clients;

        static EventMessagingCache()
        {
            _clients = new ConcurrentDictionary<string, EventMessages>(StringComparer.OrdinalIgnoreCase);
        }

        public static ConcurrentDictionary<string, EventMessages> Clients => _clients;

        public static void Clean()
        {
            foreach (var client in Clients)
            {
                client.Value.Scave();

                if (client.Value.IsEmpty)
                    Clients.TryRemove(client.Key, out _);
            }
        }

        public static ImmutableList<EventMessage> Dequeue()
        {
            var result = ImmutableList<EventMessage>.Empty;

            foreach (var client in Clients)
            {
                var items = client.Value.Dequeue();

                if (!items.IsDefaultOrEmpty)
                    result = result.AddRange(items);
            }

            return result;
        }

        public static void Remove(string connectionId)
        {
            foreach (var client in Clients)
            {
                client.Value.Remove(connectionId);

                if (client.Value.IsEmpty)
                    Clients.TryRemove(client.Key, out _);
            }
        }
        public static void Remove(EventAcknowledgeArgs e)
        {
            if (!Clients.TryGetValue(e.Client, out EventMessages items))
                return;

            items.Remove(e.Id);

            if (items.IsEmpty)
            {
                lock (items)
                {
                    if (items.IsEmpty)
                    {
                        if (Clients.TryRemove(e.Client, out EventMessages removed))
                        {
                            if (!removed.IsEmpty)
                                Clients.TryAdd(e.Client, removed);
                        }
                    }
                }
            }
        }

        public static void Remove(string client, string eventName, string recipient)
        {
            if (string.IsNullOrEmpty(client))
                return;

            if (!Clients.TryGetValue(client, out EventMessages items))
                return;

            lock (items)
            {
                items.RemoveEvents(eventName, recipient);

                if (items.IsEmpty)
                {
                    if (Clients.TryRemove(client, out EventMessages removed))
                    {
                        if (!removed.IsEmpty)
                            Clients.TryAdd(client, removed);
                    }
                }
            }
        }

        public static void Add(string client, EventMessage message)
        {
            if (!Clients.TryGetValue(client, out EventMessages items))
            {
                items = new EventMessages();

                if (!Clients.TryAdd(client, items))
                    Clients.TryGetValue(client, out items);
            }

            items.Add(message);
        }
    }
}
