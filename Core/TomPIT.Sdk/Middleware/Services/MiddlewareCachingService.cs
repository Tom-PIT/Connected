using System;
using System.Collections.Generic;
using TomPIT.Caching;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareCachingService : MiddlewareComponent, IMiddlewareCachingService
	{
		public MiddlewareCachingService(IMiddlewareContext context) : base(context)
		{
		}

		public bool Exists(string key)
		{
			return Context.Tenant.GetService<IDataCachingService>().Exists(key);
		}

		public bool IsEmpty(string key)
		{
			return Context.Tenant.GetService<IDataCachingService>().IsEmpty(key);
		}

		public void CreateKey(string key)
		{
			Context.Tenant.GetService<IDataCachingService>().CreateKey(key);
		}

		public List<T> All<T>(string key) where T : class
		{
			return Context.Tenant.GetService<IDataCachingService>().All<T>(key);
		}

		public T Get<T>(string key, string id, CacheRetrieveHandler<T> retrieve) where T : class
		{
			return Context.Tenant.GetService<IDataCachingService>().Get<T>(key, id, retrieve);
		}

		public void Clear(string key)
		{
			Context.Tenant.GetService<IDataCachingService>().Clear(key);
		}

		public T Get<T>(string key, string id) where T : class
		{
			return Context.Tenant.GetService<IDataCachingService>().Get<T>(key, id);
		}

		public T Get<T>(string key, Func<dynamic, bool> predicate) where T : class
		{
			return Context.Tenant.GetService<IDataCachingService>().Get<T>(key, predicate);
		}

		public T Get<T>(string key, Func<dynamic, bool> predicate, CacheRetrieveHandler<T> retrieve) where T : class
		{
			return Context.Tenant.GetService<IDataCachingService>().Get(key, predicate, retrieve);
		}

		public T First<T>(string key) where T : class
		{
			return Context.Tenant.GetService<IDataCachingService>().First<T>(key);
		}

		public List<T> Where<T>(string key, Func<dynamic, bool> predicate) where T : class
		{
			return Context.Tenant.GetService<IDataCachingService>().Where<T>(key, predicate);
		}

		public T Set<T>(string key, string id, T instance) where T : class
		{
			return Context.Tenant.GetService<IDataCachingService>().Set(key, id, instance);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration) where T : class
		{
			return Context.Tenant.GetService<IDataCachingService>().Set(key, id, instance, duration);
		}

		public T Set<T>(string key, string id, T instance, TimeSpan duration, bool slidingExpiration) where T : class
		{
			return Context.Tenant.GetService<IDataCachingService>().Set(key, id, instance, duration, slidingExpiration);
		}

		public void Remove(string key, string id)
		{
			Context.Tenant.GetService<IDataCachingService>().Remove(key, new List<string> { id });
		}

		public void Remove(string key, List<string> ids)
		{
			Context.Tenant.GetService<IDataCachingService>().Remove(key, ids);
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
			return Context.Tenant.GetService<IDataCachingService>().Count(key);
		}

		public void Reset(string key)
		{
			Context.Tenant.GetService<IDataCachingService>().Reset(key);
		}
	}
}
