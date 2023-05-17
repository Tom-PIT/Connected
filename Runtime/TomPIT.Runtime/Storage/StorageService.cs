using LZ4;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Storage
{
	internal class StorageService : ClientRepository<IBlob, Guid>, IStorageService, IStorageNotification
	{
		public event BlobChangedHandler BlobChanged;
		public event BlobChangedHandler BlobRemoved;
		public event BlobChangedHandler BlobAdded;
		public event BlobChangedHandler BlobCommitted;

		public StorageService(ITenant tenant) : base(tenant, "blob")
		{
			BlobContent = new BlobContentCache(Tenant);
			Tenant.GetService<IMicroServiceService>().MicroServiceInstalled += OnMicroServiceInstalled;
			PreloadCache = new();
		}

		private BlobContentCache BlobContent { get; } = null;
		private ConcurrentDictionary<Guid, HashSet<int>> PreloadCache { get; }

		private void OnMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			var blobs = All();

			foreach (var i in blobs)
			{
				if (i.MicroService == e.MicroService)
				{
					Remove(i.Token);
					BlobContent.Delete(i.Token);
				}
			}
		}

		public void Commit(string draft, string primaryKey)
		{
			Instance.SysProxy.Storage.Commit(draft, primaryKey);
			Remove(f => string.Equals(f.Draft, draft, StringComparison.OrdinalIgnoreCase) && string.Equals(f.PrimaryKey, primaryKey, StringComparison.OrdinalIgnoreCase));
		}

		public void Delete(Guid blob)
		{
			Instance.SysProxy.Storage.Delete(blob);
			Remove(blob);
		}

		public IBlobContent Download(Guid blob)
		{
			if (BlobContent.TrySelect(blob, out IBlobContent result))
				return result;

			var b = Select(blob);

			if (b is null)
				return null;

			return BlobContent.Select(b);
		}

		public IBlobContent Download(Guid microService, int type, Guid resourceGroup, string primaryKey, string topic)
		{
			var r = Get(f => f.MicroService == microService
				&& f.Type == type
				&& string.Equals(f.PrimaryKey, primaryKey, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(f.Topic, topic, StringComparison.OrdinalIgnoreCase));

			if (r is null)
			{
				var rs = Query(microService, type, resourceGroup, primaryKey, topic);

				if (rs is null || !rs.Any())
					return null;

				r = rs[0];
			}

			return Download(r.Token);
		}

		public IBlobContent Download(Guid microService, int type, Guid resourceGroup, string primaryKey)
		{
			var r = Get(f => f.MicroService == microService && f.Type == type && string.Equals(f.PrimaryKey, primaryKey, StringComparison.OrdinalIgnoreCase));

			if (r is null)
			{
				var rs = Query(microService, type, resourceGroup, primaryKey);

				if (rs is null || !rs.Any())
					return null;

				r = rs[0];
			}

			return Download(r.Token);
		}

		public List<IBlobContent> Download(List<Guid> blobs)
		{
			return BlobContent.Query(blobs);
		}

		public List<IBlob> Query(Guid microService, int type, Guid resourceGroup, string primaryKey, string topic)
		{
			return Instance.SysProxy.Storage.Query(microService, type, resourceGroup, primaryKey, topic).ToList();
		}

		public List<IBlob> Query(Guid microService, int type, Guid resourceGroup, string primaryKey)
		{
			return Instance.SysProxy.Storage.Query(microService, type, resourceGroup, primaryKey).ToList();
		}

		public List<IBlob> Query(Guid microService)
		{
			return Instance.SysProxy.Storage.Query(microService).ToList();
		}

		public List<IBlob> QueryDrafts(string draft)
		{
			return Instance.SysProxy.Storage.QueryDrafts(draft).ToList();
		}

		public IBlob Select(Guid blob)
		{
			return Get(blob,
				(f) =>
				{
					return Instance.SysProxy.Storage.Select(blob);
				});
		}

		public Guid Upload(IBlob blob, byte[] content, StoragePolicy policy)
		{
			return Upload(blob, content, policy, Guid.Empty);
		}

		public Guid Upload(IBlob blob, byte[] content, StoragePolicy policy, Guid token)
		{
			var compressed = content is null ? null : LZ4Codec.Wrap(content);
			var r = Instance.SysProxy.Storage.Upload(blob, compressed, policy, token);

			Remove(r);
			BlobContent.Delete(r);

			return r;
		}

		public void Restore(IBlob blob, byte[] content)
		{
			var compressed = content is null ? null : LZ4Codec.Wrap(content);
			Instance.SysProxy.Storage.Restore(blob, compressed);

			NotifyChanged(this, new BlobEventArgs
			{
				Blob = blob.Token,
				MicroService = blob.MicroService,
				PrimaryKey = blob.PrimaryKey,
				Type = blob.Type
			});
		}

		public void NotifyChanged(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobChanged?.Invoke(Tenant, e);
		}

		public void NotifyRemoved(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobRemoved?.Invoke(Tenant, e);
		}

		public void NotifyAdded(object sender, BlobEventArgs e)
		{
			BlobAdded?.Invoke(Tenant, e);
		}

		public void NotifyCommitted(object sender, BlobEventArgs e)
		{
			Remove(e.Blob);
			BlobContent.Delete(e.Blob);

			BlobCommitted?.Invoke(Tenant, e);
		}

		public ImmutableList<Guid> Preload(int type, Guid microService)
		{
			lock (PreloadCache)
			{
				if (PreloadCache.TryGetValue(microService, out HashSet<int> items))
				{
					if (items.Contains(type))
						return Query(type, microService);
				}

				if (microService != Guid.Empty && PreloadCache.TryGetValue(microService, out items))
				{
					if (items.Contains(type))
						return Query(type, microService);
				}

				if (items == null)
				{
					items = new HashSet<int>();

					PreloadCache.TryAdd(microService, items);
				}

				var blobs = Instance.SysProxy.Storage.Query(type, microService);

				foreach (var blob in blobs)
					Set(blob.Token, blob, TimeSpan.Zero);

				items.Add(type);

				return Query(type, microService);
			}
		}

		private ImmutableList<Guid> Query(int type, Guid microService)
		{
			if (microService == Guid.Empty)
				return Where(f => f.Type == type).Select(f => f.Token).ToImmutableList();
			else
				return Where(f => f.MicroService == microService && f.Type == type).Select(f => f.Token).ToImmutableList();
		}

		public ImmutableList<Guid> Preload(int type)
		{
			return Preload(type, Guid.Empty);
		}

		public void Release(Guid blob)
		{
			Remove(blob);
			BlobContent.Delete(blob);
		}
	}
}
