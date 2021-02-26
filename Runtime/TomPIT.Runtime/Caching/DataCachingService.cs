using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
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

		public List<T> All<T>(IMiddlewareContext context, string key) where T : class
		{
			Initialize(key);

			var items = Cache.All<CacheValue>(key);
			var result = new List<T>();

			foreach (var item in items)
				result.Add(Deserialize<T>(context, item.Value));

			return result;
		}

		public T Get<T>(IMiddlewareContext context, string key, string id, CacheRetrieveHandler<T> retrieve) where T : class
		{
			Initialize(key);

			var item = Cache.Get<CacheValue>(key, id);

			if (item == null)
			{
				var options = new EntryOptions
				{
					AllowNull = false,
					Duration = TimeSpan.Zero,
					SlidingExpiration = true
				};

				var result = retrieve(options);

				if (result != null || options.AllowNull)
					Set(key, id, result, options.Duration, options.SlidingExpiration);

				return result;
			}

			return Deserialize<T>(context, item.Value);
		}

		public T Get<T>(IMiddlewareContext context, string key, Func<dynamic, bool> predicate, CacheRetrieveHandler<T> retrieve) where T : class
		{
			Initialize(key);

			var all = Cache.All<CacheValue>(key);

			if (all != null && all.Count > 0)
			{
				var target = all.FirstOrDefault(f => predicate(f.Key));

				if (target != null)
					return Deserialize<T>(context, target.Value);
			}

			var options = new EntryOptions
			{
				AllowNull = false,
				Duration = TimeSpan.Zero,
				SlidingExpiration = true
			};

			var result = retrieve(options);

			if (result != null || options.AllowNull)
			{
				if (string.IsNullOrWhiteSpace(options.Key))
					options.Key = CreateKeyFromAttributes(result);

				if (string.IsNullOrWhiteSpace(key))
					throw new RuntimeException(SR.ErrCacheKeyNull);

				Set(key, options.Key, result, options.Duration, options.SlidingExpiration);
			}

			return result;
		}

		public T Get<T>(IMiddlewareContext context, string key, Func<T, bool> evaluator, CacheRetrieveHandler<T> retrieve) where T : class
		{
			Initialize(key);

			var enumerator = Cache.GetEnumerator<CacheValue>(key);

			if (enumerator != null)
			{
				while (enumerator.MoveNext())
				{
					var instance = Deserialize<T>(context, enumerator.Current.Value);

					if (evaluator(instance))
						return instance;
				}
			}

			var options = new EntryOptions
			{
				AllowNull = false,
				Duration = TimeSpan.Zero,
				SlidingExpiration = true
			};

			var result = retrieve(options);

			if (result != null || options.AllowNull)
			{
				if (string.IsNullOrWhiteSpace(options.Key))
					options.Key = CreateKeyFromAttributes(result);

				if (string.IsNullOrWhiteSpace(key))
					throw new RuntimeException(SR.ErrCacheKeyNull);

				Set(key, options.Key, result, options.Duration, options.SlidingExpiration);
			}

			return result;
		}

		private string CreateKeyFromAttributes(object instance)
		{
			var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var key = new StringBuilder();

			foreach (var property in properties)
			{
				var att = property.FindAttribute<CacheKeyAttribute>();

				if (att == null)
					continue;

				var value = property.GetValue(instance);

				if (key.Length > 0)
					key.Append('/');

				if (value == null)
					continue;

				key.Append(Types.Convert<string>(value, CultureInfo.InvariantCulture));
			}

			return key.ToString();
		}

		public T Get<T>(IMiddlewareContext context, string key, string id) where T : class
		{
			Initialize(key);

			var item = Cache.Get<CacheValue>(key, id);

			if (item == null)
				return default;

			return Deserialize<T>(context, item.Value);
		}

		public T Get<T>(IMiddlewareContext context, string key, Func<dynamic, bool> predicate) where T : class
		{
			Initialize(key);

			var items = Where<T>(context, key, predicate);

			if (items == null || items.Count == 0)
				return default;

			return items.FirstOrDefault(predicate);
		}

		public T First<T>(IMiddlewareContext context, string key) where T : class
		{
			Initialize(key);

			var first = Cache.First<CacheValue>(key);

			if (first == null)
				return default;

			return Deserialize<T>(context, first.Value);
		}

		public List<T> Where<T>(IMiddlewareContext context, string key, Func<dynamic, bool> predicate) where T : class
		{
			Initialize(key);

			var items = All<CacheValue>(context, key);

			if (items == null || items.Count == 0)
				return new List<T>();

			var results = items.Where(f => predicate(f.Key)).ToList();

			if (results == null || results.Count == 0)
				return new List<T>();

			var r = new List<T>();

			foreach (var result in results)
				r.Add(Deserialize<T>(context, result));

			return r;
		}

		public T Set<T>(string key, string id, T instance) where T : class
		{
			Initialize(key);

			Cache.Set(key, id, new CacheValue
			{
				Key = CreateKey(instance),
				Value = Serializer.Serialize(instance)
			});

			return instance;
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration) where T : class
		{
			Initialize(key);

			Cache.Set(key, id, new CacheValue
			{
				Key = CreateKey(instance),
				Value = Serializer.Serialize(instance)
			}, duration);

			return instance;
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration) where T : class
		{
			Initialize(key);

			Cache.Set(key, id, new CacheValue
			{
				Key = CreateKey(instance),
				Value = Serializer.Serialize(instance)
			}, duration, slidingExpiration);

			return instance;
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

		private static dynamic CreateKey(object instance)
		{
			if (instance == null)
				return new ExpandoObject();

			var result = new ExpandoObject();

			if (instance.GetType().IsCollection())
				return result;

			var members = instance.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);

			foreach (var member in members)
			{
				if (member is PropertyInfo property && property.CanRead)
				{
					var att = property.FindAttribute<CachePropertyAttribute>();

					if (att == null || att.Visibility == CachePropertyVisibility.Visible)
					{
						var value = property.GetValue(instance);

						if (property.PropertyType == typeof(string) && !string.IsNullOrWhiteSpace(value as string) && (att == null || att.Storage == CachePropertyStorage.Optimized))
						{
							if (value.ToString().Length > 128)
								value = value.ToString().Substring(0, 128);
						}

						result.TryAdd(property.Name, value);
					}
				}
			}

			return result;
		}

		private static T Deserialize<T>(IMiddlewareContext context, CacheValue value)
		{
			if (value == null || string.IsNullOrEmpty(value.Value))
				return default;

			return Deserialize<T>(context, value.Value);
		}
		private static T Deserialize<T>(IMiddlewareContext context, string value)
		{
			var result =  Serializer.Deserialize<T>(value);

			if (result == null || result.GetType().IsPrimitive || result.GetType().IsCollection())
				return result;

			var props = result.GetType().GetProperties(BindingFlags.Instance| BindingFlags.Public);
			
			foreach(var property in props)
			{
				if(property.PropertyType == typeof(DateTimeOffset) && property.CanWrite)
				{
					var utc = (DateTimeOffset)property.GetValue(result);
					
					property.SetValue(result, context.Services.Globalization.FromUtc(utc));
				}
			}

			return result;
		}
	}
}
