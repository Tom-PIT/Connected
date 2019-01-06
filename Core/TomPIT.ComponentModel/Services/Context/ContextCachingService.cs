using System;
using System.Collections.Generic;
using TomPIT.Caching;

namespace TomPIT.Services.Context
{
	internal class ContextCachingService : IContextCachingService
	{
		private static Lazy<MemoryCache> _cache = new Lazy<MemoryCache>();

		private static MemoryCache Cache { get { return _cache.Value; } }

		public bool Exists(string key)
		{
			return Cache.Exists(key);
		}

		public bool IsEmpty(string key)
		{
			return Cache.IsEmpty(key);
		}

		public void CreateKey(string key)
		{
			Cache.CreateKey(key);
		}

		public List<T> All<T>(string key) where T : class
		{
			return Cache.All<T>(key);
		}

		public T Get<T>(string key, string id, CacheRetrieveHandler<T> retrieve) where T : class
		{
			return Cache.Get<T>(key, id, retrieve);
		}

		public void Clear(string key)
		{
			Cache.Clear(key);
		}

		public T Get<T>(string key, string id) where T : class
		{
			return Cache.Get<T>(key, id);
		}

		public T Get<T>(string key, Func<T, bool> predicate) where T : class
		{
			return Cache.Get<T>(key, predicate);
		}

		public T First<T>(string key) where T : class
		{
			return Cache.First<T>(key);
		}

		public List<T> Where<T>(string key, Func<T, bool> predicate) where T : class
		{
			return Cache.Where<T>(key, predicate);
		}

		public T Set<T>(string key, string id, T instance)
		{
			return Cache.Set<T>(key, id, instance);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration)
		{
			return Cache.Set<T>(key, id, instance, duration);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration)
		{
			return Cache.Set<T>(key, id, instance, duration, slidingExpiration);
		}

		public void Remove(string key, string id)
		{
			Cache.Remove(key, id);
		}

		public void Remove<T>(string key, Func<T, bool> predicate) where T : class
		{
			Cache.Remove<T>(key, predicate);
		}

		public string GenerateKey(params object[] parameters)
		{
			return Cache.GenerateKey(parameters);
		}

		public string GenerateRandomKey(string key)
		{
			return Cache.GenerateRandomKey(key);
		}

		public int Count(string key)
		{
			return Cache.Count(key);
		}
	}
}
