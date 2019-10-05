using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Caching
{
	internal class DataCachingService : TenantObject, IDataCachingService, IDataCachingNotification
	{
		private static Lazy<MemoryCache> _cache = new Lazy<MemoryCache>();
		private static readonly Lazy<ConcurrentDictionary<string, CachingHandlerState>> _states = new Lazy<ConcurrentDictionary<string, CachingHandlerState>>();

		private static MemoryCache Cache { get { return _cache.Value; } }
		private static ConcurrentDictionary<string, CachingHandlerState> States { get { return _states.Value; } }

		public DataCachingService(ITenant tenant) : base(tenant)
		{
			Cache.Invalidate += OnInvalidate;
		}

		private void OnInvalidate(CacheEventArgs e)
		{
			if (!States.ContainsKey(e.Key))
				return;

			var handler = States[e.Key];

			if (!handler.Initialized)
			{
				lock (handler.Handler)
				{
					if (handler.Initialized)
						return;

					try
					{
						handler.Initialized = true;
						handler.Handler.Initialize();
					}
					catch
					{
						handler.Initialized = false;
						throw;
					}
				}
			}
			else
				handler.Handler.Invalidate(e.Id);
		}

		public void Clear(string cacheKey)
		{
			Cache.Clear(cacheKey);

			var u = Tenant.CreateUrl("DataCache", "Clear");
			var e = new JObject
			{
				{"key", cacheKey }
			};

			Tenant.Post(u, e);
		}

		public void Invalidate(string cacheKey, List<string> ids)
		{
			foreach (var i in ids)
				Cache.Refresh(cacheKey, i);

			var u = Tenant.CreateUrl("DataCache", "Invalidate");
			var e = new JObject
			{
				{"key", cacheKey }
			};
			var a = new JArray();

			e.Add("ids", a);

			foreach (var i in ids)
				a.Add(i);

			Tenant.Post(u, e);
		}

		public void RegisterHandler(string cacheKey, IDataCachingHandler handler)
		{
			if (States.ContainsKey(cacheKey))
				States[cacheKey].Handler = handler;
			else
			{
				States.TryAdd(cacheKey, new CachingHandlerState
				{
					Handler = handler
				});
			}
		}

		public void Remove(string cacheKey, List<string> ids)
		{
			foreach (var i in ids)
				Cache.Remove(cacheKey, i);

			PublishRemove(cacheKey, ids);
		}

		private void PublishRemove(string cacheKey, List<string> ids)
		{
			var u = Tenant.CreateUrl("DataCache", "Remove");
			var e = new JObject
			{
				{"key", cacheKey }
			};
			var a = new JArray();

			e.Add("ids", a);

			foreach (var i in ids)
				a.Add(i);

			Tenant.Post(u, e);
		}

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
			Initialize(key);

			var items = Cache.All<object>(key);
			var result = new List<T>();

			foreach (var item in items)
				result.Add(Marshall.Convert<T>(item));

			return result;
		}

		public T Get<T>(string key, string id, CacheRetrieveHandler<T> retrieve) where T : class
		{
			Initialize(key);

			var item = Cache.Get<object>(key, id);

			if (item == null)
			{
				var options = new EntryOptions
				{
					AllowNull = false,
					Duration = TimeSpan.Zero,
					SlidingExpiration = true
				};

				item = retrieve(options);

				if (item != null && options.AllowNull)
					Set(key, id, item, options.Duration, options.SlidingExpiration);
			}

			return Marshall.Convert<T>(item);
		}

		public T Get<T>(string key, Func<T, bool> predicate, CacheRetrieveHandler<T> retrieve) where T : class
		{
			Initialize(key);

			var item = Get(key, predicate);

			if (item == null)
			{
				var options = new EntryOptions
				{
					AllowNull = false,
					Duration = TimeSpan.Zero,
					SlidingExpiration = true
				};

				item = retrieve(options);

				if (item != null && options.AllowNull)
					Set(key, options.Key, item, options.Duration, options.SlidingExpiration);
			}

			return Marshall.Convert<T>(item);
		}

		public T Get<T>(string key, string id) where T : class
		{
			Initialize(key);

			return Marshall.Convert<T>(Cache.Get<object>(key, id));
		}

		public T Get<T>(string key, Func<T, bool> predicate) where T : class
		{
			Initialize(key);

			var items = Where(key, predicate);

			if (items == null || items.Count == 0)
				return default;

			return items.FirstOrDefault(predicate);
		}

		public T First<T>(string key) where T : class
		{
			Initialize(key);

			return Marshall.Convert<T>(Cache.First<object>(key));
		}

		public List<T> Where<T>(string key, Func<T, bool> predicate) where T : class
		{
			Initialize(key);

			var items = All<T>(key);

			if (items == null || items.Count == 0)
				return new List<T>();

			return items.Where(predicate).ToList();
		}

		public T Set<T>(string key, string id, T instance) where T : class
		{
			Initialize(key);

			return Cache.Set(key, id, instance);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration) where T : class
		{
			Initialize(key);

			return Cache.Set(key, id, instance, duration);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration) where T : class
		{
			Initialize(key);

			return Cache.Set(key, id, instance, duration, slidingExpiration);
		}

		public int Count(string key)
		{
			Initialize(key);

			return Cache.Count(key);
		}

		public string GenerateKey(params object[] parameters)
		{
			return Cache.GenerateKey(parameters);
		}

		public string GenerateRandomKey(string key)
		{
			Initialize(key);

			return Cache.GenerateRandomKey(key);
		}

		public void NotifyClear(DataCacheEventArgs e)
		{
			Cache.Clear(e.Key);
		}

		public void NotifyInvalidate(DataCacheEventArgs e)
		{
			foreach (var i in e.Ids)
				Cache.Refresh(e.Key, i);
		}

		public void NotifyRemove(DataCacheEventArgs e)
		{
			foreach (var i in e.Ids)
				Cache.Remove(e.Key, i);
		}

		private void Initialize(string key)
		{
			if (!States.ContainsKey(key))
				return;

			var handler = States[key];

			if (!handler.Initialized)
			{
				lock (handler.Handler)
				{
					if (handler.Initialized)
						return;

					try
					{
						handler.Initialized = true;
						handler.Handler.Initialize();
					}
					catch
					{
						handler.Initialized = false;

						throw;
					}
				}

			}
		}

		public void Reset(string cacheKey)
		{
			if (!States.ContainsKey(cacheKey))
				return;

			States[cacheKey].Initialized = false;

			Cache.Clear(cacheKey);
		}
	}
}
