using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Caching;
using TomPIT.Compilation;
using TomPIT.ComponentModel.Apis;
using TomPIT.Connectivity;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT.Data
{
	public abstract class Cache<T, K> : DataModel, IDataCachingHandler where T : class
	{
		protected Cache(IDataModelContext context, string cacheKey) : base(context)
		{
			CacheKey = cacheKey;

			Connection.GetService<IDataCachingService>().RegisterHandler(CacheKey, this);
		}

		private string CacheKey { get; }
		protected virtual TimeSpan Expiration => TimeSpan.Zero;
		protected virtual bool SlidingExpiration => true;

		protected bool Exists()
		{
			return Connection.GetService<IDataCachingService>().Exists(CacheKey);
		}

		protected bool IsEmpty()
		{
			return Connection.GetService<IDataCachingService>().IsEmpty(CacheKey);
		}

		protected void CreateKey()
		{
			Connection.GetService<IDataCachingService>().CreateKey(CacheKey);
		}

		protected List<T> All()
		{
			return Connection.GetService<IDataCachingService>().All<T>(CacheKey);
		}

		protected T Get(K id, CacheRetrieveHandler<T> retrieve)
		{
			return Connection.GetService<IDataCachingService>().Get(CacheKey, id.ToString(), retrieve);
		}

		protected void Clear()
		{
			Connection.GetService<IDataCachingService>().Clear(CacheKey);
		}

		protected T Get(K id)
		{
			return Connection.GetService<IDataCachingService>().Get<T>(CacheKey, id.ToString());
		}

		protected T Get(Func<T, bool> predicate)
		{
			return Connection.GetService<IDataCachingService>().Get(CacheKey, predicate);
		}

		protected T First()
		{
			return Connection.GetService<IDataCachingService>().First<T>(CacheKey);
		}

		protected List<T> Where(Func<T, bool> predicate)
		{
			return Connection.GetService<IDataCachingService>().Where(CacheKey, predicate);
		}

		protected T Set(K id, T instance)
		{
			return Set(id, instance, Expiration, SlidingExpiration);
		}

		protected T Set(K id, T instance, TimeSpan duration)
		{
			return Connection.GetService<IDataCachingService>().Set(CacheKey, id.ToString(), instance, duration);
		}

		protected T Set(K id, T instance, TimeSpan duration, bool slidingExpiration)
		{
			return Connection.GetService<IDataCachingService>().Set(CacheKey, id.ToString(), instance, duration, slidingExpiration);
		}
		protected void Remove(K id)
		{
			Connection.GetService<IDataCachingService>().Remove(CacheKey, new List<string> { id.ToString() });
		}

		protected int Count()
		{
			return Connection.GetService<IDataCachingService>().Count(CacheKey);
		}

		protected string GenerateKey(params object[] parameters)
		{
			return Connection.GetService<IDataCachingService>().GenerateKey(parameters);
		}

		protected string GenerateRandomKey()
		{
			return Connection.GetService<IDataCachingService>().GenerateRandomKey(CacheKey);
		}

		protected void Refresh(K id)
		{
			Connection.GetService<IDataCachingService>().Invalidate(CacheKey, new List<string> { id.ToString() });
		}

		protected void Refresh(List<K> ids)
		{
			var items = new List<string>();

			foreach (var i in ids)
				items.Add(i.ToString());

			Connection.GetService<IDataCachingService>().Invalidate(CacheKey, items);
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
			var result =  OnInvalidating(Types.Convert<K>(id));

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

		private ISysConnection Connection => ((ExecutionContext)Context).Connection;

		protected void Reset()
		{
			Connection.GetService<IDataCachingService>().Reset(CacheKey);
		}
	}
}
