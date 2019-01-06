using System;
using System.Collections.Generic;
using System.Globalization;

namespace TomPIT.Caching
{
	public abstract class StatefulCacheRepository<T, K> : CacheRepository<T, K> where T : class
	{
		public event CacheInvalidateHandler Invalidate;

		protected StatefulCacheRepository(IMemoryCache container, string key) : base(container, key)
		{
			Container.Invalidate += OnInvalidate;
		}

		private void OnInvalidate(CacheEventArgs e)
		{
			if (string.Compare(e.Key, Key, false) == 0)
			{
				if (Initialized)
					OnInvalidate(Types.Convert<K>(e.Id, CultureInfo.InvariantCulture));

				Invalidate?.Invoke(e);
			}
		}

		protected virtual void OnInvalidate(K id)
		{

		}

		protected virtual void OnInitializing()
		{

		}

		protected void Initialize()
		{
			if (Initialized)
				return;

			OnInitializing();

			Initialized = true;
		}

		private bool Initialized { get; set; }

		protected override List<T> All()
		{
			Initialize();

			return base.All();
		}

		protected override T First()
		{
			Initialize();

			return base.First();
		}

		protected override T Get(Func<T, bool> predicate)
		{
			Initialize();

			return base.Get(predicate);
		}

		protected override T Get(K id)
		{
			Initialize();

			return base.Get(id);
		}

		protected override T Get(K id, CacheRetrieveHandler<T> retrieve)
		{
			Initialize();

			return base.Get(id, retrieve);
		}

		protected override List<T> Where(Func<T, bool> predicate)
		{
			Initialize();

			return base.Where(predicate);
		}
	}
}