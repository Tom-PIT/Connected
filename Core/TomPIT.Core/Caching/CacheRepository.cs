using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;

namespace TomPIT.Caching
{
	public abstract class CacheRepository<T, K> where T : class
	{
		protected CacheRepository(IMemoryCache container, string key)
		{
			Container = container;
			Key = key;
		}

		public string Key { get; }

		protected void Remove(K id)
		{
			Container.Remove(Key, Types.Convert<string>(id, CultureInfo.InvariantCulture));
		}

		protected void Remove(Func<T, bool> predicate)
		{
			Container.Remove(Key, predicate);
		}

		protected void Refresh(K id)
		{
			Container.Refresh(Key, Types.Convert<string>(id, CultureInfo.InvariantCulture));
		}

		protected IMemoryCache Container { get; }

		public int Count { get { return Container.Count(Key); } }

		protected virtual ICollection<string> Keys()
		{
			return Container.Keys(Key);
		}

		protected virtual ImmutableList<T> All()
		{
			return Container.All<T>(Key);
		}

		protected virtual T Get(K id, CacheRetrieveHandler<T> retrieve)
		{
			return Container.Get(Key, Types.Convert<string>(id, CultureInfo.InvariantCulture), retrieve);
		}

		protected virtual T Get(K id)
		{
			return Container.Get<T>(Key, Types.Convert<string>(id, CultureInfo.InvariantCulture));
		}

		protected virtual T First()
		{
			return Container.First<T>(Key);
		}

		protected virtual T Get(Func<T, bool> predicate)
		{
			return Container.Get(Key, predicate);
		}

		protected virtual ImmutableList<T> Where(Func<T, bool> predicate)
		{
			return Container.Where(Key, predicate);
		}

		protected void Set(K id, T instance)
		{
			Container.Set(Key, Types.Convert<string>(id, CultureInfo.InvariantCulture), instance);
		}

		protected void Set(K id, T instance, TimeSpan duration)
		{
			Container.Set(Key, Types.Convert<string>(id, CultureInfo.InvariantCulture), instance, duration);
		}

		protected string GenerateKey(params object[] parameters)
		{
			return Container.GenerateKey(parameters);
		}

		protected string GenerateRandomKey()
		{
			return Container.GenerateRandomKey(Key);
		}
	}
}
