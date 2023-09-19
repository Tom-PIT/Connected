using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using TomPIT.Collections;

namespace TomPIT.Cdn.Events
{
    internal class EventMessages
    {
        private readonly List<EventMessage> _items;

        public EventMessages()
        {
            _items = new List<EventMessage>();
        }
        private List<EventMessage> Items => _items;

        public bool IsEmpty => !Items.Any();

        public ImmutableArray<EventMessage> All()
        {
            lock (Items)
                return Items.ToImmutableArray(true);
        }

        public void Scave()
        {
            var items = All().Where(f => f is not null && f.Expire <= DateTime.UtcNow);

            if (items.Any())
            {
                lock (Items)
                {
                    foreach (var item in items)
                        Items.Remove(item);
                }
            }
        }

        public ImmutableArray<EventMessage> Dequeue()
        {
            var items = All().Where(f => f is not null && f.NextVisible <= DateTime.UtcNow && f.Expire > DateTime.UtcNow).ToImmutableArray(true);

            if (!items.Any())
                return ImmutableArray<EventMessage>.Empty;

            foreach (var item in items)
            {
                item.NextVisible = DateTime.UtcNow.AddSeconds(5 * item.DequeueCount);
                item.DequeueCount++;
            }

            return items;
        }

        public void Remove(string connectionId)
        {
            var items = All().Where(f => string.Compare(f.Connection, connectionId, true) == 0);

            if (items.Any())
            {
                lock (Items)
                {
                    foreach (var item in items)
                        Items.Remove(item);
                }
            }
        }

        public void Remove(ulong id)
        {
            if (All().FirstOrDefault(f => f.Id == id) is EventMessage message)
            {
                lock (Items)
                {
                    Items.Remove(message);
                }
            }
        }

        public void RemoveEvents(string eventName, string recipient)
        {
            var obsolete = All().Where(f => string.Compare(f.Event, eventName, true) == 0
                && string.Compare(f.Recipient, recipient, true) == 0);

            if (obsolete.Any())
            {
                lock (Items)
                {
                    foreach (var o in obsolete)
                        Items.Remove(o);
                }
            }
        }

        public void Add(EventMessage message)
        {
            lock (Items)
                Items.Add(message);
        }
    }
}
