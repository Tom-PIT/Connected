using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.Caching;
using TomPIT.Reflection;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareCachingService : MiddlewareComponent, IMiddlewareCachingService
	{
		private IDataCachingService _contextCache;
		public MiddlewareCachingService(IMiddlewareContext context) : base(context)
		{
		}

		public bool Exists(string key)
		{
			return ContextCache.Exists(key) || Context.Tenant.GetService<IDataCachingService>().Exists(key);
		}

		public bool IsEmpty(string key)
		{
			return ContextCache.IsEmpty(key) && Context.Tenant.GetService<IDataCachingService>().IsEmpty(key);
		}

		public void CreateKey(string key)
		{
			Context.Tenant.GetService<IDataCachingService>().CreateKey(key);
		}

		public List<T> All<T>(string key) where T : class
		{
			return Merge(ContextCache.All<T>(Context, key), Context.Tenant.GetService<IDataCachingService>().All<T>(Context, key));
		}

		public T Get<T>(string key, Func<T, bool> matchEvaluator, CacheRetrieveHandler<T> retrieve) where T : class
		{
			return ContextCache.Get(Context, key, matchEvaluator, (f) =>
			{
				var shared = Context.Tenant.GetService<IDataCachingService>().Get<T>(Context, key, matchEvaluator, null);

				if (shared is not null)
					return shared;

				return retrieve(f);
			});
		}

		public T Get<T>(string key, string id, CacheRetrieveHandler<T> retrieve) where T : class
		{
			return ContextCache.Get(Context, key, id, (f) =>
			{
				var shared = Context.Tenant.GetService<IDataCachingService>().Get<T>(Context, key, id);

				if (shared is not null)
					return shared;

				return retrieve(f);
			});
		}

		public T Get<T>(string key, Func<dynamic, bool> predicate, CacheRetrieveHandler<T> retrieve) where T : class
		{
			return ContextCache.Get(Context, key, predicate, (f) =>
			{
				var shared = Context.Tenant.GetService<IDataCachingService>().Get<T>(Context, key, predicate);

				if (shared is not null)
					return shared;

				return retrieve(f);
			});
		}


		public T Get<T>(string key, string id) where T : class
		{
			var contextItem = ContextCache.Get<T>(Context, key, id);

			if (contextItem is not null)
				return contextItem;

			return Context.Tenant.GetService<IDataCachingService>().Get<T>(Context, key, id);
		}

		public T Get<T>(string key, Func<dynamic, bool> predicate) where T : class
		{
			var contextItem = ContextCache.Get<T>(Context, key, predicate);

			if (contextItem is not null)
				return contextItem;

			return Context.Tenant.GetService<IDataCachingService>().Get<T>(Context, key, predicate);
		}

		public void Clear(string key)
		{
			ContextCache.Clear(key);
			Context.Tenant.GetService<IDataCachingService>().Clear(key);
		}
		public T First<T>(string key) where T : class
		{
			if (ContextCache.First<T>(Context, key) is T result)
				return result;

			return Context.Tenant.GetService<IDataCachingService>().First<T>(Context, key);
		}

		public List<T> Where<T>(string key, Func<dynamic, bool> predicate) where T : class
		{
			return Merge(ContextCache.Where<T>(Context, key, predicate), Context.Tenant.GetService<IDataCachingService>().Where<T>(Context, key, predicate));
		}

		public T Set<T>(string key, string id, T instance) where T : class
		{
			return ContextCache.Set(key, id, instance);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration) where T : class
		{
			return ContextCache.Set(key, id, instance, duration);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration) where T : class
		{
			return ContextCache.Set(key, id, instance, duration, slidingExpiration);
		}

		public void Remove(string key, string id)
		{
			ContextCache.Remove(key, new List<string> { id });
			Context.Tenant.GetService<IDataCachingService>().Remove(key, new List<string> { id });
		}

		public void Remove(string key, List<string> ids)
		{
			ContextCache.Remove(key, ids);
			Context.Tenant.GetService<IDataCachingService>().Remove(key, ids);
		}

		public void Remove<T>(string key, Func<dynamic, bool> predicate) where T : class
		{
			ContextCache.Remove<T>(Context, key, predicate);
			Context.Tenant.GetService<IDataCachingService>().Remove<T>(Context, key, predicate);
		}

		public string GenerateKey(params object[] parameters)
		{
			return Context.Tenant.GetService<IDataCachingService>().GenerateKey(parameters);
		}

		public string GenerateRandomKey(string key)
		{
			return Context.Tenant.GetService<IDataCachingService>().GenerateRandomKey(key);
		}

		public int Count(string key)
		{
			return ContextCache.Count(key);
		}

		public void Reset(string key)
		{
			ContextCache.Clear(key);
			Context.Tenant.GetService<IDataCachingService>().Reset(key);
		}

		public void Flush()
		{
			Context.Tenant.GetService<IDataCachingService>().Merge(Context, ContextCache);
		}

		private static List<T> Merge<T>(List<T> contextItems, List<T> sharedItems)
		{
			if (contextItems is null)
				return sharedItems;

			var result = new List<T>(contextItems);

			foreach (var sharedItem in sharedItems)
			{
				if (sharedItem is null)
					continue;

				if (ReflectionExtensions.CacheKeyProperty(sharedItem) is not PropertyInfo cacheProperty)
				{
					//Q: should we compare every property and add only if not matched?
					contextItems.Add(sharedItem);
					continue;
				}

				if (FindExisting(cacheProperty.GetValue(sharedItems), contextItems) is null)
					result.Add(sharedItem);
			}

			return result;
		}

		private static T FindExisting<T>(object value, List<T> items)
		{
			if (items.IsEmpty())
				return default;

			if (ReflectionExtensions.CacheKeyProperty(items[0]) is not PropertyInfo cacheProperty)
				return default;

			foreach (var item in items)
			{
				var id = cacheProperty.GetValue(item);

				if (Types.Compare(id, value))
					return item;
			}

			return default;
		}

		protected IDataCachingService ContextCache
		{
			get
			{
				if (_contextCache is null)
				{
					if (Context is MiddlewareContext mc && mc.Owner is not null && mc.Owner.Services.Cache is MiddlewareCachingService cache)
						return cache.ContextCache;
					else
						_contextCache = Context.Tenant.GetService<IDataCachingService>().CreateService(Context.Tenant);
				}

				return _contextCache;
			}
		}

		protected override void OnDisposing()
		{
			if(_contextCache is not null)
			{
				_contextCache.Dispose();
				_contextCache = null;
			}

			base.OnDisposing();
		}
	}
}
