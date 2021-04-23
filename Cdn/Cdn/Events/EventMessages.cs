using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

		public ImmutableArray<EventMessage> All() => Items.ToImmutableArray();

		public void Scave()
		{
			var items = All().Where(f => f.Expire <= DateTime.UtcNow);

			foreach (var item in items)
				Items.Remove(item);
		}

		public ImmutableArray<EventMessage> Dequeue()
		{
			var items = All().Where(f => f.NextVisible <= DateTime.UtcNow).ToImmutableArray();

			if (!items.Any())
				return ImmutableArray<EventMessage>.Empty;

			foreach (var item in items)
				item.NextVisible = item.NextVisible.AddSeconds(5);

			return items;
		}

		public void Remove(string connectionId)
		{
			var items = All().Where(f => string.Compare(f.Connection, connectionId, true) == 0);

			foreach (var item in items)
				Items.Remove(item);
		}

		public void Remove(ulong id)
		{
			if(All().FirstOrDefault(f => f.Id == id) is EventMessage message)
				Items.Remove(message);
		}

		public void Remap(string connection)
		{
			foreach (var item in All())
				item.Connection = connection;
		}

		public void RemoveEvents(string eventName)
		{
			var obsolete = All().Where(f => string.Compare(f.Event, eventName, true) == 0);

			foreach (var o in obsolete)
				Items.Remove(o);
		}

		public void Add(EventMessage message)
		{
			Items.Add(message);
		}
	}
}
