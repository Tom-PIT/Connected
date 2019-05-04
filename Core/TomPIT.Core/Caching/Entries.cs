using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.Caching
{
	internal class Entries
	{
		private Lazy<ConcurrentDictionary<string, Entry>> _items = new Lazy<ConcurrentDictionary<string, Entry>>();

		private ConcurrentDictionary<string, Entry> Items { get { return _items.Value; } }

		public void Scave()
		{
			var expired = new List<string>();

			foreach (var i in Items.Keys)
			{
				var r = Items[i];

				if (r == null || r.Expired)
					expired.Add(i);
			}

			foreach (var i in expired)
			{
				if (Items.TryRemove(i, out Entry d))
					d.Dispose();
			}
		}

		public List<T> All<T>() where T : class
		{
			var r = new List<T>();

			foreach (var i in Items.Values)
			{
				if (i.Expired)
					Remove(i.Id);
				else
					r.Add(i.Instance as T);
			}

			return r;
		}

		public void Remove(string key)
		{
			if (Items.Count == 0)
				return;

			if (Items.TryRemove(key, out Entry v))
				v.Dispose();
		}

		public ICollection<String> Keys { get { return Items.Keys; } }
		public void Set(string key, object instance, TimeSpan duration, bool slidingExpiration)
		{
			Items[key] = new Entry(key, instance, duration, slidingExpiration);
		}

		public Entry Get(string key)
		{
			return Find(key);
		}


		public Entry First()
		{
			if (Count == 0)
				return null;

			return Items.ElementAt(0).Value;
		}

		public Entry Get<T>(Func<T, bool> predicate) where T : class
		{
			return Find<T>(predicate);
		}

		public List<string> Remove<T>(Func<T, bool> predicate) where T : class
		{
			var ds = Where(predicate);

			if (ds == null)
				return null;

			var result = new List<string>();

			foreach (var i in ds)
			{
				var key = Items.FirstOrDefault(f => f.Value != null && f.Value.Instance == i).Key;

				RemoveInternal(key);

				result.Add(key);
			}

			return result;
		}

		public List<T> Where<T>(Func<T, bool> predicate) where T : class
		{
			var values = Items.Values.Select(f => f.Instance).Cast<T>();

			if (values == null || values.Count() == 0)
				return null;

			var filtered = values.Where(predicate);

			if (filtered == null || filtered.Count() == 0)
				return null;

			var r = new List<T>();

			foreach (var i in filtered)
			{
				var ce = Items.FirstOrDefault(f => f.Value.Instance == i);

				if (ce.Value == null)
					continue;

				if (ce.Value.Expired)
				{
					RemoveInternal(ce.Value.Id);
					continue;
				}

				ce.Value.Hit();
				r.Add(i);
			}

			return r;
		}

		private void RemoveInternal(string key)
		{
			if (Items.TryRemove(key, out Entry d))
				d.Dispose();
		}

		private Entry Find<T>(Func<T, bool> predicate) where T : class
		{
			var instances = Items.Values.Select(f => f.Instance).Cast<T>();

			if (instances == null || instances.Count() == 0)
				return null;

			T instance = instances.FirstOrDefault(predicate);

			if (instance == null)
				return null;

			var ce = Items.Values.FirstOrDefault(f => f.Instance == instance);

			if (ce == null)
				return null;

			if (ce.Expired)
			{
				RemoveInternal(ce.Id);
				return null;
			}

			ce.Hit();

			return ce;
		}

		private Entry Find(string key)
		{
			if (!Items.ContainsKey(key))
				return null;

			if (Items.TryGetValue(key, out Entry d))
			{
				if (d.Expired)
				{
					RemoveInternal(key);
					return null;
				}

				d.Hit();

				return d;
			}
			else
			{
				RemoveInternal(key);

				return null;
			}
		}

		public bool Exists(string key)
		{
			return Find(key) != null;
		}

		public void Clear()
		{
			foreach (var i in Items)
				Remove(i.Key);
		}

		public int Count { get { return Items.Count; } }
	}
}