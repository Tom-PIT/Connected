using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	public abstract class Cache<T, K> : DataModel, IDataCachingHandler where T : class
	{
		protected Cache(IMiddlewareContext context, string cacheKey) : base(context)
		{
			CacheKey = cacheKey;

			Context.Tenant.GetService<IDataCachingService>().RegisterHandler(CacheKey, this);
		}

		private string CacheKey { get; }
		protected virtual TimeSpan Expiration => TimeSpan.Zero;
		protected virtual bool SlidingExpiration => true;

		protected bool Exists()
		{
			return Context.Tenant.GetService<IDataCachingService>().Exists(CacheKey);
		}

		protected bool IsEmpty()
		{
			return Context.Tenant.GetService<IDataCachingService>().IsEmpty(CacheKey);
		}

		protected void CreateKey()
		{
			Context.Tenant.GetService<IDataCachingService>().CreateKey(CacheKey);
		}

		protected List<T> All()
		{
			return Context.Tenant.GetService<IDataCachingService>().All<T>(CacheKey);
		}

		protected T Get(K id, CacheRetrieveHandler<T> retrieve)
		{
			return Context.Tenant.GetService<IDataCachingService>().Get(CacheKey, id.ToString(), retrieve);
		}

		protected void Clear()
		{
			Context.Tenant.GetService<IDataCachingService>().Clear(CacheKey);
		}

		protected T Get(K id)
		{
			return Context.Tenant.GetService<IDataCachingService>().Get<T>(CacheKey, id.ToString());
		}

		protected T Get(Func<T, bool> predicate)
		{
			return Context.Tenant.GetService<IDataCachingService>().Get(CacheKey, predicate);
		}

		protected T First()
		{
			return Context.Tenant.GetService<IDataCachingService>().First<T>(CacheKey);
		}

		protected List<T> Where(Func<T, bool> predicate)
		{
			return Context.Tenant.GetService<IDataCachingService>().Where(CacheKey, predicate);
		}

		protected T Set(K id, T instance)
		{
			return Set(id, instance, Expiration, SlidingExpiration);
		}

		protected T Set(K id, T instance, TimeSpan duration)
		{
			return Context.Tenant.GetService<IDataCachingService>().Set(CacheKey, id.ToString(), instance, duration);
		}

		protected T Set(K id, T instance, TimeSpan duration, bool slidingExpiration)
		{
			return Context.Tenant.GetService<IDataCachingService>().Set(CacheKey, id.ToString(), instance, duration, slidingExpiration);
		}
		protected void Remove(K id)
		{
			Context.Tenant.GetService<IDataCachingService>().Remove(CacheKey, new List<string> { id.ToString() });
		}

		protected int Count()
		{
			return Context.Tenant.GetService<IDataCachingService>().Count(CacheKey);
		}

		protected string GenerateKey(params object[] parameters)
		{
			return Context.Tenant.GetService<IDataCachingService>().GenerateKey(parameters);
		}

		protected string GenerateRandomKey()
		{
			return Context.Tenant.GetService<IDataCachingService>().GenerateRandomKey(CacheKey);
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
			Context.Tenant.GetService<IDataCachingService>().Reset(CacheKey);
		}
	}
}
