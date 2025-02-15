﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TomPIT.Caching
{
	internal class Entries
	{
		private readonly Lazy<ConcurrentDictionary<string, Entry>> _items = new Lazy<ConcurrentDictionary<string, Entry>>();

		private ConcurrentDictionary<string, Entry> Items => _items.Value;
		public ImmutableList<string> Keys => Items.Keys.ToImmutableList();
		public int Count => Items.Count;

		public bool Any()
		{
			return Items.Any();
		}
		public void Scave()
		{
			var expired = new HashSet<string>();

			foreach (var i in Items)
			{
				var r = i.Value;

				if (r == null || r.Expired)
					expired.Add(i.Key);
			}

			foreach (var i in expired)
				Remove(i);
		}

		public ImmutableList<T> All<T>() where T : class
		{
			var r = new List<T>();

			var expired = Items.Where(f => f.Value.Expired);

			foreach (var e in expired)
				Remove(e.Value.Id);

			var instances = Items.Select(f => f.Value.Instance);

			foreach (var i in instances)
				r.Add(i as T);

			return r.ToImmutableList();
		}

		public void Remove(string key)
		{
			if (Items.IsEmpty)
				return;

			if (Items.TryRemove(key, out Entry v))
				v?.Dispose();
		}

		public void Set(string key, object instance, Entry value)
		{
			if (value is not Entry)
				return;

			Items[key] = value;
		}

		public void Set(string key, object instance, TimeSpan duration, bool slidingExpiration, CacheScope scope)
		{
			Items[key] = new Entry(key, instance, duration, slidingExpiration, scope);
		}

		public IEnumerator<T> GetEnumerator<T>()
		{
			return new EntryEnumerator<T>(Items);
		}
		public Entry Get(string key)
		{
			return Find(key);
		}

		public Entry First()
		{
			if (Count == 0)
				return null;

			return Items.First().Value;
		}

		public Entry Get<T>(Func<T, bool> predicate) where T : class
		{
			return Find(predicate);
		}

		public Entry Get<T>(Func<dynamic, bool> predicate) where T : class
		{
			return Find<T>(predicate);
		}

		public List<string> Remove<T>(Func<T, bool> predicate) where T : class
		{
			var ds = Where(predicate);

			if (ds == null)
				return null;

			var result = new HashSet<string>();

			foreach (var i in ds)
			{
				var key = Items.FirstOrDefault(f => f.Value?.Instance == i).Key;

				RemoveInternal(key);

				result.Add(key);
			}

			return result.ToList();
		}

		public ImmutableList<T> Where<T>(Func<T, bool> predicate) where T : class
		{
			var values = Items.Values.Select(f => f.Instance).Cast<T>();

			if (values == null || !values.Any())
				return null;

			var filtered = values.Where(predicate);

			if (filtered == null || !filtered.Any())
				return null;

			var r = new List<T>();

			foreach (var i in filtered)
			{
				var ce = Items.FirstOrDefault(f => f.Value?.Instance == i);

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

			return r.ToImmutableList();
		}

		private void RemoveInternal(string key)
		{
			if (Items.TryRemove(key, out Entry d))
				d.Dispose();
		}

		private Entry Find<T>(Func<T, bool> predicate) where T : class
		{
			var instances = Items.Values.Select(f => f?.Instance).Cast<T>();

			if (instances == null || !instances.Any())
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

		private Entry Find<T>(Func<dynamic, bool> predicate) where T : class
		{
			var instances = Items.Values.Select(f => f?.Instance).Cast<dynamic>();

			if (instances == null || !instances.Any())
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
				if (d is not null && d.Expired)
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
	}
}