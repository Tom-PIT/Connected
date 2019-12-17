using System;
using System.Collections.Generic;
using TomPIT.Caching;

namespace TomPIT.Data
{
	public abstract class Cache<T, K> : DataModel, IDataCachingHandler where T : class
	{
		protected Cache(string cacheKey)
		{
			CacheKey = cacheKey;

			Context.Tenant.GetService<IDataCachingService>().RegisterHandler(CacheKey, this);
		}

		private string CacheKey { get; }
		protected virtual TimeSpan Expiration => TimeSpan.FromSeconds(90);
		protected virtual bool SlidingExpiration => true;

		protected bool Exists()
		{
			return Context.Services.Cache.Exists(CacheKey);
		}

		protected bool IsEmpty()
		{
			return Context.Services.Cache.IsEmpty(CacheKey);
		}

		protected void CreateKey()
		{
			Context.Services.Cache.CreateKey(CacheKey);
		}

		protected List<T> All()
		{
			return Context.Services.Cache.All<T>(CacheKey);
		}

		protected T Get(K id, CacheRetrieveHandler<T> retrieve)
		{
			return Context.Services.Cache.Get(CacheKey, id.ToString(), retrieve);
		}

		protected void Clear()
		{
			Context.Services.Cache.Clear(CacheKey);
		}

		protected T Get(K id)
		{
			return Context.Services.Cache.Get<T>(CacheKey, id.ToString());
		}

		protected T Get(Func<dynamic, bool> predicate)
		{
			return Context.Services.Cache.Get<T>(CacheKey, predicate);
		}

		protected T Get(Func<dynamic, bool> predicate, CacheRetrieveHandler<T> retrieve)
		{
			return Context.Services.Cache.Get<T>(CacheKey, predicate, retrieve);
		}

		protected T First()
		{
			return Context.Services.Cache.First<T>(CacheKey);
		}

		protected List<T> Where(Func<dynamic, bool> predicate)
		{
			return Context.Services.Cache.Where<T>(CacheKey, predicate);
		}

		protected T Set(K id, T instance)
		{
			return Set(id, instance, Expiration, SlidingExpiration);
		}

		protected T Set(K id, T instance, TimeSpan duration)
		{
			return Context.Services.Cache.Set(CacheKey, id.ToString(), instance, duration);
		}

		protected T Set(K id, T instance, TimeSpan duration, bool slidingExpiration)
		{
			return Context.Services.Cache.Set(CacheKey, id.ToString(), instance, duration, slidingExpiration);
		}
		protected void Remove(K id)
		{
			Context.Services.Cache.Remove(CacheKey, new List<string> { id.ToString() });
		}

		protected int Count()
		{
			return Context.Services.Cache.Count(CacheKey);
		}

		protected string GenerateKey(params object[] parameters)
		{
			return Context.Services.Cache.GenerateKey(parameters);
		}

		protected string GenerateRandomKey()
		{
			return Context.Services.Cache.GenerateRandomKey(CacheKey);
		}

		protected void Refresh(K id)
		{
			Context.Tenant.GetService<IDataCachingService>().Invalidate(CacheKey, new List<string> { id.ToString() });
		}

		protected void Refresh(List<K> ids)
		{
			var items = new List<string>();

			foreach (var i in ids)
				items.Add(i.ToString());

			Context.Tenant.GetService<IDataCachingService>().Invalidate(CacheKey, items);
		}

		public virtual void Invalidate(K id, bool remove = false)
		{
			OnInvalidate(id, remove);
		}

		protected virtual void OnInvalidate(K id, bool remove)
		{
			if (remove)
				Remove(id);
			else
				Refresh(id);
		}

		void IDataCachingHandler.Invalidate(string id)
		{
			var result = OnInvalidating(Types.Convert<K>(id));

			if (result == default)
				return;

			Set(Types.Convert<K>(id), result, Expiration, SlidingExpiration);
		}

		void IDataCachingHandler.Initialize()
		{
			OnInitializing();
		}

		protected virtual T OnInvalidating(K id)
		{
			return default;
		}

		protected virtual void OnInitializing()
		{

		}

		protected void Reset()
		{
			Context.Services.Cache.Reset(CacheKey);
		}
	}
}
