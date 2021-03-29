using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
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
			if (Items.TryGetValue(key, out Entries value))
				return value.Any();

			return true;
		}

		public IEnumerator<T> GetEnumerator<T>(string key)
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.GetEnumerator<T>();

			return null;
		}

		public ImmutableList<T> All<T>(string key) where T : class
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.All<T>();

			return ImmutableList<T>.Empty;
		}

		public int Count(string key)
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.Count;

			return 0;
		}

		public void Remove(string key)
		{
			if (Items.TryGetValue(key, out Entries value))
				value.Clear();
		}

		public void CreateKey(string key)
		{
			if (Exists(key))
				return;

			Items.TryAdd(key, new Entries());
		}

		public void Remove(string key, string id)
		{
			if (Items.TryGetValue(key, out Entries value))
				value.Remove(id);
		}

		public void Set(string key, string id, object instance, TimeSpan duration, bool slidingExpiration)
		{
			if (!Items.TryGetValue(key, out Entries value))
			{
				value = new Entries();

				if (!Items.TryAdd(key, value))
					return;
			}

			value.Set(id, instance, duration, slidingExpiration);
		}


		public bool Exists(string key)
		{
			return Items.ContainsKey(key);
		}

		public bool Exists(string key, string id)
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.Exists(id);

			return false;
		}

		public Entry Get(string key, string id)
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.Get(id);

			return null;
		}

		public Entry First(string key)
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.First();

			return null;
		}

		public Entry Get<T>(string key, Func<T, bool> predicate) where T : class
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.Get(predicate);

			return null;
		}

		public Entry Get<T>(string key, Func<dynamic, bool> predicate) where T : class
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.Get(predicate);

			return null;
		}

		public ImmutableList<T> Where<T>(string key, Func<T, bool> predicate) where T : class
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.Where(predicate);

			return null;
		}

		public List<string> Remove<T>(string key, Func<T, bool> predicate) where T : class
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.Remove(predicate);

			return null;
		}

		public ImmutableList<string> Keys(string key)
		{
			if (Items.TryGetValue(key, out Entries value))
				return value.Keys;

			return ImmutableList<string>.Empty;
		}
	}
}
