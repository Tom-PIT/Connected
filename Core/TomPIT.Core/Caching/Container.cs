using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Caching
{
	internal class Container
	{
		private ConcurrentDictionary<string, Entries> _items = null;

		private ConcurrentDictionary<string, Entries> Items => _items ??= new ConcurrentDictionary<string, Entries>();

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
				Items.TryRemove(i, out _);
		}

		public bool IsEmpty(string key)
		{
			if (!Items.ContainsKey(key))
				return false;

			return Items[key].Count > 0;
		}

		public IEnumerator<T> GetEnumerator<T>(string key)
		{
			if (!Items.ContainsKey(key))
				return null;

			return Items[key].GetEnumerator<T>();
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
			return Items.ContainsKey(key) && Items[key].Exists(id);
		}

		public Entry Get(string key, string id)
		{
			return Items.ContainsKey(key) ? Items[key].Get(id) : null;
		}

		public Entry First(string key)
		{
			return Items.ContainsKey(key) ? Items[key].First() : null;
		}

		public Entry Get<T>(string key, Func<T, bool> predicate) where T : class
		{
			return Items.ContainsKey(key) ? Items[key].Get(predicate) : null;
		}

		public Entry Get<T>(string key, Func<dynamic, bool> predicate) where T : class
		{
			return Items.ContainsKey(key) ? Items[key].Get<T>(predicate) : null;
		}

		public List<T> Where<T>(string key, Func<T, bool> predicate) where T : class
		{
			return Items.ContainsKey(key) ? Items[key].Where(predicate) : null;
		}

		public List<string> Remove<T>(string key, Func<T, bool> predicate) where T : class
		{
			return Items.ContainsKey(key) ? Items[key].Remove(predicate) : null;
		}

		public ICollection<string> Keys(string key)
		{
			return Items.ContainsKey(key) ? Items[key].Keys : null;
		}
	}
}
