using System;
using System.Collections.Generic;
using System.Globalization;

namespace TomPIT.Caching
{
	public abstract class SynchronizedRepository<T, K> : CacheRepository<T, K> where T : class
	{
		private object _sync = new object();
		public event CacheInvalidateHandler Invalidate;

		protected SynchronizedRepository(IMemoryCache container, string key) : base(container, key)
		{
			/*
			 * Hook to the second event in the invalidation process
			 * - Invalidating
			 * - Invalidate
			 * - Invalidated
			 */
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

			lock (_sync)
			{
				if (Initialized)
					return;

				Initializing = true;

				OnInitializing();

				Initialized = true;
				Initializing = false;
			}
		}

		private bool Initializing { get; set; }
		private bool Initialized { get; set; }

		protected override List<T> All()
		{
			if (!Initializing)
				Initialize();

			return base.All();
		}

		protected override T First()
		{
			if (!Initializing)
				Initialize();

			return base.First();
		}

		protected override T Get(Func<T, bool> predicate)
		{
			if (!Initializing)
				Initialize();

			return base.Get(predicate);
		}

		protected override T Get(K id)
		{
			if (!Initializing)
				Initialize();

			return base.Get(id);
		}

		protected override T Get(K id, CacheRetrieveHandler<T> retrieve)
		{
			if (!Initializing)
				Initialize();

			return base.Get(id, retrieve);
		}

		protected override List<T> Where(Func<T, bool> predicate)
		{
			if (!Initializing)
				Initialize();

			return base.Where(predicate);
		}
	}
}