using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Caching
{
	internal class Container
	{
		private ConcurrentDictionary<string, Entries> _items = null;

		private ConcurrentDictionary<string, Entries> Items
		{
			get
			{
				if (_items == null)
					_items = new ConcurrentDictionary<string, Entries>();

				return _items;
			}
		}

		public void Clear()
		{
			foreach (var i in Items)
				i.Value.Clear();

			Items.Clear();
		}

		public void Scave()
		{
			foreach (var i in Items)
				i.Value.Scave();

			var empties = Items.Where(f => f.Value.Count == 0).Select(f => f.Key);

			foreach (var i in empties)
				Items.TryRemove(i, out Entries d);
		}

		public bool IsEmpty(string key)
		{
			if (!Items.ContainsKey(key))
				return false;

			return Items[key].Count > 0;
		}

		public List<T> All<T>(string key) where T : class
		{
			if (!Items.ContainsKey(key))
				return null;

			return Items[key].All<T>();
		}

		public int Count(string key)
		{
			if (!Items.ContainsKey(key))
				return 0;

			return Items[key].Count;
		}

		public void Remove(string key)
		{
			if (!Items.ContainsKey(key))
				return;

			var d = Items[key];

			if (d != null)
				d.Clear();
		}

		public void CreateKey(string key)
		{
			if (Exists(key))
				return;

			Items.TryAdd(key, new Entries());
		}

		public void Remove(string key, string id)
		{
			if (!Items.ContainsKey(key))
				return;

			var d = Items[key];

			if (d != null)
				d.Remove(id);
		}

		public void Set(string key, string id, object instance, TimeSpan duration, bool slidingExpiration)
		{
			Entries d = null;

			if (Items.ContainsKey(key))
				d = Items[key];

			if (d == null)
			{
				d = new Entries();

				if (!Items.TryAdd(key, d))
					return;
			}

			d.Set(id, instance, duration, slidingExpiration);
		}


		public bool Exists(string key)
		{
			return Items.ContainsKey(key);
		}

		public bool Exists(string key, string id)
		{
			if (!Items.ContainsKey(key))
				return false;

			return Items[key].Exists(id);
		}

		public Entry Get(string key, string id)
		{
			if (!Items.ContainsKey(key))
				return null;

			var d = Items[key];

			return d.Get(id);
		}

		public Entry First(string key)
		{
			if (!Items.ContainsKey(key))
				return null;

			var d = Items[key];

			return d.First();
		}

		public Entry Get<T>(string key, Func<T, bool> predicate) where T : class
		{
			if (!Items.ContainsKey(key))
				return null;

			var d = Items[key];

			return d.Get<T>(predicate);
		}

		public Entry Get<T>(string key, Func<dynamic, bool> predicate) where T : class
		{
			if (!Items.ContainsKey(key))
				return null;

			var d = Items[key];

			return d.Get<T>(predicate);
		}

		public List<T> Where<T>(string key, Func<T, bool> predicate) where T : class
		{
			if (!Items.ContainsKey(key))
				return null;

			var d = Items[key];

			return d.Where<T>(predicate);
		}

		public List<string> Remove<T>(string key, Func<T, bool> predicate) where T : class
		{
			if (!Items.ContainsKey(key))
				return null;

			var d = Items[key];

			return d.Remove(predicate);
		}

		public ICollection<string> Keys(string key)
		{
			if (!Items.ContainsKey(key))
				return null;

			var d = Items[key];

			return d.Keys;
		}
	}
}
