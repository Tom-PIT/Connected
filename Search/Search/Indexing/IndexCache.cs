using System;
using System.Collections.Concurrent;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Search.Catalogs;

namespace TomPIT.Search.Indexing
{
	internal static class IndexCache
	{
		private static Lazy<ConcurrentDictionary<Guid, CatalogHost>> _items = new Lazy<ConcurrentDictionary<Guid, CatalogHost>>();
		private static readonly object sync = new object();

		public static CatalogHost Ensure(Guid catalogId)
		{
			var c = TryGet(catalogId);

			if (c == null)
			{
				lock (sync)
				{
					c = TryGet(catalogId);

					if (c == null)
						c = Set(catalogId, TimeSpan.FromMinutes(30), true);
				}
			}

			return c;
		}

		public static void Flush()
		{
			foreach (var i in Items)
				i.Value.Flush();
		}

		public static void Scave()
		{
			var dead = Items.Where(f => f.Value.Expired || !f.Value.IsValid);

			foreach (var i in dead)
				Remove(i.Key);
		}

		public static CatalogHost Set(Guid catalog, TimeSpan duration, bool slidingExpiration)
		{
			CatalogHost r = null;

			if (Items.ContainsKey(catalog))
				Items.TryGetValue(catalog, out r);

			if (r == null)
			{
				if (!(Instance.Tenant.GetService<IComponentService>().SelectConfiguration(catalog) is ISearchCatalogConfiguration cat))
					return null;

				r = new CatalogHost(cat, duration, slidingExpiration);

				if (!Items.TryAdd(catalog, r))
					return null;
			}

			return r;
		}

		public static bool Exists(Guid catalog)
		{
			return Items.ContainsKey(catalog);
		}

		public static void Clear()
		{
			if (Items.Count == 0)
				return;

			foreach (var i in Items.Keys)
				Remove(i);
		}

		public static CatalogHost TryGet(Guid catalog)
		{
			if (!Exists(catalog))
				return null;

			if (!Items.TryGetValue(catalog, out CatalogHost d))
				return null;

			return d;
		}

		private static ConcurrentDictionary<Guid, CatalogHost> Items => _items.Value;


		public static void Remove(Guid key)
		{
			if (Items.TryRemove(key, out CatalogHost host))
			{
				if (!host.IsDisposing && !host.IsDisposed)
					host.Dispose();
			}
		}
	}
}