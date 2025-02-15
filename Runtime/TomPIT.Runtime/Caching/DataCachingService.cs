﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Serialization;

namespace TomPIT.Caching
{
	internal class DataCachingService : TenantObject, IDataCachingService, IDataCachingNotification
	{
		private readonly Lazy<SingletonProcessor<string>> _cacheItemProcessor = new(System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

		private MemoryCache _cache;

		private SingletonProcessor<string> CacheItemProcessor => _cacheItemProcessor.Value;

		public DataCachingService(ITenant tenant) : this(tenant, CacheScope.Shared)
		{
		}

		public DataCachingService(ITenant tenant, CacheScope scope) : base(tenant)
		{
			Scope = scope;
			_cache = new MemoryCache(scope);
		}

		private CacheScope Scope { get; set; }

		public void Clear(string cacheKey)
		{
			Cache.Clear(cacheKey);

			if (Scope == CacheScope.Shared)
				Instance.SysProxy.DataCache.Clear(cacheKey);
		}

		public void Invalidate(string cacheKey, List<string> ids)
		{
			foreach (var i in ids)
				Cache.Refresh(cacheKey, i);

			if (Scope == CacheScope.Shared)
				Instance.SysProxy.DataCache.Invalidate(cacheKey, ids);
		}

		public void Remove<T>(IMiddlewareContext context, string key, Func<dynamic, bool> predicate) where T : class
		{
			var items = Where<T>(context, key, predicate);

			if (items is null || items.IsEmpty())
				return;

			var ids = new List<string>();

			if (ReflectionExtensions.CacheKeyProperty(items[0]) is not PropertyInfo cacheProperty)
				return;

			foreach (var i in items)
			{
				if (!Types.TryConvertInvariant(cacheProperty.GetValue(i), out string id))
					continue;

				ids.Add(id);
				Cache.Remove(key, id);
			}

			if (!ids.IsEmpty())
				PublishRemove(key, ids);
		}

		public void Remove(string cacheKey, List<string> ids)
		{
			foreach (var i in ids)
				Cache.Remove(cacheKey, i);

			PublishRemove(cacheKey, ids);
		}

		private void PublishRemove(string key, List<string> ids)
		{
			if (Scope == CacheScope.Shared)
				Instance.SysProxy.DataCache.Remove(key, ids);
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
			var items = Cache.All<CacheValue>(key);
			var result = new List<T>();

			foreach (var item in items)
				result.Add(Deserialize<T>(context, item.Value));

			return result;
		}

		public T Get<T>(IMiddlewareContext context, string key, string id, CacheRetrieveHandler<T> retrieve) where T : class
		{
			var item = Cache.Get<CacheValue>(key, id);

			if (item is CacheValue value)
				return Deserialize<T>(context, value.Value);

			return RetrieveAndSet(context, key, id, retrieve);
		}

		public T Get<T>(IMiddlewareContext context, string key, Func<dynamic, bool> predicate, CacheRetrieveHandler<T> retrieve) where T : class
		{
			if (Cache.All<CacheValue>(key) is ImmutableList<CacheValue> all && all.Any())
			{
				if (all.FirstOrDefault(f => predicate(f.Key)) is CacheValue target)
					return Deserialize<T>(context, target.Value);
			}

			return RetrieveAndSet(context, key, null, retrieve);
		}

		public T Get<T>(IMiddlewareContext context, string key, Func<T, bool> evaluator, CacheRetrieveHandler<T> retrieve) where T : class
		{
			var enumerator = Cache.GetEnumerator<CacheValue>(key);

			while (enumerator?.MoveNext() ?? false)
			{
				var instance = Deserialize<T>(context, enumerator.Current.Value);

				if (evaluator(instance))
					return instance;
			}

			return RetrieveAndSet(context, key, null, retrieve);
		}

		public T Get<T>(IMiddlewareContext context, string key, string id) where T : class
		{
			var item = Cache.Get<CacheValue>(key, id);

			if (item == null)
				return default;

			return Deserialize<T>(context, item.Value);
		}

		public T Get<T>(IMiddlewareContext context, string key, Func<dynamic, bool> predicate) where T : class
		{
			var items = Where<T>(context, key, predicate);

			if (items is null || items.Count == 0)
				return default;

			return items.FirstOrDefault(predicate);
		}

		public T First<T>(IMiddlewareContext context, string key) where T : class
		{
			var first = Cache.First<CacheValue>(key);

			if (first == null)
				return default;

			return Deserialize<T>(context, first.Value);
		}

		public List<T> Where<T>(IMiddlewareContext context, string key, Func<dynamic, bool> predicate) where T : class
		{
			var results = Cache.Where<CacheValue>(key, f => predicate(f.Key));

			if (results == null || results.Count == 0)
				return new List<T>();

			var r = new List<T>();

			foreach (var result in results)
				r.Add(Deserialize<T>(context, result));

			return r;
		}

		public T Set<T>(string key, string id, T instance) where T : class
		{
			Cache.Set(key, id, new CacheValue
			{
				Key = CreateKey(instance),
				Value = Serializer.Serialize(instance)
			}, TimeSpan.FromMinutes(1), true);

			return instance;
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration) where T : class
		{
			Cache.Set(key, id, new CacheValue
			{
				Key = CreateKey(instance),
				Value = Serializer.Serialize(instance)
			}, duration);

			return instance;
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration) where T : class
		{
			Cache.Set(key, id, new CacheValue
			{
				Key = CreateKey(instance),
				Value = Serializer.Serialize(instance)
			}, duration, slidingExpiration);

			return instance;
		}

		public int Count(string key)
		{
			return Cache.Count(key);
		}

		public string GenerateKey(params object[] parameters)
		{
			return Cache.GenerateKey(parameters);
		}

		public string GenerateRandomKey(string key)
		{
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

		public void Reset(string cacheKey)
		{
			Cache.Clear(cacheKey);
		}

		private T RetrieveAndSet<T>(IMiddlewareContext context, string key, string id, CacheRetrieveHandler<T> retrieve) where T : class
		{
			if (retrieve is null)
				return default;

			T entity = default;

			//Predicate was used 
			if (string.IsNullOrWhiteSpace(id))
			{
				var result = Retrieve(id, retrieve, out var options);
				TryStoring(context, key, result, options);
				return result;
			}

			CacheItemProcessor.Start($"{key}_{id}",
				 () =>
				 {
					 var result = Retrieve(id, retrieve, out var options);
					 TryStoring(context, key, result, options);
					 entity = result;
				 },
				 () =>
				 {
					 entity = Get<T>(context, key, id);
				 });

			return entity;
		}

		private static T Retrieve<T>(string key, CacheRetrieveHandler<T> retrieve, out EntryOptions options) where T : class
		{
			options = new EntryOptions
			{
				AllowNull = false,
				Duration = TimeSpan.FromMinutes(2),
				SlidingExpiration = true,
				Key = key
			};

			var result = retrieve(options);

			if (result is not null || options.AllowNull)
			{
				if (string.IsNullOrWhiteSpace(options.Key))
					options.Key = ReflectionExtensions.ResolveCacheKey(result);
			}

			return result;
		}

		private bool TryStoring<T>(IMiddlewareContext context, string key, T item, EntryOptions options) where T : class
		{
			if (CanStore(options) && item is not null || options.AllowNull)
			{
				if (string.IsNullOrWhiteSpace(options.Key))
				{
					context.Tenant.LogWarning(nameof(DataCachingService), $"{SR.ErrCacheKeyNull} ({item?.GetType().Name})", LogCategories.Middleware);
					return false;
				}
				else
				{
					Set(key, options.Key, item, options.Duration, options.SlidingExpiration);
					return true;
				}
			}

			return false;
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
			var result = Serializer.Deserialize<T>(value);

			if (result == null || result.GetType().IsPrimitive || result.GetType().IsCollection())
				return result;

			var props = result.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

			foreach (var property in props)
			{
				if (property.PropertyType == typeof(DateTimeOffset) && property.CanWrite)
				{
					var utc = (DateTimeOffset)property.GetValue(result);

					property.SetValue(result, context.Services.Globalization.FromUtc(utc));
				}
			}

			return result;
		}

		public IDataCachingService CreateService(ITenant tenant)
		{
			return new DataCachingService(tenant, CacheScope.Context);
		}

		public void Merge(IMiddlewareContext context, IDataCachingService service)
		{
			if (service is not DataCachingService ctx)
				throw new ArgumentException(null, nameof(service));

			Cache.Merge(ctx.Cache);
		}

		private bool CanStore(EntryOptions options)
		{
			if (Scope == CacheScope.Context)
				return true;

			return options.Scope != CacheScope.Context;
		}

		public void Dispose()
		{
			if (_cache is null)
				return;

			_cache.Dispose();
			_cache = null;
		}

		private MemoryCache Cache => _cache;
	}
}
